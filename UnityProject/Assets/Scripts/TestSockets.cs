using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Text;

public class TestSockets : MonoBehaviour 
{
	private Vector2 scrollPosition = new Vector2(10, 270);


	void OnGUI()
	{
		if(GUI.Button(new Rect(10,370,100,30), "Start Server"))
		{
			SocketsManager.Instance.StartServer();
			SocketsManager.Instance.Server.OnClientConnected = OnClientConnected;
			SocketsManager.Instance.Server.OnClientMessage   = OnClientMessage;
		}

		if(SocketsManager.Instance.Server != null)
			GUI.Label(new Rect(190,370,400,30), "Server started at ip " + SocketsManager.Instance.Server.ServerEndPoint.Address + ", port " + SocketsManager.Instance.Server.ServerEndPoint.Port);

		if(GUI.Button(new Rect(10,470,100,30), "Start Client"))
		{
			SocketsManager.Instance.StartClient();
			SocketsManager.Instance.Client.SendMessageToServer("Hello Server!");
			SocketsManager.Instance.Client.SendMessageToServer("Hello Server2!");
		}

		if(SocketsManager.Instance.Client != null)
			GUI.Label(new Rect(190,470,400,30), "Client connected to server at ip " + SocketsManager.Instance.Client.ServerEndPoint.Address + ", port " + SocketsManager.Instance.Client.ServerEndPoint.Port);

		// Begin a scroll view. All rects are calculated automatically - 
		// it will use up any available screen space and make sure contents flow correctly.
		// This is kept small with the last two parameters to force scrollbars to appear.
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width (500), GUILayout.Height (300));
		
		// We just add a single label to go inside the scroll view. Note how the
		// scrollbars will work correctly with wordwrap.
		GUILayout.Label(LogManager.Instance.Log);
		
		// End the scrollview we began above.
		GUILayout.EndScrollView();
	}

	private void OnClientConnected(TcpClient client)
	{
		LogManager.Instance.LogMessage("OnClientConnected = " + client.ToString());
	}

	private void OnClientMessage(TcpClient client, byte[] message)
	{
		ASCIIEncoding encoder = new ASCIIEncoding();
		string messageString  = encoder.GetString(message, 0, message.Length);
		LogManager.Instance.LogMessage("OnClientMessage = " + messageString);
	}
}
