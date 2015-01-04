using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Net;

public class SocketServerInfo
{
	#region Variables

	public string ip			 		  = null;
	public string info  		 		  = null;
	public System.DateTime lastListenTime = System.DateTime.MinValue;

	#endregion

	#region Constructors

	public SocketServerInfo()
	{
	}
	
	public SocketServerInfo(string ip, string info)
	{
		this.ip   			= ip;
		this.info 			= info;
		this.lastListenTime = System.DateTime.Now;
	}

	#endregion

	#region Methods

	public IPAddress GetIPAddress()
	{
		return IPAddress.Parse(ip);
	}

	public static string ToJson(SocketServerInfo info)
	{
		return LitJson.JsonMapper.ToJson(info);
	}

	public static SocketServerInfo FromJson(string data)
	{
		return LitJson.JsonMapper.ToObject<SocketServerInfo>(data);
	}

	#endregion
}