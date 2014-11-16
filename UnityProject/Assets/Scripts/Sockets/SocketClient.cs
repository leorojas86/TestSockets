﻿using UnityEngine;
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
		byte[] bytes = NetworkUtils.GetMessageBytes(message);

		SendMessageToServer(bytes);
	}

	public void SendMessageToServer(byte[] bytes)
	{
		NetworkUtils.SendBytesToClient(bytes, _tcpClient);
	}

	#endregion
}
