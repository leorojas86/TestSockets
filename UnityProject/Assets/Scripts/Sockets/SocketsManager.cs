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
	#region Variables

	private static SocketsManager _instance = null;

	private SocketServer _server = null;
	private SocketClient _client = null;

	private int _port = 3000;

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

	public void Log(string message)
	{
		if(_isLogEnabled)
			Debug.Log(message);
	}

	#endregion
}
