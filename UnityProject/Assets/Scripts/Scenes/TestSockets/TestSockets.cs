using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Net;

public class TestSockets : MonoBehaviour 
{
	private Vector2 scrollPosition = new Vector2(10, 270);

	void Start()
	{
		SocketsManager.Instance.Server.OnClientMessage 		= OnClientMessage;
		SocketsManager.Instance.Server.OnClientDisconnected = OnClientDisconnected;

		SocketsManager.Instance.Client.OnServerMessage 		= OnServerMessage;
		SocketsManager.Instance.Client.OnServerDisconnected = OnServerDisconnected;
	}

	void Update()
	{
		SocketsManager.Instance.Update();
	}

	void OnGUI()
	{
		// Begin a scroll view. All rects are calculated automatically - 
		// it will use up any available screen space and make sure contents flow correctly.
		// This is kept small with the last two parameters to force scrollbars to appear.
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width (500), GUILayout.Height (300));
		
		// We just add a single label to go inside the scroll view. Note how the
		// scrollbars will work correctly with wordwrap.
		GUILayout.Label(LogManager.Instance.Log);
		
		// End the scrollview we began above.
		GUILayout.EndScrollView();

		string serverButtonText = !SocketsManager.Instance.Server.IsStarted ? "Start Server" : "Stop Server";

		if(GUI.Button(new Rect(10,370,100,30), serverButtonText))
		{
			if(!SocketsManager.Instance.Server.IsStarted)
			{
				SocketsManager.Instance.StartServer(OnClientConnected);
				SocketsManager.Instance.Server.StartSendingServerInfoBroadcast(string.Empty);
			}
			else
				SocketsManager.Instance.StopServer();
		}

		if(SocketsManager.Instance.Server.IsStarted)
		{
			GUI.Label(new Rect(190,370,400,30), "Server started at ip " + SocketsManager.Instance.Server.ServerEndPoint.Address + ", port " + SocketsManager.Instance.Server.ServerEndPoint.Port);

			if(SocketsManager.Instance.Server.Clients.Count > 0 && GUI.Button(new Rect(500,370,200,30), "Send Message to Clients"))
				SocketsManager.Instance.Server.SendMessageToClients("Test Message");
		}

		string clientButtonText = !SocketsManager.Instance.Client.IsConnected ? "Connect Client" : "Disconnect Client";

		if(GUI.Button(new Rect(10,410,100,30), clientButtonText))
		{
			if(!SocketsManager.Instance.Client.IsConnected)
				SocketsManager.Instance.FindServers(OnServerFound, null);
			else
				SocketsManager.Instance.DisconnectClientFromServer();
		}

		if(SocketsManager.Instance.Client.IsConnected)
		{
			GUI.Label(new Rect(190,410,400,30), "Client connected to server at ip " + SocketsManager.Instance.Client.ConnectedServerEndPoint.Address + ", port " + SocketsManager.Instance.Client.ConnectedServerEndPoint.Port);

			if(GUI.Button(new Rect(500,410,200,30), "Send Message to Server"))
				SocketsManager.Instance.Client.SendMessageToServer("Test Message");
		}
	}

	private void OnServerFound(SocketServerInfo serverInfo)
	{
		SocketsManager.Instance.Client.StopFindingServers();
		ConnectClient(serverInfo.IP);
	}

	private void OnServerDisconnected(TcpClient server)
	{
		LogManager.Instance.LogMessage("OnServerDisconnected");
	}

	private void ConnectClient(IPAddress serverAddress)
	{
		SocketsManager.Instance.ConnectClientToServer(serverAddress);
		SocketsManager.Instance.Client.SendMessageToServer("Hello Server!");
		SocketsManager.Instance.Client.SendMessageToServer("Hello Server2!");
	}
	
	private void OnClientConnected(TcpClient client)
	{
		LogManager.Instance.LogMessage("OnClientConnected = " + client.ToString());

		SocketsManager.Instance.Server.SendMessageToClient("Hello Client", client);
	}

	private void OnClientMessage(SocketMessage message)
	{
		string messageString = message.GetStringData();
		LogManager.Instance.LogMessage("OnClientMessage = " + messageString);
	}

	private void OnClientDisconnected(TcpClient client)
	{
		LogManager.Instance.LogMessage("OnClientDisconnected");
	}

	private void OnServerMessage(SocketMessage message)
	{
		string messageString = message.GetStringData();
		LogManager.Instance.LogMessage("OnServerMessage = " + messageString);
	}
}
