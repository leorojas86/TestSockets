using UnityEngine;
using System.Collections;

public abstract class PlayerInput : MultiplayerMessage 
{
	#region Constructors
	
	public PlayerInput(int subType):base(MultiplayerMessage.Type.Input, subType)
	{
	}
	
	#endregion

	#region Methods

	//public abstract GameAction Clone();

	#endregion
}
