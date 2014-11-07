using UnityEngine;
using System.Collections;

public class TestSockets : MonoBehaviour 
{
	private Vector2 scrollPosition = new Vector2(10, 270);


	void OnGUI()
	{
		if(GUI.Button(new Rect(10,370,100,30)," Start Server"))
			new SocketServer();
			//SocketsManager.Instance.StartServerListening();

		if(GUI.Button(new Rect(10,470,100,30)," Start Client"))
			new SocketClient();
			//SocketsManager.Instance.StartClientListening();

		// Begin a scroll view. All rects are calculated automatically - 
		// it will use up any available screen space and make sure contents flow correctly.
		// This is kept small with the last two parameters to force scrollbars to appear.
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width (500), GUILayout.Height (300));
		
		// We just add a single label to go inside the scroll view. Note how the
		// scrollbars will work correctly with wordwrap.
		GUILayout.Label (LogManager.Instance.Log);
		
		// End the scrollview we began above.
		GUILayout.EndScrollView ();

	}
}
