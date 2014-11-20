using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

public class SocketServer
{
	#region Variables

	private TcpListener _tcpClientsListener 		    = null;
	private Thread _listenIncomingClientsThread 		= null;
	private Thread _sendServerInfoBroadcastThread		= null;
	private List<TcpClient> _clients        		    = new List<TcpClient>();
	private IPEndPoint _serverEndPoint				    = null;

	private System.Action<TcpClient> _onClientConnected  = null;
	public System.Action<SocketMessage> OnClientMessage  = null;
	public System.Action<TcpClient> OnClientDisconnected = null;

	private bool _isStarted = false;
	
	private bool _isBroadcastingServerInfo = false;

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

	#endregion

	#region Constructors
	
	public SocketServer()
	{
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

	public void StartSendingServerInfoBroadcast()
	{
		if(!_isBroadcastingServerInfo)
		{
			if(_isStarted)
			{
				_sendServerInfoBroadcastThread = new Thread(new ThreadStart(SendServerInfoBroadcast));
				_sendServerInfoBroadcastThread.Start();

				_isBroadcastingServerInfo = true;
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
			_serverEndPoint = null;
			_listenIncomingClientsThread = null;
			_tcpClientsListener.Stop();
			_tcpClientsListener = null;

			_clients.Clear();

			StopBroadcastMessages();

			_isStarted = false;
		}
	}

	public void StopBroadcastMessages()
	{
		if(_isBroadcastingServerInfo)
		{
			_sendServerInfoBroadcastThread = null;
			_isBroadcastingServerInfo      = false;
		}
	}

	private void SendServerInfoBroadcast()
	{
		Socket broadcastSocket 			= new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		broadcastSocket.EnableBroadcast = true;
		IPAddress broadcast    			= IPAddress.Parse("255.255.255.255");//Boadcast to all local network
		IPEndPoint endPoint    			= new IPEndPoint(broadcast, _serverEndPoint.Port);
		byte[] messageBytes    			= Encoding.ASCII.GetBytes("ServerIP:" + _serverEndPoint.Address);

		try
		{
			while(_sendServerInfoBroadcastThread != null)
			{
				broadcastSocket.SendTo(messageBytes, endPoint);
				Thread.Sleep(500);//Sent message every 0.5 seconds

				//LogManager.Instance.LogMessage("Sending broadcast message");
			}
		}
		catch(Exception e)
		{
			LogManager.Instance.LogMessage("Exception sending broadcast message = " + e.ToString());
		}
	}
	
	private void ProcessIncomingClientsThread()
	{
		_tcpClientsListener.Start();
		
		while(_listenIncomingClientsThread != null)
		{
			//blocks until a client has connected to the server
			TcpClient client = _tcpClientsListener.AcceptTcpClient();
			
			//create a thread to handle communication 
			//with connected client
			Thread listenClientMessagesThread = new Thread(new ParameterizedThreadStart(ProcessClientMessagesThread));
			listenClientMessagesThread.Start(client);

			_clients.Add(client);

			NotifyOnClientConnected(client);
		}
	}

	private void ProcessClientMessagesThread(object client)
	{
		try
		{
			TcpClient tcpClient = (TcpClient)client;

			while(_isStarted)
			{
				if(tcpClient.Connected)
				{
					byte[] bytes = NetworkUtils.ReadBytesFromClient(tcpClient);

					if(bytes != null)
					{
						List<byte[]> messages = NetworkUtils.GetMessagesFromBytes(bytes);

						for(int x = 0; x < messages.Count; x++)
							NotifyOnClientMessage(tcpClient, messages[x]);
					}
				}
				else
					NotifyOnClientDisconnected(tcpClient);
			}
		}
		catch(Exception e)
		{
			LogManager.Instance.LogMessage("Exception while processing client messages, exception = " + e.ToString());
		}
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
		byte[] bytes = NetworkUtils.GetMessageBytes(message);;
		
		SendMessageToClient(bytes, client);
	}

	public void SendMessageToClient(byte[] message, TcpClient client)
	{
		NetworkUtils.SendBytesToClient(message, client);
	}

	private void NotifyOnClientConnected(TcpClient client)
	{
		if(_onClientConnected != null)
		{
			SocketsManager.Instance.InvokeAction(_onClientConnected, client);
			//_onClientConnected(client);
		}
	}

	private void NotifyOnClientMessage(TcpClient client, byte[] message)
	{
		if(OnClientMessage != null)
		{
			SocketMessage socketMessage = new SocketMessage(client, message);
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
