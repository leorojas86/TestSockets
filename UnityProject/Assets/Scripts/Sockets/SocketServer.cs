using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

public class SocketServer : MonoBehaviour
{
	#region Variables

	private TcpListener _tcpClientsListener 		    = null;
	private Thread _listenIncomingClientsThread 		= null;
	private List<TcpClient> _clients        		    = new List<TcpClient>();
	private List<TcpClient> _recentlyConnectedClients   = new List<TcpClient>();
	private IPEndPoint _serverEndPoint				    = null;

	private System.Action<TcpClient> _onClientConnected  = null;
	public System.Action<SocketMessage> OnClientMessage  = null;
	public System.Action<TcpClient> OnClientDisconnected = null;

	private bool _isStarted = false;
	
	private bool _isBroadcastingServerInfo = false;

	private SocketServerInfo _serverInfo = null;

	private IEnumerator _sendServerInfoBroadcastCoroutine = null;

	#endregion

	#region Properties

	public IPEndPoint ServerEndPoint
	{
		get { return _serverEndPoint; }
	}

	public bool IsStarted
	{
		get { return _isStarted; }
	}

	public bool IsBroadcastingServerInfo
	{
		get { return _isBroadcastingServerInfo; }
	}

	public List<TcpClient> Clients
	{
		get { return _clients; }
	}

	public SocketServerInfo ServerInfo
	{
		get { return _serverInfo; }
	}

	#endregion

	#region Methods

	public void StartServer(IPAddress ip, int port, System.Action<TcpClient> onClientConnected)
	{
		if(!_isStarted)
		{
			_isStarted = true;

			_onClientConnected 			 = onClientConnected;
			_serverEndPoint 	 		 = new IPEndPoint(ip, port);
			_tcpClientsListener  		 = new TcpListener(_serverEndPoint);
			_listenIncomingClientsThread = new Thread(new ThreadStart(ProcessIncomingClientsThread));
			_listenIncomingClientsThread.Start();
		}
		else
			LogManager.Instance.LogMessage("Can not start server twice");
	}

	public void StartSendingServerInfoBroadcast(string serverInfo)
	{
		if(!_isBroadcastingServerInfo)
		{
			if(_isStarted)
			{
				_isBroadcastingServerInfo         = true;
				_serverInfo 		      		  = new SocketServerInfo(_serverEndPoint.Address.ToString(), serverInfo);
				_sendServerInfoBroadcastCoroutine = SendServerInfoBroadcastCoroutine();
				StartCoroutine(_sendServerInfoBroadcastCoroutine);
			}
			else
				LogManager.Instance.LogMessage("Can not start broadcasting server info if the server is not started");
		}
		else
			LogManager.Instance.LogMessage("Can not start broadcasting server info twice");
	}

	public void StopServer()
	{
		if(_isStarted)
		{
			_isStarted      = false;
			_serverEndPoint = null;

			_clients.Clear();

			StopBroadcastMessages();
			StopProcessingIncomingClients();
		}
	}

	public void StopBroadcastMessages()
	{
		if(_isBroadcastingServerInfo)
		{
			StopCoroutine(_sendServerInfoBroadcastCoroutine);
			_sendServerInfoBroadcastCoroutine = null;
			_isBroadcastingServerInfo         = false;
		}
	}

	private void StopProcessingIncomingClients()//TODO: Stop processing incoming clients as soon as possible
	{
		_listenIncomingClientsThread = null;
		
		_tcpClientsListener.Stop();
		_tcpClientsListener = null;
	}

	private IEnumerator SendServerInfoBroadcastCoroutine()
	{
		Socket broadcastSocket 			= new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		broadcastSocket.EnableBroadcast = true;
		IPAddress broadcast    			= IPAddress.Parse("255.255.255.255");//Boadcast to all local network
		IPEndPoint endPoint    			= new IPEndPoint(broadcast, _serverEndPoint.Port);
		string serverInfo 				= SocketServerInfo.ToJson(_serverInfo);
		byte[] messageBytes    			= Encoding.ASCII.GetBytes(serverInfo);

		//try
		//{
			while(_isBroadcastingServerInfo)
			{
				broadcastSocket.SendTo(messageBytes, endPoint);
				yield return new WaitForSeconds(0.5f);//Sent message every 0.5 seconds
			}
		//}
		//catch(Exception e)
		//{
			//LogManager.Instance.LogMessage("Exception sending broadcast message = " + e.ToString());
		//}
	}
	
