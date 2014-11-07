using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;


public class SocketClient 
{
	public SocketClient()
	{
		TcpClient client = new TcpClient();
		
		IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
		
		client.Connect(serverEndPoint);
		
		NetworkStream clientStream = client.GetStream();
		
		ASCIIEncoding encoder = new ASCIIEncoding();
		byte[] buffer = encoder.GetBytes("Hello Server!");
		
		clientStream.Write(buffer, 0 , buffer.Length);
		clientStream.Flush();
	}
}
