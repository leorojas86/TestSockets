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
			this.x = x;
			this.y = y;
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

	private List<List<Player>> _slotsCollums = new List<List<Player>>();

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

	public List<SlotInfo> WinnerSlots
	{
		get { return _winnerSlots; }
	}

	#endregion

	#region Constructor

	private CatMultiplayerManager()
	{
	}

	#endregion

	#region Methods

	protected override void InitializeMessages()
	{
		_gameActions.Add(new SelectSlotAction());
		_playerInputs.Add(new SelectSlotInput());
	}

	public override void StartNewGame()
	{
		base.StartNewGame();

		_player 	 = SocketsManager.Instance.Server.IsStarted ? Player.PlayerX : Player.PlayerO;
		_winner      = Player.None;
		_winnerSlots = null;

		if(_currentPlayerTurn == Player.None || _currentPlayerTurn == Player.PlayerX)
			_currentPlayerTurn = Player.PlayerO;
		else
			_currentPlayerTurn = Player.PlayerX;

		for(int x = 0; x < _slotsSize; x++)
		{
			List<Player> collum = new List<Player>();
			
			for(int y = 0; y < _slotsSize; y++)
				collum.Add(Player.None);
			
			_slotsCollums.Add(collum);
		}
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
				if(selectSlotInput.player == _currentPlayerTurn && _slotsCollums[selectSlotInput.slotX][selectSlotInput.slotY] == Player.None)
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
			_slotsCollums[selectSlotAction.slotX][selectSlotAction.slotY] = selectSlotAction.player;

			CheckForWinner();
			//Update turn
			_currentPlayerTurn = _currentPlayerTurn == Player.PlayerX ? Player.PlayerO : Player.PlayerX;
		}

		base.ProcessAction(action);
	}

	private void CheckForWinner()
	{
		Debug.Log("CheckForWinner");

		//Check Rows
		for(int x = 0; x < _slotsSize; x++)
		{
			List<Player> currentCollum = _slotsCollums[x];
			Player player 			   = currentCollum[0];

			if(player != Player.None)
			{
				Player currentPlayer = player;
				List<SlotInfo> slots = new List<SlotInfo>();

				slots.Add(new SlotInfo(x, 0));

				string collum = "collum " + x + " " + player + ",";

				for(int y = 1; y < _slotsSize && player == currentPlayer; y++)
				{
					currentPlayer = currentCollum[y];
					collum 		  += currentPlayer + ",";

					if(player == currentPlayer)
						slots.Add(new SlotInfo(x, y));
				}

				Debug.Log(collum);

				if(slots.Count == _slotsSize)
				{
					Debug.Log("Winner on rows = " + player);
					_winner 		= player;
					_winnerSlots 	= slots;
					return;
				}
			}
		}

		//Check Collums
		for(int y = 0; y < _slotsSize; y++)
		{
			Player player = _slotsCollums[0][y];

			if(player != Player.None)
			{
				Player currentPlayer = player;
				List<SlotInfo> slots = new List<SlotInfo>();

				slots.Add(new SlotInfo(0, y));

				string row = "row " + y + " " + player + ",";

				for(int x = 1; x < _slotsSize && player == currentPlayer; x++)
				{
					currentPlayer = _slotsCollums[x][y];
					row 		 += currentPlayer + ",";

					if(player == currentPlayer)
					   slots.Add(new SlotInfo(x, y));
				}

				Debug.Log(row);

				if(slots.Count == _slotsSize)
				{
					Debug.Log("Winner on collums = " + player);
					_winner 		= player;
					_winnerSlots 	= slots;
					return;
				}
			}
		}

		//Check top-left to bottom/right 
		Player player1 = _slotsCollums[0][0];

		if(player1 != Player.None)
		{
			List<SlotInfo> slots1	= new List<SlotInfo>();
			slots1.Add(new SlotInfo(0, 0));

			for(int x = 1, y = 1; x < _slotsSize; x++, y++)
			{
				if(player1 != _slotsCollums[x][y])
					break;

				slots1.Add(new SlotInfo(x, y));
			}

			if(slots1.Count == _slotsSize)
			{
				Debug.Log("Winner on top-left to bottom/right = " + player1);
				_winner 		= player1;
				_winnerSlots 	= slots1;
				return;
			}
		}

		//Check top-right to bottom-left
		Player player2 = _slotsCollums[2][0];

		if(player2 != Player.None)
		{
			List<SlotInfo> slots2 = new List<SlotInfo>();
			slots2.Add(new SlotInfo(2, 0));
			
			for(int x = 1, y = 1; y < _slotsSize; x--, y++)
			{
				if(player2 != _slotsCollums[x][y])
					break;

				slots2.Add(new SlotInfo(x, y));
			}
			
			if(slots2.Count == _slotsSize)
			{
				Debug.Log("Winner on top-right to bottom-left = " + player2);
				_winner 		= player2;
				_winnerSlots 	= slots2;
				return;
			}
		}

		Debug.Log ("No winner found");
	}

	public bool IsWinnerSlot(int x, int y)
	{
		string winnerSlots = "";

		for(int i = 0; i < _winnerSlots.Count; i++)
			winnerSlots += "x = " + _winnerSlots[i].x + " y = " + _winnerSlots[i].y + ",";

		//Debug.Log("Winner Slots: " + winnerSlots);

		for(int i = 0; i < _winnerSlots.Count; i++)
		{
			SlotInfo winnerSlot = _winnerSlots[i];

			if(winnerSlot.x == x && winnerSlot.y == y)
			{
				//Debug.Log("IsWinnerSlot  x = " + x + " y = " + y);
				return true;
			}
		}

		//Debug.Log("NOT IsWinnerSlot  x = " + x + " y = " + y);
		return false;
	}

	#endregion
}
