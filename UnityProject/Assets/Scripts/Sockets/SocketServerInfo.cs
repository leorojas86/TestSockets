using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Net;

public class SocketServerInfo
{
	#region Variables

	public IPAddress ip = null;
	public string info  = null;
	public bool listen  = true;

	#endregion

	#region Constructors
	
	public SocketServerInfo(IPAddress ip, string info)
	{
		this.ip   = ip;
		this.info = info;
	}

	#endregion
}