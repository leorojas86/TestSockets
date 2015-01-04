using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

public class Lobby : MonoBehaviour 
{
	#region Enums

	public enum Games
	{
		Table,
		Cat,
		Count
	}

	#endregion

	#region Structs

	public struct ServerInfo
	{
		public Games type;

		public ServerInfo(Games type)
		{
			this.type = type;
		}
	}

	#endregion

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

			if(GUI.Button(new Rect(10, 300, 100, 30), "Stop Game"))
			{
				SocketsManager.Instance.StopServer();
				SocketsManager.Instance.FindServers(null, null);
			}
		}
		else
		{
			float y = 0;
			
			for(int x = 0; x < SocketsManager.Instance.Client.FoundServers.Count; x++)
			{
				SocketServerInfo serverInfo = SocketsManager.Instance.Client.FoundServers[x];

				if(GUI.Button(new Rect(10, y, 100, 30), serverInfo.ip))
				{
					if(SocketsManager.Instance.ConnectClientToServer(serverInfo.GetIPAddress()))
					{
						ServerInfo info = GetServerInfo(serverInfo);
						Application.LoadLevel(info.type + "Scene");
					}
				}
				
				y += 50;
			}

			y = 300;
			
			for(int x = 0; x < (int)Games.Count; x++)
			{
				Games currentGame = (Games)x;
				
				string text = "Start " + currentGame + " Game";
				
				if(GUI.Button(new Rect(10, y, 100, 30), text))
				{
					SocketsManager.Instance.StopFindingServers();
					SocketsManager.Instance.StartServer(OnClientConnected);
					ServerInfo serverInfo = new ServerInfo(currentGame);
					string serverInfoJson = LitJson.JsonMapper.ToJson(serverInfo);

					SocketsManager.Instance.Server.StartSendingServerInfoBroadcast(serverInfoJson);
					//Debug.Log("serverInfoJson = " + SocketServerInfo.ToJson(SocketsManager.Instance.Server.ServerInfo));
				}
				
				y += 50;
			}
		}
	}

	private void OnClientConnected(TcpClient client)
	{
		//Application.LoadLevel("TableScene");
		ServerInfo serverInfo = GetServerInfo(SocketsManager.Instance.Server.ServerInfo);
		StartCoroutine(LoadSceneCoroutine(serverInfo.type + "Scene"));
	}

	private static ServerInfo GetServerInfo(SocketServerInfo serverInfo)
	{
		return LitJson.JsonMapper.ToObject<ServerInfo>(serverInfo.info);
	}

	private IEnumerator LoadSceneCoroutine(string scene)
	{
		yield return new WaitForEndOfFrame();

		Application.LoadLevel(scene);
	}

	#endregion
}
