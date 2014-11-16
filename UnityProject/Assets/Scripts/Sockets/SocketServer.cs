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
			if(clientStream.DataAvailable)
			{
				//Debug.Log("clientStream.DataAvailable = " + clientStream.DataAvailable);

				using(MemoryStream memoryStream = new MemoryStream())
				{
					int bufferLength 	  = 0; 
					byte[] readBuffer     = new byte[1024];
					int numberOfBytesRead = 0;
					
					// Incoming message may be larger than the buffer size. 
					while(clientStream.DataAvailable)
					{
						numberOfBytesRead = clientStream.Read(readBuffer, 0, readBuffer.Length);
						LogManager.Instance.LogMessage("numberOfBytesRead = " + numberOfBytesRead);
						memoryStream.Write(readBuffer, 0, numberOfBytesRead);
						bufferLength += numberOfBytesRead;
					}

					//Debug.Log("clientStream.DataAvailable = " + clientStream.DataAvailable);
					byte[] buffer = memoryStream.GetBuffer();

					using(MemoryStream memoryStream2 = new MemoryStream(buffer))
					{
						using(BinaryReader binaryReader = new BinaryReader(memoryStream2))
						{
							//Debug.Log("using(BinaryReader binaryReader = new BinaryReader(memoryStream))");

							while(bufferLength > 0)
							{
								int messageLength = binaryReader.ReadInt32();//Read the message length

								LogManager.Instance.LogMessage("messageLength = " + messageLength);

								bufferLength	 -= 4;
								byte[] bytes 	  = binaryReader.ReadBytes(messageLength);
								bufferLength     -= messageLength;
								LogManager.Instance.LogMessage("bufferLength = " + bufferLength);
								NotifyOnClientMessage(tcpClient, bytes);
							}
						}
					}
				}
			}
		}
	}

	private void NotifyOnClientMessage(TcpClient client, byte[] bytes)
	{
		if(OnClientMessage != null)
			OnClientMessage(client, bytes);
	}

	#endregion
}
