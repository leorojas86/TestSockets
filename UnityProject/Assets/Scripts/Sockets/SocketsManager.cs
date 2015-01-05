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
public class SocketsManager : MonoBehaviour
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

	private SocketServer _server = null;
	private SocketClient _client = null;

	private int _port = 3000;

	private List<IActionInvoker> _invokingActions = new List<IActionInvoker>();

	private bool _isLogEnabled = false;

	#endregion

	#region Properties

	public static SocketsManager Instance
	{
		get 
		{
			if(_instance != null)
				return _instance; 

			_instance = TestSocketsManagers.Instance.CreateNewManagerInstance<SocketsManager>();
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

	public bool IsLogEnabled
	{
		get { return _isLogEnabled; }
		set { _isLogEnabled = value; }
	}

	#endregion
	
	#region Methods

	void Awake()
	{
		_server = gameObject.AddComponent<SocketServer>();
		_client = gameObject.AddComponent<SocketClient>();
	}

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

    void Update()
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
		Debug.Log("InvokeAction 1");
		InvokeActionData<T> invokeData = new InvokeActionData<T>(action, param);
		Debug.Log("InvokeAction 2");
		_invokingActions.Add(invokeData);
	}

	public void Log(string message)
	{
		if(_isLogEnabled)
			Debug.Log(message);
	}

	void OnDestroy()
	{
		Debug.Log("OnDestroy");

		_server.StopServer();
		_server.StopBroadcastMessages();
		_client.Disconnect();
	}

	#endregion
}
