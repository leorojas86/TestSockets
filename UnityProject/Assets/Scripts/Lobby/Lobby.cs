using UnityEngine;
using System.Collections;

public class Lobby : MonoBehaviour 
{
	#region Methods

	void Start() 
	{
		SocketsManager.Instance.FindServers(OnServerFound, OnServerLost);
	}

	private void OnServerFound(SocketServerInfo serverInfo)
	{
	}

	private void OnServerLost(SocketServerInfo serverInfo)
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	#endregion
}
