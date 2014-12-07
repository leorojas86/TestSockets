using UnityEngine;
using System.Collections;

public class PlayerInput : MultiplayerMessage 
{
	#region Constructors
	
	public PlayerInput():base(MultiplayerMessage.Type.Input)
	{
	}
	
	#endregion

}
