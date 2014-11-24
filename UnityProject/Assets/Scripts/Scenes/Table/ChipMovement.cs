using UnityEngine;
using System.Collections;

public class ChipMovement
{
	#region Variables

	private string _x;
	private string _y;
	private string _z;

	#endregion

	#region Properties

	public string X
	{
		get { return _x; }
		set { _x = value; }
	}

	public string Y
	{
		get { return _y; }
		set { _y = value; }
	}

	public string Z
	{
		get { return _z; }
		set { _z = value; }
	}

	#endregion

	#region Constructors

	public ChipMovement()
	{
	}
	
	public ChipMovement(Vector3 position)
	{
		_x = position.x.ToString();
		_y = position.y.ToString();
		_z = position.z.ToString();
	}

	#endregion

	#region Methods

	public Vector3 GetPosition()
	{
		return new Vector3(float.Parse(_x), float.Parse(_y), float.Parse(_z));
	}
	
	public static ChipMovement FromString(string data)
	{
		Debug.Log("FromString data = " + data);
		return LitJson.JsonMapper.ToObject<ChipMovement>(data);
	}
	
	public static string ToString(ChipMovement movement)
	{
		return LitJson.JsonMapper.ToJson(movement);
	}

	#endregion
}
