 using UnityEngine;
using System.Collections;

public class StartNewGameInput : PlayerInput
{	
	#region Constructors
	
	public StartNewGameInput():base((int)CatMultiplayerManager.GameActions.StartNewGame)
	{
	}
	
	#endregion
}
