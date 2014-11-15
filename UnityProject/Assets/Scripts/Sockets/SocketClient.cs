using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

public class SocketClient 
{
	#region Variables

	private TcpClient _tcpClient = null;

	private IPEndPoint _serverEndPoint = null;

	#endregion

	#region Properties
	
	public IPEndPoint ServerEndPoint
	{
		get { return _serverEndPoint; }
	}
	
	#endregion

	#region Constructors
	
	public SocketClient(IPAddress serverAddress, int port)
	{
		_serverEndPoint = new IPEndPoint(serverAddress, port);
		_tcpClient 		= new TcpClient();
		_tcpClient.Connect(_serverEndPoint);
		
		NetworkStream clientStream 	= _tcpClient.GetStream();
		ASCIIEncoding encoder 		= new ASCIIEncoding();
		byte[] buffer 				= encoder.GetBytes("Hello Server!");
		
		clientStream.Write(buffer, 0 , buffer.Length);
		clientStream.Flush();
	}

	#endregion
}
