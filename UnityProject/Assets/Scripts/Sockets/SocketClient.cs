using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

public class SocketClient 
{
	#region Variables

	private TcpClient _tcpClient 	   		    = null;
	private IPEndPoint _connectedServerEndPoint = null;
	private Thread _listenServerMessagesThread  = null;

	private Thread _listenBroadcastMessagesThread = null;

	public System.Action<byte[]> OnServerMessage = null;

	private System.Action<SocketServerInfo> _onServerFound = null;
	private System.Action<SocketServerInfo> _onServerLost  = null;

	private bool _isConnected = false;

	private bool _isFindingServers = false;

	private Dictionary<string, SocketServerInfo> _foundServers = new Dictionary<string, SocketServerInfo>();

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

	public Dictionary<string, SocketServerInfo> FoundServers
	{
		get { return _foundServers; }
	}

	#endregion

	#region Constructors
	
	public SocketClient()
	{
	}

	#endregion

	#region Methods

	public void FindServers(int port, System.Action<SocketServerInfo> onServerFound, System.Action<SocketServerInfo> onServerLost)
	{
		if(!_isFindingServers)
		{
			_listenBroadcastMessagesThread = new Thread(new ParameterizedThreadStart(ListenBroadcastMessages));
			_listenBroadcastMessagesThread.Start(port);

			_isFindingServers = true;

			_onServerFound = onServerFound;
			_onServerLost  = onServerLost;
		}
	}

	public void StopFindingServers()
	{
		if(_isFindingServers)
		{
			_listenBroadcastMessagesThread = null;

			_foundServers.Clear();

			_isFindingServers = false;

			_onServerFound = null;
			_onServerLost  = null;
		}
	}

	public bool ConnectToServer(IPAddress serverAddress, int port)
	{
		if(!_isConnected)
		{
			_connectedServerEndPoint = new IPEndPoint(serverAddress, port);
			_tcpClient 				 = new TcpClient();

			try
			{
				_tcpClient.Connect(_connectedServerEndPoint);

				_listenServerMessagesThread = new Thread(new ThreadStart(ProcessServerMessagesThread));
				_listenServerMessagesThread.Start();

				_isConnected = true;

				//StopFindingServers();
				return true;
			}
			catch(Exception e)
			{
				LogManager.Instance.LogMessage("Could not connect to server at ip " + serverAddress + " using port = " + port + " exception = " + e.ToString());
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
			_connectedServerEndPoint = null;
			_tcpClient.Close();
			_tcpClient = null;
			_listenServerMessagesThread = null;

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
		NetworkUtils.SendBytesToClient(bytes, _tcpClient);
	}

	private void NotifyOnServerMessage(byte[] message)
	{
		if(OnServerMessage != null)
			OnServerMessage(message);
	}

	private void ProcessServerMessagesThread()
	{	
		while(_listenServerMessagesThread != null)
		{
			byte[] bytes = NetworkUtils.ReadBytesFromClient(_tcpClient);

			if(bytes != null)
			{
				int bytesLength = bytes.Length;
				
				using(MemoryStream memoryStream = new MemoryStream(bytes))
				{
					using(BinaryReader binaryReader = new BinaryReader(memoryStream))
					{
						//Debug.Log("using(BinaryReader binaryReader = new BinaryReader(memoryStream))");
						
						while(bytesLength > 0)
						{
							int messageLength = binaryReader.ReadInt32();//Read the message length
							bytesLength	 	 -= 4;
							byte[] message 	  = binaryReader.ReadBytes(messageLength);
							bytesLength      -= messageLength;
							
							NotifyOnServerMessage(message);
						}
					}
				}
			}
		}
	}

	private void ListenBroadcastMessages(object portParam) 
	{
		int port 		   = (int)portParam;
		UdpClient listener = new UdpClient(port);
		IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);
		
		try 
		{
			while (_listenBroadcastMessagesThread != null) 
			{
				LogManager.Instance.LogMessage("Waiting for broadcast");
				byte[] bytes = listener.Receive( ref groupEP);
				string data  = Encoding.ASCII.GetString(bytes,0,bytes.Length);

				if(data.Contains("ServerIP:"))
				{
					string ip = data.Replace("ServerIP:", string.Empty);

					if(_foundServers.ContainsKey(ip))
						;//_foundServers[ip].lastListenTime = Time.time;
					else
					{
						SocketServerInfo serverInfo = new SocketServerInfo(IPAddress.Parse(ip), data);
						_foundServers.Add(ip, serverInfo);
						NotifyOnServerFound(serverInfo);
					}
				}

				//LogManager.Instance.LogMessage("Received broadcast from " + groupEP.ToString() + " :\n " + data + "\n");
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

	private void NotifyOnServerFound(SocketServerInfo serverInfo)
	{
		if(_onServerFound != null)
			_onServerFound(serverInfo);
	}

	
	#endregion
}
