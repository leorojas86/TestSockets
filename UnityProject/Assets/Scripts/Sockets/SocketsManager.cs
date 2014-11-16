using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;


/// <summary>
/// http://tech.pro/tutorial/704/csharp-tutorial-simple-threaded-tcp-server
/// </summary>
public class SocketsManager 
{
	#region Variables

	private static SocketsManager _instance = null;

	private SocketServer _server = null;

	private SocketClient _client = null;

	private int _port = 3000;

	#endregion

	#region Properties

	public static SocketsManager Instance
	{
		get 
		{
			if(_instance != null)
				return _instance; 

			_instance = new SocketsManager();
			return _instance;
		} 
	}

	public SocketServer Server
	{
		get { return _server; }
	}

	public SocketClient Client
	{
		get { return _client; }
	}

	public int Port
	{
		get { return _port; }
		set { _port = value; }
	}

	#endregion

	#region Constructors

	private SocketsManager()
	{
	}

	#endregion

	#region Methods

	public void StartServer()
	{
		if(_server == null)
		{
			IPAddress myAddress = NetworkUtils.GetMyIP4Address();
			_server 			= new SocketServer();
			_server.StartServer(myAddress, _port);
		}
		else
			LogManager.Instance.LogMessage("Can not start server twice, please stop the server before starting a new one");
	}

	public void StartClient()
	{
		if(_client == null)
		{
			IPAddress myAddress = NetworkUtils.GetMyIP4Address();
			_client 			= new SocketClient();

			if(!_client.ConnectToServer(myAddress, _port))
				_client = null;
		}
		else
			LogManager.Instance.LogMessage("Can not start client twice, please stop the server before starting a new one");
	}

	#endregion
}
