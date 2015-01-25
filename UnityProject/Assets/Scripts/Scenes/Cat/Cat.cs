using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

public class Cat : MonoBehaviour 
{
	#region Constants

	private const string BUTTON_NAME_FORMAT = "Button_X_Y";

	#endregion

	#region Variables

	public Sprite spriteEmpty = null;
	public Sprite spriteX	  = null;
	public Sprite spriteO 	  = null;

	public TextMesh text = null;

	private List<List<SimpleButton>> _slotsButtons = new List<List<SimpleButton>>();

	#endregion

	#region Methods

	void Awake()
	{
		for(int x = 0; x < CatMultiplayerManager.Instance.SlotsSize; x++)
		{
			List<SimpleButton> collum = new List<SimpleButton>(); 

			for(int y = 0; y < CatMultiplayerManager.Instance.SlotsSize; y++)
			{
				string buttonName   = BUTTON_NAME_FORMAT.Replace("X", x.ToString()).Replace("Y", y.ToString());
				SimpleButton button = transform.Find(buttonName).GetComponent<SimpleButton>();
				button.OnClick 		= OnSlotButtonClick;
				button.customTag    = new Vector2(x, y);
				button.GetComponent<SpriteRenderer>().sprite = spriteEmpty;

				collum.Add(button);
			}

			_slotsButtons.Add(collum);
		}

		CatMultiplayerManager.Instance.StartNewGame();
		CatMultiplayerManager.Instance.OnGameAction = OnGameAction;

		UpdateTurnText();

		SocketsManager.Instance.Server.OnClientDisconnected = OnClientDisconnected;
		SocketsManager.Instance.Client.OnServerDisconnected = OnServerDisconnected;
	}

	private void OnClientDisconnected(TcpClient client)
	{
		Application.LoadLevel("Lobby");
	}

	private void OnServerDisconnected(TcpClient client)
	{
		Application.LoadLevel("Lobby");
	}

	private void UpdateTurnText()
	{
		if(CatMultiplayerManager.Instance.CurrentPlayerTurn == CatMultiplayerManager.Instance.MyPlayer)
			text.text = "Your Turn";
		else
			text.text = CatMultiplayerManager.Instance.CurrentPlayerTurn.ToString().Replace("Player", string.Empty) + " Turn";
	}

	private void UpdateWinnerText()
	{
		text.text = "Winner " + CatMultiplayerManager.Instance.Winner.ToString();
	}

	private void OnSlotButtonClick(SimpleButton sender)
	{
		Vector2 slotPosition 			= (Vector2)sender.customTag;
		SelectSlotInput selectSlotInput = new SelectSlotInput((int)slotPosition.x, (int)slotPosition.y, CatMultiplayerManager.Instance.MyPlayer);

		Debug.Log("OnSlotButtonClick slotPosition = " + slotPosition);
		CatMultiplayerManager.Instance.ProcessInput(selectSlotInput);
		//sender.GetComponent<SpriteRenderer>().sprite = spriteX;
	}

	private void OnGameAction(GameAction gameAction)
	{
		SelectSlotAction selectSlotAction = gameAction as SelectSlotAction;

		if(selectSlotAction != null)
		{
			//Debug.Log("selectSlotAction.slotX = " + selectSlotAction.slotX + " selectSlotAction.slotY = " + selectSlotAction.slotY);
			SimpleButton slotButton 						 = _slotsButtons[selectSlotAction.slotX][selectSlotAction.slotY];
			Sprite sprite									 = selectSlotAction.player == CatMultiplayerManager.Player.PlayerX ? spriteX : spriteO; 
			slotButton.GetComponent<SpriteRenderer>().sprite = sprite;

			UpdateTurnText();

			if(CatMultiplayerManager.Instance.Winner != CatMultiplayerManager.Player.None)
				UpdateWinnerText();
		}
		else
			Debug.LogError("Unknow game action = " + gameAction);
	}

	#endregion
}
