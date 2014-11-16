using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Net;

public class TestSockets : MonoBehaviour 
{
	private Vector2 scrollPosition = new Vector2(10, 270);

	private string connectToServerAddress = NetworkUtils.GetMyIP4Address().ToString();

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

		string serverButtonText = SocketsManager.Instance.Server == null ? "Start Server" : "Stop Server";

		if(GUI.Button(new Rect(10,370,100,30), serverButtonText))
		{
			if(SocketsManager.Instance.Server == null)
			{
				IPAddress myAddress = NetworkUtils.GetMyIP4Address();
				SocketsManager.Instance.StartServer(myAddress);
				SocketsManager.Instance.Server.OnClientConnected = OnClientConnected;
				SocketsManager.Instance.Server.OnClientMessage   = OnClientMessage;
			}
			else
				SocketsManager.Instance.StopServer();
		}

		if(SocketsManager.Instance.Server != null)
			GUI.Label(new Rect(190,370,400,30), "Server started at ip " + SocketsManager.Instance.Server.ServerEndPoint.Address + ", port " + SocketsManager.Instance.Server.ServerEndPoint.Port);

		string clientButtonText = SocketsManager.Instance.Client == null ? "Start Client" : "Stop Client";

		if(GUI.Button(new Rect(10,410,100,30), clientButtonText))
		{
			if(SocketsManager.Instance.Client == null)
			{
				IPAddress serverAddress = IPAddress.Parse(connectToServerAddress);
				SocketsManager.Instance.StartClient(serverAddress);
				SocketsManager.Instance.Client.OnServerMessage = OnServerMessage;
				SocketsManager.Instance.Client.SendMessageToServer("Hello Server!");
				SocketsManager.Instance.Client.SendMessageToServer("Hello Server2!");
			}
			else
				SocketsManager.Instance.StopClient();
		}

		if(SocketsManager.Instance.Client != null)
			GUI.Label(new Rect(190,410,400,30), "Client connected to server at ip " + SocketsManager.Instance.Client.ServerEndPoint.Address + ", port " + SocketsManager.Instance.Client.ServerEndPoint.Port);
		else
			connectToServerAddress = GUI.TextField(new Rect(130,410,100,30), connectToServerAddress);
	}

	private void OnClientConnected(TcpClient client)
	{
		LogManager.Instance.LogMessage("OnClientConnected = " + client.ToString());

		SocketsManager.Instance.Server.SendMessageToClient("Hello Client", client);
	}

	private void OnClientMessage(TcpClient client, byte[] message)
	{
		string messageString = NetworkUtils.GetMessageString(message);
		LogManager.Instance.LogMessage("OnClientMessage = " + messageString);
	}

	private void OnServerMessage(byte[] message)
	{
		string messageString = NetworkUtils.GetMessageString(message);
		LogManager.Instance.LogMessage("OnServerMessage = " + messageString);
	}
}
