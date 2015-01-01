using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cat : MonoBehaviour 
{
	#region Constants

	private const string BUTTON_NAME_FORMAT = "Button_X_Y";

	#endregion

	#region Variables

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
				collum.Add(button);
			}
		}
	}

	private void OnSlotButtonClick(SimpleButton sender)
	{
		Debug.Log(sender.name);
	}

	#endregion
}
