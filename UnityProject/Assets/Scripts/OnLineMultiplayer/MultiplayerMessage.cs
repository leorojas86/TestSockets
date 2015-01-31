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

	public Type type           = Type.Unknown;
	public int subType         = 0;
	protected int messageBytes = 32;

	#endregion

	#region Constructors

	public MultiplayerMessage(Type type, int subType, int messageBytes)
	{
		this.type 	      = type;
		this.subType      = subType;
		this.messageBytes = messageBytes;
	}

	public MultiplayerMessage()
	{
	}

	#endregion

	#region Methods

	public byte[] ToBytes()
	{
		using(MemoryStream memoryStream = new MemoryStream(messageBytes))
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
