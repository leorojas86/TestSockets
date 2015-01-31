using UnityEngine;
using System.Collections;

public class SelectSlotAction : GameAction
{
	#region Constants

	public const int MESSAGE_BYTES = 32;

	#endregion

	#region Variables

	public int slotX 						   = 0;
	public int slotY 						   = 0;
	public CatMultiplayerManager.Player player = CatMultiplayerManager.Player.None;

	#endregion

	#region Constructors

	public SelectSlotAction(int slotX, int slotY, CatMultiplayerManager.Player player):base((int)CatMultiplayerManager.GameActions.SelectSlot, MESSAGE_BYTES)
	{
		this.slotX  = slotX;
		this.slotY  = slotY;
		this.player = player;
	}

	public SelectSlotAction():base((int)CatMultiplayerManager.GameActions.SelectSlot, MESSAGE_BYTES)
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
