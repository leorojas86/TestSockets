﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

public class SocketClient : MonoBehaviour
{
	#region Constants

	private float LOST_SERVER_SECONDS = 4;

	#endregion

	#region Variables

	private TcpClient _tcpClient 	   		    = null;
	private IPEndPoint _connectedServerEndPoint = null;

	public System.Action<SocketMessage> OnServerMessage  = null;
	public System.Action<TcpClient> OnServerDisconnected = null;

	private System.Action<SocketServerInfo> _onServerFound = null;
	private System.Action<SocketServerInfo> _onServerLost  = null;

	private bool _isConnected = false;

	private bool _isFindingServers = false;

	private List<SocketServerInfo> _foundServers         = new List<SocketServerInfo>();
	private List<SocketServerInfo> _recentlyFoundServers = new List<SocketServerInfo>();

	private Thread _findServersThread = null;
	private IEnumerator _checkForLostServersCoroutine     = null;

	private IEnumerator _processServerMessagesCoroutine = null;

	#endregion

	#region Properties
	
	public IPEndPoint ConnectedServerEndPoint
	{
		get { return _connectedServerEndPoint; }
	}

	public bool IsConnected
	{
		get { return _isConnected; } 
	}

	public bool IsFindingServers
	{
		get { return _isFindingServers; }
	}

	public List<SocketServerInfo> FoundServers
	{
		get { return _foundServers; }
	}

	#endregion

	#region Methods

	public void FindServers(System.Action<SocketServerInfo> onServerFound, System.Action<SocketServerInfo> onServerLost)
	{
		if(!_isFindingServers)
		{
			_findServersThread = new Thread(new ParameterizedThreadStart(FindServersThread));
			_findServersThread.Start(SocketsManager.Instance.Port);
			//StartCoroutine(_listenBroadcastMessagesThread);

			_checkForLostServersCoroutine = CheckForLostServersCoroutine();
			StartCoroutine(_checkForLostServersCoroutine);

			_isFindingServers = true;

			_onServerFound = onServerFound;
			_onServerLost  = onServerLost;

			_foundServers.Clear();
		}
	}

	public void StopFindingServers()
	{
		if(_isFindingServers)
		{
			//StopCoroutine(_listenBroadcastMessagesThread);
			_findServersThread = null;

			StopCoroutine(_checkForLostServersCoroutine);
			_checkForLostServersCoroutine  = null;
				
			_foundServers.Clear();

			_isFindingServers = false;

			_onServerFound = null;
			_onServerLost  = null;
		}
	}

	public bool ConnectToServer(IPAddress serverAddress)
	{
		if(!_isConnected)
		{
			_connectedServerEndPoint = new IPEndPoint(serverAddress, SocketsManager.Instance.Port);
			_tcpClient 				 = new TcpClient();

			try
			{
				_tcpClient.Connect(_connectedServerEndPoint);

				_processServerMessagesCoroutine = ProcessServerMessagesCoroutine();

				StartCoroutine(_processServerMessagesCoroutine);

				_isConnected = true;
				return true;
			}
			catch(Exception e)
			{
				LogManager.Instance.LogMessage("Could not connect to server at ip " + serverAddress + " using port = " + _connectedServerEndPoint.Port + " exception = " + e.ToString());
			}
		}
		else
			LogManager.Instance.LogMessage("Can not connect to server twice");

		return false;
	}

	public void Disconnect()
	{
		if(_isConnected)
		{
			StopCoroutine(_processServerMessagesCoroutine);
			_processServerMessagesCoroutine = null;

			_connectedServerEndPoint = null;

			_tcpClient.Close();
			_tcpClient = null;

			_isConnected = false;
		}
	}

	public void SendMessageToServer(string message)
	{
		byte[] bytes = NetworkUtils.GetMessageBytes(message);

		SendMessageToServer(bytes);
	}

	public void SendMessageToServer(byte[] bytes)
	{
		if(_tcpClient != null)
		{
			try
			{
				SocketsManager.Instance.Log("Sending message to server = " + bytes.Length);
				NetworkUtils.SendBytesToTCPConnection(bytes, _tcpClient);
			}
			catch(IOException e)
			{
				NotifyOnServerDisconnected();
				Disconnect();
			}
		}
		else
			Debug.LogError("Can not send message to server, client is not connected to server");
	}

