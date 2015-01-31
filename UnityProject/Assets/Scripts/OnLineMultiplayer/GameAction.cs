using UnityEngine;
using System.Collections;

public abstract class GameAction : MultiplayerMessage 
{
	#region Constructors

	public GameAction(int subType):base(MultiplayerMessage.Type.Action, subType)
	{
	}

	#endregion

	#region Methods

	//public abstract GameAction Clone();

	#endregion
}
