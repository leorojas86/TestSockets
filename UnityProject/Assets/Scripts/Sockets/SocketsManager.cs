using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;


/// <summary>
/// http://tech.pro/tutorial/704/csharp-tutorial-simple-threaded-tcp-server
/// </summary>
public class SocketsManager 
{
	#region Structs

	public interface IActionInvoker
	{
		void Invoke();
	}

	public class InvokeActionData<T> : IActionInvoker
	{
		public Action<T> action = null;
		public T actionParam;

		public InvokeActionData(Action<T> action, T actionParam)
		{
			this.action 	 = action;
			this.actionParam = actionParam;
		}

		public void Invoke()
		{
			action(actionParam);
		}
	}

	#endregion

	#region Variables

	private static SocketsManager _instance = null;

	private SocketServer _server = new SocketServer();
	private SocketClient _client = new SocketClient();

	private int _port = 3000;

	private List<IActionInvoker> _invokingActions = new List<IActionInvoker>();

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

	public void StartServer(System.Action<TcpClient> onClientConnected)
	{
		IPAddress myAddress = NetworkUtils.GetMyIP4Address();
		_server.StartServer(myAddress, _port, onClientConnected);
	}

	public void StopServer()
	{
		_server.StopServer();
	}

	public void FindServers(System.Action<SocketServerInfo> onServerFound, System.Action<SocketServerInfo> onServerLost)
	{
		_client.FindServers(_port, onServerFound, onServerLost);
	}

	public void StopFindingServers()
	{
		_client.StopFindingServers();
	}

	public bool ConnectClientToServer(IPAddress serverAddress)
	{
		return _client.ConnectToServer(serverAddress, _port);
	}

	public void DisconnectClientFromServer()
	{
		_client.Disconnect();
	}

	public void Update()
	{
		for(int x = 0; x < _invokingActions.Count; x++)
		{
			IActionInvoker invokeAction = _invokingActions[x];
			invokeAction.Invoke();
		}

		_invokingActions.Clear();
	}

	public void InvokeAction<T>(Action<T> action, T param)
	{
		InvokeActionData<T> invokeData = new InvokeActionData<T>(action, param);
		_invokingActions.Add(invokeData);
	}

	#endregion
}
