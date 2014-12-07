using UnityEngine;
using System.Collections;
using System.IO;

public class MultiplayerMessage
{
	#region Enums

	public enum Type
	{
		Unknown,
		Input,
		Action
	}

	#endregion

	#region Variables

	public Type type = 0;

	#endregion

	#region Constructors

	public MultiplayerMessage(Type type)
	{
		this.type = type;
	}

	#endregion

	#region Methods

	public virtual byte[] ToBytes()
	{
		Debug.Log("This method must be overriden by children");
		return null;
	}

	public virtual void FromBytes(byte[] bytes)
	{
		using(MemoryStream memoryStream = new MemoryStream(bytes))
		{
			using(BinaryReader binaryReader = new BinaryReader(memoryStream))
				type = (Type)binaryReader.ReadInt32();
		}
	}

	#endregion
}
