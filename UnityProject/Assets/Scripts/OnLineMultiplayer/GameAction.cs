using UnityEngine;
using System.Collections;

public class GameAction : MultiplayerMessage 
{
	#region Constructors

	public GameAction():base(MultiplayerMessage.Type.Action)
	{
	}

	#endregion
}
