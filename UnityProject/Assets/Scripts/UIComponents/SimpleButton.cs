using UnityEngine;
using System.Collections;

public class SimpleButton : MonoBehaviour 
{
	public System.Action<SimpleButton> OnClick = null;

	void OnMouseUp() 
	{
		if(OnClick != null)
			OnClick(this);
	}
}
