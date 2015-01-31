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

	public struct GameInfo
	{
		public Games type;

		public GameInfo(Games type)
		{
			this.type = type;
		}
	}

	#endregion

	#region Methods

	void Start() 
	{
		SocketsManager.Instance.Client.FindServers(null, null);

		SocketsManager.Instance.IsLogEnabled = true;
	}

	void OnGUI()
	{
		if(SocketsManager.Instance.Server.IsStarted)
		{
			GameInfo gameInfo = GetGameInfo(SocketsManager.Instance.Server.ServerInfo);

			GUI.Label(new Rect(10, 50, 300, 30), "Waiting for " + gameInfo.type + " client at " + SocketsManager.Instance.Server.ServerInfo.ip);

			if(GUI.Button(new Rect(10, 300, 100, 30), "Stop Game"))
			{
				SocketsManager.Instance.Server.StopServer();
				SocketsManager.Instance.Client.FindServers(null, null);
			}
		}
		else
		{
			float y = 0;
			
			for(int x = 0; x < SocketsManager.Instance.Client.FoundServers.Count; x++)
			{
				SocketServerInfo serverInfo = SocketsManager.Instance.Client.FoundServers[x];
				GameInfo gameInfo 			= GetGameInfo(serverInfo); 

				if(GUI.Button(new Rect(10, y, 200, 30), gameInfo.type + "@" + serverInfo.ip))
				{
					if(SocketsManager.Instance.Client.ConnectToServer(serverInfo.GetIPAddress()))
					{
						SocketsManager.Instance.Client.StopFindingServers();
						Application.LoadLevel(gameInfo.type + "Scene");
					}
				}
				
				y += 50;
			}

			y = 300;
			
			for(int x = 0; x < (int)Games.Count; x++)
			{
				Games currentGame = (Games)x;
				string text       = "Start " + currentGame + " Game";
				
				if(GUI.Button(new Rect(10, y, 200, 30), text))
				{
					SocketsManager.Instance.Client.StopFindingServers();
					SocketsManager.Instance.Server.StartServer(OnClientConnected);
					GameInfo serverInfo   = new GameInfo(currentGame);
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
		SocketsManager.Instance.Server.StopSendingInfoBroadcast();
		GameInfo gameInfo = GetGameInfo(SocketsManager.Instance.Server.ServerInfo);
		Application.LoadLevel(gameInfo.type + "Scene");
	}

	private static GameInfo GetGameInfo(SocketServerInfo serverInfo)
	{
		return LitJson.JsonMapper.ToObject<GameInfo>(serverInfo.info);
	}

	#endregion
}
