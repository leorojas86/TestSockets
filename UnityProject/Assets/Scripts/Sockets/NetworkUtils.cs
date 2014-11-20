using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.IO;

public class NetworkUtils 
{
	public static IPAddress GetMyIP4Address()
	{
		IPAddress address = null;

		NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

		foreach(NetworkInterface networkInterface in interfaces)
		{
			//Debug.Log("networkInterface.Description = " + networkInterface.Description + " networkInterface.Name = " + networkInterface.Name + " networkInterface.NetworkInterfaceType = " + networkInterface.NetworkInterfaceType);

			if(networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
			{
				UnicastIPAddressInformationCollection unicastAddress = networkInterface.GetIPProperties().UnicastAddresses;

				foreach(UnicastIPAddressInformation ipAddress in unicastAddress)
				{
					//Debug.Log("ipAddress.Address.AddressFamily = " + ipAddress.Address.AddressFamily + " ipAddress.Address = " + ipAddress.Address);

					if(ipAddress.Address.AddressFamily == AddressFamily.InterNetwork)
					{
						//Debug.LogWarning("ipAddress.Address = " + ipAddress.Address);
						address = ipAddress.Address;
					}
				}
			}
		}

		return address;

		/*IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");

		for(int x = 0; x < ipHostInfo.AddressList.Length; x++)
		{
			IPAddress ipAddress = ipHostInfo.AddressList[x];

			if(ipAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				Debug.Log("ipAddress = " + ipAddress);
				return ipAddress;
			}
		}

		return null;*/
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

	public static string GetMessageString(byte[] message)
	{
		ASCIIEncoding encoder = new ASCIIEncoding();
		return encoder.GetString(message, 0, message.Length);
	}

	public static byte[] GetMessageBytes(string message)
	{
		ASCIIEncoding encoder = new ASCIIEncoding();
		return encoder.GetBytes(message);
	}

	public static byte[] ReadBytesFromClient(TcpClient tcpClient)
	{
		NetworkStream clientStream = tcpClient.GetStream();

		if(clientStream.DataAvailable)
		{
			List<byte> bytesList = new List<byte>();
			byte[] readBuffer    = new byte[1024];
			
			// Incoming message may be larger than the buffer size. 
			while(clientStream.DataAvailable)
			{
				int numberOfBytesRead = clientStream.Read(readBuffer, 0, readBuffer.Length);
				byte[] readBytes 	  = new byte[numberOfBytesRead];
				System.Array.Copy(readBuffer, readBytes, numberOfBytesRead);
				bytesList.AddRange(readBytes);
			}

			return bytesList.ToArray();
		}

		return null;
	}
}
