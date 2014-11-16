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

	private TcpListener _tcpClientsListener 		    	= null;
	private Thread _listenClientsThread 			    	= null;
	private List<TcpClient> _clients        		    	= new List<TcpClient>();
	private IPEndPoint _serverEndPoint				    	= null;
	public System.Action<TcpClient> OnClientConnected 	  	= null;
	public System.Action<TcpClient, byte[]> OnClientMessage 	= null;

	#endregion

	#region Properties

	public IPEndPoint ServerEndPoint
	{
		get { return _serverEndPoint; }
	}
	
	#endregion

	#region Constructors
	
	public SocketServer()
	{
	}

	#endregion

	#region Methods

	public void StartServer(IPAddress ip, int port)
	{
		_serverEndPoint 	 = new IPEndPoint(ip, port);
		_tcpClientsListener  = new TcpListener(_serverEndPoint);
		_listenClientsThread = new Thread(new ThreadStart(ProcessIncomingClientsThread));
		_listenClientsThread.Start();
	}

	public void StopServer()
	{
		_serverEndPoint = null;
		_listenClientsThread.Abort();
		_listenClientsThread = null;
		_tcpClientsListener.Stop();
		_tcpClientsListener = null;
	}

	private void ProcessIncomingClientsThread()
	{
		_tcpClientsListener.Start();
		
		while(_tcpClientsListener != null)
		{
			//blocks until a client has connected to the server
			TcpClient client = _tcpClientsListener.AcceptTcpClient();
			
			//create a thread to handle communication 
			//with connected client
			Thread clientThread = new Thread(new ParameterizedThreadStart(ProcessClientMessagesThread));
			clientThread.Start(client);

			_clients.Add(client);

			NotifyOnClientConnected(client);
		}
	}

	private void NotifyOnClientConnected(TcpClient client)
	{
		if(OnClientConnected != null)
			OnClientConnected(client);
	}

	private void ProcessClientMessagesThread(object client)
	{
		TcpClient tcpClient		   = (TcpClient)client;
		NetworkStream clientStream = tcpClient.GetStream();

		while(true)
		{
			byte[] bytes = NetworkUtils.ReadBytesFromClient(tcpClient);

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

							NotifyOnClientMessage(tcpClient, message);
						}
					}
				}
			}
		}
	}

	private void NotifyOnClientMessage(TcpClient client, byte[] message)
	{
		if(OnClientMessage != null)
			OnClientMessage(client, message);
	}

	public void SendMessageToClients(string message)
	{
		byte[] bytes = NetworkUtils.GetMessageBytes(message);
		SendMessageToClients(bytes);
	}

	public void SendMessageToClients(byte[] message)
	{
		foreach(TcpClient client in _clients)
			SendMessageToClient(message, client);
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


	#endregion
}
