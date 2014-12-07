using UnityEngine;
using System.Collections;

public abstract class MultiplayerServer 
{
	#region Constructors

	public MultiplayerServer()
	{
	}

	#endregion

	#region Methods

	public virtual void StartNewGame()
	{
		SocketsManager.Instance.Client.OnServerMessage = OnSocketMessage;
		SocketsManager.Instance.Server.OnClientMessage = OnSocketMessage;
	}

	private void OnSocketMessage(SocketMessage message)
	{

	}

	public abstract GameAction ProcessInput(PlayerInput input);

	#endregion
}
