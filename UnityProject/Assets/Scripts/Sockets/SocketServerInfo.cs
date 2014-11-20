using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Net;

public class SocketServerInfo
{
	#region Variables

	public string ip			= null;
	public IPAddress ipAddress 	= null;
	public string info  		= null;
	public bool listen  		= true;

	#endregion

	#region Constructors
	
	public SocketServerInfo(string ip, string info)
	{
		this.ip 	   = ip;
		this.ipAddress = IPAddress.Parse(ip);
		this.info 	   = info;
	}

	#endregion
}