using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class NetworkUtils 
{
	public static IPAddress GetMyIP4Address()
	{
		IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());

		for(int x = 0; x < ipHostInfo.AddressList.Length; x++)
		{
			IPAddress ipAddress = ipHostInfo.AddressList[x];

			if(ipAddress.AddressFamily == AddressFamily.InterNetwork)
				return ipAddress;
		}

		return null;
	}
}
