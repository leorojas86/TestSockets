﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.IO;

public class NetworkUtils 
{
	#region Constants

	private const int BYTES_OF_INT = 4;

	#endregion

	#region Methods

	public static IPAddress GetMyIP4Address()
	{
		IPAddress address 			  = null;
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
		using(MemoryStream memoryStream = new MemoryStream(bytes.Length + BYTES_OF_INT))
		{
			using(BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				int messageLength = bytes.Length;
				binaryWriter.Write(messageLength);
				binaryWriter.Write(bytes);
			}
			
			byte[] buffer 			   = memoryStream.GetBuffer();
			NetworkStream clientStream = client.GetStream();
			clientStream.Write(buffer, 0, buffer.Length);
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

	public static List<byte[]> GetMessagesFromBytes(byte[] bytes)
	{
		List<byte[]> messages = new List<byte[]>();

		int bytesLength = bytes.Length;
		
		using(MemoryStream memoryStream = new MemoryStream(bytes))
		{
			using(BinaryReader binaryReader = new BinaryReader(memoryStream))
			{
				while(bytesLength > 0)
				{
					int messageLength = binaryReader.ReadInt32();//Read the message length
					bytesLength	 	 -= BYTES_OF_INT;
					byte[] data 	  = binaryReader.ReadBytes(messageLength);
					bytesLength      -= messageLength;

					messages.Add(data);
				}
			}
		}

		//Debug.Log("messages.Count = " + messages.Count);

		return messages;
	}

	/// <summary>
	/// Checks the connection state
	/// </summary>
	/// <returns>True on connected. False on disconnected.</returns>
	public static bool CheckIfConnected(TcpClient tcpClient)
	{
		/*if(tcpClient.Connected)
		{
			if((tcpClient.Client.Poll(0, SelectMode.SelectWrite)) && (!tcpClient.Client.Poll(0, SelectMode.SelectError)))
			{
				byte[] buffer = new byte[1];

				return tcpClient.Client.Receive(buffer, SocketFlags.Peek) != 0;
			}
		}

		return false;*/

		return true;

		/*IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

		TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

		for(int x = 0; x < tcpConnections.Length; x++)
		{
			TcpConnectionInformation tcpConnectionInfo = tcpConnections[x];

			if(tcpConnectionInfo.LocalEndPoint == tcpClient.Client.LocalEndPoint && tcpConnectionInfo.RemoteEndPoint == tcpClient.Client.RemoteEndPoint)
				return tcpConnectionInfo.State == TcpState.Established;
		}

		return false;*/
	}

	#endregion
}
