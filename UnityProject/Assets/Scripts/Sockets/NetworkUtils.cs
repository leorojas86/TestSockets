using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

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

	public static void SendBytesToClient(byte[] bytes, TcpClient client)
	{
		using(MemoryStream memoryStream = new MemoryStream(bytes.Length + 4))
		{
			using(BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				int messageLength = bytes.Length;
				binaryWriter.Write(messageLength);
				binaryWriter.Write(bytes);
			}
			
			byte[] buffer 			   = memoryStream.GetBuffer();
			NetworkStream clientStream = client.GetStream();
			clientStream.Write(buffer, 0 , buffer.Length);
			clientStream.Flush();
		}
	}
}
