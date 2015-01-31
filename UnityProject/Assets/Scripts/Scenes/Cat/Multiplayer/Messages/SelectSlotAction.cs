using UnityEngine;
using System.Collections;

public class SelectSlotAction : GameAction
{
	#region Variables

	public int slotX 						   = 0;
	public int slotY 						   = 0;
	public CatMultiplayerManager.Player player = CatMultiplayerManager.Player.None;

	#endregion

	#region Constructors

	public SelectSlotAction(int slotX, int slotY, CatMultiplayerManager.Player player):base((int)CatMultiplayerManager.GameActions.SelectSlot)
	{
		this.slotX  = slotX;
		this.slotY  = slotY;
		this.player = player;
	}

	public SelectSlotAction():base((int)CatMultiplayerManager.GameActions.SelectSlot)
	{
	}

	#endregion

	#region Methods

	protected override void Write(System.IO.BinaryWriter binaryWriter)
	{
		base.Write(binaryWriter);
		
		binaryWriter.Write(slotX);
		binaryWriter.Write(slotY);
		binaryWriter.Write((int)player);

		_messageBytes += sizeof(int) * 3;
	}
	
	protected override bool Read(System.IO.BinaryReader binaryReader)
	{
		bool success = base.Read(binaryReader);
		
		slotX  = binaryReader.ReadInt32();
		slotY  = binaryReader.ReadInt32();
		player = (CatMultiplayerManager.Player)binaryReader.ReadInt32();
		
		return success;
	}

	#endregion
}
