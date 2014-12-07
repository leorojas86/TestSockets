using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatMultiplayerServer : MultiplayerServer 
{
	#region Enums

	public enum SlotValue
	{
		Empty,
		Player1,
		Player2
	}

	#endregion

	#region Variables

	private static CatMultiplayerServer _instance = null;

	private List<List<SlotValue>> _slotsRows = new List<List<SlotValue>>();

	private int _slotsSize = 3;

	#endregion

	#region Properties

	public static CatMultiplayerServer Instance
	{
		get 
		{
			if(_instance != null)
				return _instance;

			_instance = new CatMultiplayerServer();
			return _instance; 
		}
	}

	public int SlotsSize
	{
		get { return _slotsSize; }
	}

	#endregion

	#region Constructor

	public CatMultiplayerServer()
	{
		StartNewGame();
	}

	#endregion

	#region Methods

	public override void StartNewGame()
	{
		base.StartNewGame();

		for(int x = 0; x < _slotsSize; x++)
		{
			List<SlotValue> row = new List<SlotValue>();
			
			for(int y = 0; y < _slotsSize; y++)
				row.Add(SlotValue.Empty);
			
			_slotsRows.Add(row);
		}
	}

	public override GameAction ProcessInput(PlayerInput input)
	{
		return null;
	}

	#endregion
}
