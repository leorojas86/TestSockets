using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatMultiplayerManager : MultiplayerManager 
{
	#region Structs

	public struct SlotInfo
	{
		public int x;
		public int y;
		
		public SlotInfo(int x, int y)
		{
			this.x = 0;
			this.y = 0;
		}
	}

	#endregion

	#region Enums

	public enum Player
	{
		None,
		PlayerX,
		PlayerO
	}

	public enum GameActions
	{
		None,
		StartGame,
		SelectSlot
	} 

	public enum PlayerInputs
	{
		None,
		SelectSlot
	} 

	#endregion

	#region Variables

	private static CatMultiplayerManager _instance = null;

	private List<List<Player>> _slotsRows = new List<List<Player>>();

	private int _slotsSize 				= 3;
	private Player _currentPlayerTurn 	= Player.None;
	private Player _player 			  	= Player.None;

	private Player _winner 	 			= Player.None;
	private List<SlotInfo> _winnerSlots = null;   

	#endregion

	#region Properties

	public static CatMultiplayerManager Instance
	{
		get 
		{
			if(_instance != null)
				return _instance;

			_instance = new CatMultiplayerManager();
			return _instance; 
		}
	}

	public int SlotsSize
	{
		get { return _slotsSize; }
	}

	public Player Winner
	{
		get { return _winner; }
	}

	public Player MyPlayer
	{
		get { return _player; } 
	}

	public Player CurrentPlayerTurn
	{
		get { return _currentPlayerTurn; }
	}

	#endregion

	#region Constructor

	private CatMultiplayerManager()
	{
		_player = SocketsManager.Instance.Server.IsStarted ? Player.PlayerX : Player.PlayerO;

		StartNewGame();
	}

	#endregion

	#region Methods

	protected override void InitializeMessages()
	{
		_gameActions.Add(new SelectSlotAction());
		_playerInputs.Add(new SelectSlotInput());
	}

	public void StartNewGame()
	{
		base.StartNewGame();

		for(int x = 0; x < _slotsSize; x++)
		{
			List<Player> collum = new List<Player>();
			
			for(int y = 0; y < _slotsSize; y++)
				collum.Add(Player.None);
			
			_slotsRows.Add(collum);
		}

		if(_currentPlayerTurn == Player.None || _currentPlayerTurn == Player.PlayerX)
			_currentPlayerTurn = Player.PlayerO;
		else
			_currentPlayerTurn = Player.PlayerX;

		_winner 		= Player.None;
		_winnerSlots 	= null;
	}

	public override bool ProcessInput(PlayerInput input)
	{
		if(base.ProcessInput(input))//Only server must process inputs, clients must send the input to the server to let the server process the input
			return true;
		else
		{
			SelectSlotInput selectSlotInput = input as SelectSlotInput;

			if(selectSlotInput != null)
			{
				if(selectSlotInput.player == _currentPlayerTurn && _slotsRows[selectSlotInput.slotX][selectSlotInput.slotY] == Player.None)
				{
					SelectSlotAction selectSlotAction = new SelectSlotAction(selectSlotInput.slotX, selectSlotInput.slotY, selectSlotInput.player);
					ProcessAction(selectSlotAction);
					return true;
				}
			}
			else
				Debug.LogError("Unknown player input = " + input);
		}

		return false;
	}

	public override void ProcessAction(GameAction action)
	{
		SelectSlotAction selectSlotAction = action as SelectSlotAction;
		
		if(selectSlotAction != null)
		{
			_slotsRows[selectSlotAction.slotX][selectSlotAction.slotY] = selectSlotAction.player;

			CheckForWinner();
			//Update turn
			_currentPlayerTurn = _currentPlayerTurn == Player.PlayerX ? Player.PlayerO : Player.PlayerX;
		}

		base.ProcessAction(action);
	}

	private void CheckForWinner()
	{
		//Check Rows
		for(int x = 0; x < _slotsRows.Count; x++)
		{
			List<Player> currentRow = _slotsRows[x];

			Player player 			= currentRow[0];
			List<SlotInfo> slots	= new List<SlotInfo>();
			slots.Add(new SlotInfo(x, 0));

			for(int y = 1; y < currentRow.Count; y++)
			{
				if(player != currentRow[y])
					break;

				slots.Add(new SlotInfo(x, y));
			}

			if(slots.Count == _slotsSize)
			{
				_winner 		= player;
				_winnerSlots 	= slots;
				return;
			}
		}

		//Check Collums
		for(int y = 0; y < _slotsSize; y++)
		{
			List<Player> currentCollum = new List<Player>();

			Player player 			= _slotsRows[0][y];
			List<SlotInfo> slots	= new List<SlotInfo>();
			slots.Add(new SlotInfo(0, y));

			for(int x = 0; x < _slotsSize; x++)
			{
				if(player != _slotsRows[x][y])
					break;

				slots.Add(new SlotInfo(x, y));
			}

			if(slots.Count == _slotsSize)
			{
				_winner 		= player;
				_winnerSlots 	= slots;
				return;
			}
		}

		//Check next 
		Player player1 			= _slotsRows[0][0];
		List<SlotInfo> slots1	= new List<SlotInfo>();
		slots1.Add(new SlotInfo(0, 0));

		for(int x = 1, y = 1; x < _slotsRows.Count; x++, y++)
		{
			if(player1 != _slotsRows[x][y])
				break;

			slots1.Add(new SlotInfo(x, y));
		}

		if(slots1.Count == _slotsSize)
		{
			_winner 		= player1;
			_winnerSlots 	= slots1;
			return;
		}

		//Check next 
		Player player2 			= _slotsRows[2][0];
		List<SlotInfo> slots2	= new List<SlotInfo>();
		slots2.Add(new SlotInfo(2, 0));
		
		for(int x = 1, y = 1; x < _slotsRows.Count; x--, y++)
		{
			if(player2 != _slotsRows[x][y])
				break;

			slots2.Add(new SlotInfo(x, y));
		}
		
		if(slots2.Count == _slotsSize)
		{
			_winner 		= player2;
			_winnerSlots 	= slots2;
			return;
		}
	}

	#endregion
}
