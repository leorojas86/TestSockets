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
		Action,
		Message
	}

	#endregion

	#region Variables

	public Type type            = Type.Unknown;
	public int subType          = 0;
	protected int _messageBytes = 0;

	#endregion

	#region Constructors

	public MultiplayerMessage(Type type, int subType)
	{
		this.type 	      = type;
		this.subType      = subType;
	}

	public MultiplayerMessage()
	{
	}

	#endregion

	#region Methods

	public byte[] ToBytes()
	{
		using(MemoryStream memoryStream = new MemoryStream(_messageBytes))
		{
			using(BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				Write(binaryWriter);
				return memoryStream.GetBuffer();
			}
		}
	}

	protected virtual void Write(BinaryWriter binaryWriter)
	{
		binaryWriter.Write((int)type);
		binaryWriter.Write((int)subType);

		Debug.Log("sizeof(int) = " + sizeof(int));
		_messageBytes = 0;
		_messageBytes += sizeof(int) * 2;
	}

	public bool FromBytes(byte[] bytes)
	{
		using(MemoryStream memoryStream = new MemoryStream(bytes))
		{
			using(BinaryReader binaryReader = new BinaryReader(memoryStream))
				return Read(binaryReader);
		}
	}

	protected virtual bool Read(BinaryReader binaryReader)
	{
		type 		= (Type)binaryReader.ReadInt32();
		int subType = binaryReader.ReadInt32();
		
		return subType == this.subType;
	}

	#endregion
}
