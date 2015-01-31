 using UnityEngine;
using System.Collections;

public class StartNewGameAction : GameAction
{	
	#region Constructors
	
	public StartNewGameAction():base((int)CatMultiplayerManager.GameActions.StartNewGame)
	{
	}
	
	#endregion
}
