using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

public class SocketServer
{
	#region Variables

	private TcpListener _tcpClientsListener = null;
	private Thread _listenClientsThread 	= null;
	private List<TcpClient> _clients        = new List<TcpClient>();
	private IPEndPoint _serverEndPoint		= null;

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
		_listenClientsThread = new Thread(new ThreadStart(ListenForClients));
		_listenClientsThread.Start();
	}

	private void ListenForClients()
	{
		_tcpClientsListener.Start();
		
		while (true)
		{
			//blocks until a client has connected to the server
			TcpClient client = _tcpClientsListener.AcceptTcpClient();
			
			//create a thread to handle communication 
			//with connected client
			Thread clientThread = new Thread(new ParameterizedThreadStart(OnClientMessage));
			clientThread.Start(client);

			_clients.Add(client);
		}
	}

	private void OnClientMessage(object client)
	{
		TcpClient tcpClient 	   = (TcpClient)client;
		NetworkStream clientStream = tcpClient.GetStream();
		
		byte[] message = new byte[4096];
		int bytesRead;
		
		while (true)
		{
			bytesRead = 0;
			
			try
			{
				//blocks until a client sends a message
				bytesRead = clientStream.Read(message, 0, 4096);
			}
			catch
			{
				//a socket error has occured
				break;
			}
			
			if (bytesRead == 0)
			{
				//the client has disconnected from the server
				break;
			}
			
			//message has successfully been received
			ASCIIEncoding encoder = new ASCIIEncoding();
			LogManager.Instance.LogMessage(encoder.GetString(message, 0, bytesRead));
		}
		
		tcpClient.Close();
	}

	#endregion
}
