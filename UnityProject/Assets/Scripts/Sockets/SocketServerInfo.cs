using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Net;

public class SocketServerInfo
{
	#region Variables

	public string ip			= null;
	public string info  		= null;
	public float lastListenTime = 0;

	#endregion

	#region Constructors
	
	public SocketServerInfo(string ip, string info)
	{
		this.ip   			= ip;
		this.info 			= info;
		this.lastListenTime = Time.time;
	}

	#endregion

	#region Properties

	public IPAddress IP
	{
		get { return IPAddress.Parse(ip); }
	}

	#endregion

	#region Methods

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