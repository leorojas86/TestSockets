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
			return true;
		}
		catch(Exception e)
		{
			LogManager.Instance.LogMessage("Could not connect to server at ip " + serverAddress + " using port = " + port + " exception = " + e.ToString());
			return false;
		}
	}

	public void SendMessageToServer(string message)
	{
		ASCIIEncoding encoder = new ASCIIEncoding();
		byte[] bytes 		  = encoder.GetBytes(message);
		
		SendBytesToServer(bytes);
	}

	public void SendBytesToServer(byte[] bytes)
	{
		using(MemoryStream memoryStream = new MemoryStream(bytes.Length + 4))
		{
			using(BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				int messageLength = bytes.Length;
				binaryWriter.Write(messageLength);
				binaryWriter.Write(bytes);
			}

			byte[] buffer 			   = memoryStream.GetBuffer();
			//LogManager.Instance.LogMessage("SendBytesToServer, buffer = " + buffer.Length);
			NetworkStream clientStream = _tcpClient.GetStream();
			clientStream.Write(buffer, 0 , buffer.Length);
			clientStream.Flush();
		}
	}

	#endregion
}
