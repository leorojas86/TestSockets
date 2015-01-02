using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
				button.GetComponent<SpriteRenderer>().sprite = spriteEmpty;

				collum.Add(button);
			}
		}

		UpdateTurnText();
	}

	private void UpdateTurnText()
	{
		if(CatMultiplayerManager.Instance.CurrentPlayerTurn == CatMultiplayerManager.Instance.MyPlayer)
			text.text = "Your Turn";
		else
			text.text = CatMultiplayerManager.Instance.CurrentPlayerTurn.ToString().Replace("Player", string.Empty);
	}

	private void OnSlotButtonClick(SimpleButton sender)
	{
		sender.GetComponent<SpriteRenderer>().sprite = spriteX;
	}

	#endregion
}