	private void ProcessIncomingClientsThread()
	{
		_tcpClientsListener.Start();
		
		while(_listenIncomingClientsThread != null)
		{
			TcpClient client = _tcpClientsListener.AcceptTcpClient();
			_recentlyConnectedClients.Add(client);
		}
	}

	void Update()
	{
		if(_recentlyConnectedClients.Count > 0)
		{
			for(int x = 0; x < _recentlyConnectedClients.Count; x++)
			{
				TcpClient currentClient = _recentlyConnectedClients[x];
				_clients.Add(currentClient);

				NotifyOnClientConnected(currentClient);

				StartCoroutine(ProcessClientMessagesCoroutine(currentClient));
			}

			_recentlyConnectedClients.Clear();
		}
	}

	private IEnumerator ProcessClientMessagesCoroutine(TcpClient client)
	{
		//try
		//{
			while(_isStarted)
			{
				//if(tcpClient.Connected)
				//{
					byte[] bytes = NetworkUtils.ReadBytesFromTCPConnection(client);

					if(bytes != null)
					{
						List<byte[]> messages = NetworkUtils.GetMessagesFromBytes(bytes);

						for(int x = 0; x < messages.Count; x++)
							NotifyOnClientMessage(client, messages[x]);
					}
				//}
				//else
					//NotifyOnClientDisconnected(tcpClient);

				yield return new WaitForEndOfFrame();
			}
		//}
		//catch(Exception e)
		//{
			//LogManager.Instance.LogMessage("Exception while processing client messages, exception = " + e.ToString());
		//}
}

	public void SendMessageToClients(string message)
	{
		byte[] bytes = NetworkUtils.GetMessageBytes(message);
		SendMessageToClients(bytes);
	}

	public void SendMessageToClients(byte[] message)
	{
		for(int x = 0; x < _clients.Count; x++)
			SendMessageToClient(message, _clients[x]);
	}

	public void SendMessageToClient(string message, TcpClient client)
	{
		byte[] bytes = NetworkUtils.GetMessageBytes(message);
		
		SendMessageToClient(bytes, client);
	}

	public void SendMessageToClient(byte[] bytes, TcpClient client)
	{
		try
		{
			SocketsManager.Instance.Log("Sending message to client = " + bytes.Length);

			NetworkUtils.SendBytesToTCPConnection(bytes, client);
		}
		catch(IOException e)
		{
			NotifyOnClientDisconnected(client);
			_clients.Remove(client);
		}
	}

	private void NotifyOnClientConnected(TcpClient client)
	{
		Debug.Log("NotifyOnClientConnected");

		if(_onClientConnected != null)
		{
			//SocketsManager.Instance.InvokeAction(_onClientConnected, client);

			//Debug.Log("SocketsManager.Instance.InvokeAction");
			_onClientConnected(client);

			Debug.Log("NotifyOnClientConnected completed");
		}
	}

	private void NotifyOnClientMessage(TcpClient client, byte[] bytes)
	{
		SocketsManager.Instance.Log("On Client Message = " + bytes.Length);

		if(OnClientMessage != null)
		{
			SocketMessage socketMessage = new SocketMessage(client, bytes);
			SocketsManager.Instance.InvokeAction(OnClientMessage, socketMessage);
			//OnClientMessage(socketMessage);
		}
	}

	private void NotifyOnClientDisconnected(TcpClient client)
	{
		if(OnClientDisconnected != null)
		{
			SocketsManager.Instance.InvokeAction(OnClientDisconnected, client);
		}
	}

	#endregion
}
