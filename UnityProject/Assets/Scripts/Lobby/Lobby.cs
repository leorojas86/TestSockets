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

	void OnGUI()
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

		if(!SocketsManager.Instance.Server.IsStarted)
		{
			if(GUI.Button(new Rect(10, 300, 100, 30), "Start Table Game"))
			{
				SocketsManager.Instance.StartServer(OnClientConnected);
				SocketsManager.Instance.Server.StartSendingServerInfoBroadcast();
			}
		}
	}

	private void OnClientConnected(TcpClient client)
	{
		Application.LoadLevel("TableScene");
	}

	#endregion
}
