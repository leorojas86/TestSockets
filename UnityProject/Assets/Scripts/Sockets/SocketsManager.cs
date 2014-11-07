using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;


/// <summary>
/// http://tech.pro/tutorial/704/csharp-tutorial-simple-threaded-tcp-server
/// </summary>
public class SocketsManager 
{
	#region Variables

	private static SocketsManager _instance = null;

	#endregion

	#region Properties

	public static SocketsManager Instance
	{
		get 
		{
			if(_instance != null)
				return _instance; 

			_instance = new SocketsManager();
			return _instance;
		} 
	}

	#endregion

	#region Constructors

	private SocketsManager()
	{
	}

	#endregion
	

	// Incoming data from the client.
	public string data = null;
	
	public void StartServerListening() 
	{
		// Data buffer for incoming data.
		byte[] bytes = new Byte[1024];
		
		// Establish the local endpoint for the socket.
		// Dns.GetHostName returns the name of the 
		// host running the application.
		IPAddress ipAddress = NetworkUtils.GetMyIP4Address();

		if(ipAddress != null)
		{
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
			
			// Create a TCP/IP socket.
			Socket listener = new Socket(AddressFamily.InterNetwork,
			                             SocketType.Stream, ProtocolType.Tcp );
			
			// Bind the socket to the local endpoint and 
			// listen for incoming connections.
			try {
				listener.Bind(localEndPoint);
				listener.Listen(10);
				
				// Start listening for connections.
				while (true) {
					LogManager.Instance.LogMessage("Waiting for a connection...");
					// Program is suspended while waiting for an incoming connection.
					Socket handler = listener.Accept();
					data = null;
					
					// An incoming connection needs to be processed.
					while (true) {
						bytes = new byte[1024];
						int bytesRec = handler.Receive(bytes);
						data += Encoding.ASCII.GetString(bytes,0,bytesRec);
						if (data.IndexOf("<EOF>") > -1) {
							break;
						}
					}
					
					// Show the data on the console.
					LogManager.Instance.LogMessage( "Text received : " + data);
					
					// Echo the data back to the client.
					byte[] msg = Encoding.ASCII.GetBytes(data);
					
					handler.Send(msg);
					handler.Shutdown(SocketShutdown.Both);
					handler.Close();
				}
				
			} catch (Exception e) {
				LogManager.Instance.LogMessage(e.ToString());
			}
		}
		else
			LogManager.Instance.LogMessage("A valid ip4 address was not found");
		
	}

	public void StartClientListening() {
		// Data buffer for incoming data.
		byte[] bytes = new byte[1024];
		
		// Connect to a remote device.
		try {
			// Establish the remote endpoint for the socket.
			// This example uses port 11000 on the local computer.
			IPAddress ipAddress = NetworkUtils.GetMyIP4Address();
			
			if(ipAddress != null)
			{
				LogManager.Instance.LogMessage("ipAddress.AddressFamily = " + ipAddress.AddressFamily);

				LogManager.Instance.LogMessage("ipAddress = " + ipAddress);

				IPEndPoint remoteEP = new IPEndPoint(ipAddress,11000);
				
				// Create a TCP/IP  socket.
				Socket sender = new Socket(AddressFamily.InterNetwork, 
				                           SocketType.Stream, ProtocolType.Tcp );
				
				// Connect the socket to the remote endpoint. Catch any errors.
				try {
					sender.Connect(remoteEP);
					
					LogManager.Instance.LogMessage("Socket connected to " + 
					                  sender.RemoteEndPoint.ToString());
					
					// Encode the data string into a byte array.
					byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");
					
					// Send the data through the socket.
					int bytesSent = sender.Send(msg);
					
					// Receive the response from the remote device.
					int bytesRec = sender.Receive(bytes);
					LogManager.Instance.LogMessage("Echoed test = " +
					                  Encoding.ASCII.GetString(bytes,0,bytesRec));
					
					// Release the socket.
					sender.Shutdown(SocketShutdown.Both);
					sender.Close();
					
				} catch (ArgumentNullException ane) {
					LogManager.Instance.LogMessage("ArgumentNullException : " + ane.ToString());
				} catch (SocketException se) {
					LogManager.Instance.LogMessage("SocketException : " + se.ToString());
				} catch (Exception e) {
					LogManager.Instance.LogMessage("Unexpected exception : " + e.ToString());
				}
			}
			else
				LogManager.Instance.LogMessage("A valid ip4 address was not found");
				
			} catch (Exception e) {
				LogManager.Instance.LogMessage( e.ToString());
		}
	}
}
