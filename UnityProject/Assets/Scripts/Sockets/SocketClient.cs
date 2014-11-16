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

	private TcpClient _tcpClient 	   = null;
	private IPEndPoint _serverEndPoint = null;
	private Thread _clientThread	   = null;

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

	public bool ConnectToServer(IPAddress serverAddress, int port)
	{
		_serverEndPoint = new IPEndPoint(serverAddress, port);
		_tcpClient 		= new TcpClient();

		try
		{
			_tcpClient.Connect(_serverEndPoint);

			_clientThread = new Thread(new ThreadStart(ProcessServerMessagesThread));
			_clientThread.Start();

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
		_clientThread.Abort();
		_clientThread = null;
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

	#endregion
}
