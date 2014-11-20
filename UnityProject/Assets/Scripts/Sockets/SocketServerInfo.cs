using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Net;

public class SocketServerInfo
{
	#region Variables

	public IPAddress ip;
	public string info;
	public float lastListenTime;

	#endregion

	#region Constructors
	
	public SocketServerInfo(IPAddress ip, string info)
	{
		this.ip   = ip;
		this.info = info;
	}

	#endregion
}