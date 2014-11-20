using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

public class Lobby : MonoBehaviour 
{
	#region Methods

	void Start() 
	{
		SocketsManager.Instance.FindServers(null, null);
	}

	void Update()
	{
		SocketsManager.Instance.Update();
	}

	void OnGUI()
	{
		if(SocketsManager.Instance.Server.IsStarted)
		{
			GUI.Label(new Rect(10, 50, 100, 30), "Waiting for client");
		}
		else
		{
			float y = 0;
			
			foreach(KeyValuePair<string, SocketServerInfo> pair in SocketsManager.Instance.Client.FoundServers)
			{
				if(GUI.Button(new Rect(10, y, 100, 30), pair.Value.ip.ToString()))
				{
					if(SocketsManager.Instance.ConnectClientToServer(pair.Value.ip))
						Application.LoadLevel("TableScene");
				}
				
				y += 50;
			}
		}

		string text = SocketsManager.Instance.Server.IsStarted ? "Stop Game" : "Start Table Game";

		if(GUI.Button(new Rect(10, 300, 100, 30), text))
		{
			if(SocketsManager.Instance.Server.IsStarted)
			{
				SocketsManager.Instance.StopServer();
				SocketsManager.Instance.FindServers(null, null);
			}
			else
			{
				SocketsManager.Instance.StopFindingServers();
				SocketsManager.Instance.StartServer(OnClientConnected);
				SocketsManager.Instance.Server.StartSendingServerInfoBroadcast();
			}
		}
	}

	private void OnClientConnected(TcpClient client)
	{
		//Application.LoadLevel("TableScene");
		StartCoroutine(LoadSceneCoroutine("TableScene"));
	}

	private IEnumerator LoadSceneCoroutine(string scene)
	{
		yield return new WaitForEndOfFrame();

		Application.LoadLevel(scene);
	}

	#endregion
}
