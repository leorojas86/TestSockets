using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

public class SocketClient 
{
	#region Variables

	private TcpClient _tcpClient 	   		   = null;
	private IPEndPoint _serverEndPoint 		   = null;
	private Thread _listenServerMessagesThread = null;

	private Thread _listenBroadcastMessagesThread = null;

	public System.Action<byte[]> OnServerMessage = null;

	#endregion

	#region Properties
	
	public IPEndPoint ServerEndPoint
	{
		get { return _serverEndPoint; }
	}
	
	#endregion

	#region Constructors
	
	public SocketClient()
	{
	}

	#endregion

	#region Methods

	public void FindServers()
	{
		_listenBroadcastMessagesThread = new Thread(new ThreadStart(ListenBroadcastMessages));
		_listenBroadcastMessagesThread.Start();
	}

	public bool ConnectToServer(IPAddress serverAddress, int port)
	{
		_serverEndPoint = new IPEndPoint(serverAddress, port);
		_tcpClient 		= new TcpClient();

		try
		{
			_tcpClient.Connect(_serverEndPoint);

			_listenServerMessagesThread = new Thread(new ThreadStart(ProcessServerMessagesThread));
			_listenServerMessagesThread.Start();

			FindServers();

			return true;
		}
		catch(Exception e)
		{
			LogManager.Instance.LogMessage("Could not connect to server at ip " + serverAddress + " using port = " + port + " exception = " + e.ToString());
			return false;
		}
	}

	public void Disconnect()
	{
		_serverEndPoint = null;
		_tcpClient.Close();
		_tcpClient = null;
		_listenServerMessagesThread.Abort();
		_listenServerMessagesThread = null;

		if(_listenBroadcastMessagesThread != null)
		{
			_listenBroadcastMessagesThread.Abort();
			_listenBroadcastMessagesThread = null;
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
		NetworkStream clientStream = _tcpClient.GetStream();
		
		while(true)
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

	private static void ListenBroadcastMessages() 
	{
		UdpClient listener = new UdpClient(SocketsManager.Instance.Port);
		IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, SocketsManager.Instance.Port);
		
		try 
		{
			while (true) 
			{
				LogManager.Instance.LogMessage("Waiting for broadcast");
				byte[] bytes = listener.Receive( ref groupEP);
				
				LogManager.Instance.LogMessage("Received broadcast from " + groupEP.ToString() + " :\n " + Encoding.ASCII.GetString(bytes,0,bytes.Length) + "\n");
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

	
	#endregion
}
