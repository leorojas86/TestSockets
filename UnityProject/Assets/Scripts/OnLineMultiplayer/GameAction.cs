using UnityEngine;
using System.Collections;

public abstract class GameAction : MultiplayerMessage 
{
	#region Constructors

	public GameAction(int subType, int messageBytes):base(MultiplayerMessage.Type.Action, subType, messageBytes)
	{
	}

	#endregion

	#region Methods

	//public abstract GameAction Clone();

	#endregion
}
