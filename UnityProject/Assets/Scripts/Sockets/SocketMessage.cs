using UnityEngine;
using System.Collections;
using System.Net.Sockets;

public class SocketMessage 
{
	#region Variables

	public TcpClient sender = null;
	public byte[] data		= null;

	#endregion

	#region Contructors

	public SocketMessage(TcpClient sender, byte[] data)
	{
		this.sender = sender;
		this.data   = data;
	}

	#endregion

	#region Methods

	public string GetStringData()
	{
		return NetworkUtils.GetMessageString(data);
	}

	#endregion
}
