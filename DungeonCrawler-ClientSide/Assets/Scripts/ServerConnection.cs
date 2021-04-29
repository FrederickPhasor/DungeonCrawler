using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System;
public class ServerConnection : MonoBehaviour
{
	Socket socket;
	IPAddress address = IPAddress.Parse("192.168.56.101");
	[SerializeField] TMP_InputField dynamicIP;
	[SerializeField] TMP_InputField dynamicPort;
	private void Start()
	{
		dynamicIP.text = "192.168.56.101";
		dynamicPort.text = "9087";
	}
	public void  ConnectToServer()
	{
		IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(dynamicIP.text), Int32.Parse(dynamicPort.text));
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try
		{
			socket.Connect(iPEndPoint);
			socket.ReceiveTimeout = 2000;
		}
		catch
		{
			Debug.Log("Something went wrong with the server connection.");
		}
	}
	public void SendPetition(string message)
	{
		socket.Send(System.Text.Encoding.ASCII.GetBytes(message));
	}
	public string WaitForAnswer()
	{
		if (!socket.Connected)
			return "No connection stablished";
		byte[] rawAnswer = new byte[120];
		socket.Receive(rawAnswer);
		string message = System.Text.Encoding.ASCII.GetString(rawAnswer);
		return message;
	}
	public void DisconnectFromServer()
	{
		socket.Close();
	}
}
