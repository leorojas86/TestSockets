using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class MultiplayerManager 
{
	#region Variables
	
	protected List<GameAction> _gameActions   = new List<GameAction>();
	protected List<PlayerInput> _playerInputs = new List<PlayerInput>(); 

	private System.Action<GameAction> OnGameAction = null;
	
	#endregion

	#region Constructors

	public MultiplayerManager()
	{
		SocketsManager.Instance.Client.OnServerMessage = OnSocketMessage;
		SocketsManager.Instance.Server.OnClientMessage = OnSocketMessage;
	}

	#endregion

	#region Methods

	public virtual void StartNewGame()
	{
		InitializeMessages();

		SocketsManager.Instance.Client.OnServerMessage = OnSocketMessage;
		SocketsManager.Instance.Server.OnClientMessage = OnSocketMessage;
	}

	protected virtual void OnSocketMessage(SocketMessage message)
	{
		MultiplayerMessage multiplayerMessage = new MultiplayerMessage();

		multiplayerMessage.FromBytes(message.data);

		switch(multiplayerMessage.type)
		{
			case MultiplayerMessage.Type.Action:
				GameAction action = FindAction(message.data);
				ProcessAction(action);
			break;
			case MultiplayerMessage.Type.Input:
				PlayerInput input = FindInput(message.data);
				ProcessInput(input);
			break;
		}
	}

	private GameAction FindAction(byte[] bytes)
	{
		for(int x = 0; x < _gameActions.Count; x++)
		{
			GameAction currentAction = _gameActions[x];

			if(currentAction.FromBytes(bytes))
				return currentAction;
		}

		Debug.LogError("Could not find game action");
		return null;
	}

	private PlayerInput FindInput(byte[] bytes)
	{
		for(int x = 0; x < _playerInputs.Count; x++)
		{
			PlayerInput currentInput = _playerInputs[x];
			
			if(currentInput.FromBytes(bytes))
				return currentInput;
		}
		
		Debug.LogError("Could not find game action");
		return null;
	}
	
	public abstract void InitializeMessages();

	public virtual bool ProcessInput(PlayerInput input)
	{
		if(!SocketsManager.Instance.Server.IsStarted)//it's client, only server should process inputs
		{
			SocketsManager.Instance.Client.SendMessageToServer(input.ToBytes());//Sending player input to server to let server process it
			return true;
		}

		return false;
	}
	
	public virtual void ProcessAction(GameAction action)
	{
		if(SocketsManager.Instance.Server.IsStarted)//it's server, clients need to be notified be notified
			SocketsManager.Instance.Server.SendMessageToClients(action.ToBytes());
		
		if(OnGameAction != null)
			OnGameAction(action);
	}

	#endregion
}