	private IEnumerator ProcessServerMessagesCoroutine()
	{	
		//try
		//{
			while(_processServerMessagesCoroutine != null)
			{
				byte[] bytes = NetworkUtils.ReadBytesFromTCPConnection(_tcpClient);

				if(bytes != null)
				{
					List<byte[]> messages = NetworkUtils.GetMessagesFromBytes(bytes);

					for(int x = 0; x < messages.Count; x++)
						NotifyOnServerMessage(_tcpClient, messages[x]);
				}

				yield return new WaitForEndOfFrame();
			}
		//}
		//catch(Exception e)
		//{
			//LogManager.Instance.LogMessage("Exception while processing server messages, exception = " + e.ToString());
		//}
	}

	private void FindServersThread(object portObject) 
	{
		int port 		   = (int)portObject;
		UdpClient listener = new UdpClient(port);
		IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);
		
		try 
		{
			while(_findServersThread != null) 
			{
				LogManager.Instance.LogMessage("Waiting for broadcast");
				byte[] bytes 				= listener.Receive(ref groupEP);
				string data  				= Encoding.ASCII.GetString(bytes, 0, bytes.Length);
				SocketServerInfo serverInfo = SocketServerInfo.FromJson(data);

				if(serverInfo != null)
				{
					SocketServerInfo existentServerInfo = FindServerInfo(serverInfo.ip);

					if(existentServerInfo != null)
						existentServerInfo.lastListenTime = System.DateTime.Now;
					else
					{
						_foundServers.Add(serverInfo);
						_recentlyFoundServers.Add(serverInfo);
					}
				}
			}	
		} 
		catch (Exception e) 
		{
			LogManager.Instance.LogMessage("ListenBroadcastMessages exception = " + e.ToString());
		}
		finally
		{
			listener.Close();
		}
	}

	void Update()
	{
		if(_recentlyFoundServers.Count > 0)
		{
			for(int x = 0; x < _recentlyFoundServers.Count; x++)
			{
				SocketServerInfo currentServerInfo = _recentlyFoundServers[x];
				NotifyOnServerFound(currentServerInfo);
			}

			_recentlyFoundServers.Clear();
		}
	}

	private SocketServerInfo FindServerInfo(string ip)
	{
		for(int x = 0; x < _foundServers.Count; x++)
		{
			SocketServerInfo serverInfo = _foundServers[x];

			if(serverInfo.ip == ip)
				return serverInfo;
		}

		return null;
	}

	private IEnumerator CheckForLostServersCoroutine()
	{
		while(_checkForLostServersCoroutine != null)
		{
			List<SocketServerInfo> lostServers = new List<SocketServerInfo>();

			for(int x = 0; x < _foundServers.Count; x++)
			{
				SocketServerInfo serverInfo = _foundServers[x];

				System.TimeSpan timeSpan = System.DateTime.Now - serverInfo.lastListenTime;

				if(timeSpan.Seconds > LOST_SERVER_SECONDS)
					lostServers.Add(serverInfo);
			}

			for(int x = 0; x < lostServers.Count; x++)
			{
				SocketServerInfo lostServer = lostServers[x];
				NotifyOnServerLost(lostServer);
				_foundServers.Remove(lostServer);
			}

			yield return new WaitForSeconds(1);//Wait for 1 second
		}
	}

	private void NotifyOnServerFound(SocketServerInfo serverInfo)
	{
		if(_onServerFound != null)
			_onServerFound(serverInfo);
	}

	private void NotifyOnServerLost(SocketServerInfo serverInfo)
	{
		if(_onServerLost != null)
			_onServerLost(serverInfo);
	}

	private void NotifyOnServerMessage(TcpClient sender, byte[] bytes)
	{
		SocketsManager.Instance.Log("On Server Message = " + bytes.Length);

		if(OnServerMessage != null)
		{
			SocketMessage socketMessage = new SocketMessage(sender, bytes);
			OnServerMessage(socketMessage);
		}
	}

	private void NotifyOnServerDisconnected()
	{
		if(OnServerDisconnected != null)
			OnServerDisconnected(_tcpClient);
	}

	void OnDestroy()
	{
		Disconnect();
	}
	
	#endregion
}
