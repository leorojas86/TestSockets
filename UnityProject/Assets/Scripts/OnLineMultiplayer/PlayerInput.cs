using UnityEngine;
using System.Collections;

public abstract class PlayerInput : MultiplayerMessage 
{
	#region Constructors
	
	public PlayerInput(int subType, int messageBytes):base(MultiplayerMessage.Type.Input, subType, messageBytes)
	{
	}
	
	#endregion

	#region Methods

	//public abstract GameAction Clone();

	#endregion
}
