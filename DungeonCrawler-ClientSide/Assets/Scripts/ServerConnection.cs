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
	IPAddress address = IPAddress.Parse("192.168.56.102");
	[SerializeField] TMP_InputField dynamicIP;
	[SerializeField] TMP_InputField dynamicPort;
	private void Start()
	{
		dynamicIP.text = "192.168.56.102";
		dynamicPort.text = "8108";
	}
	private bool  ConnectToServer()
	{
		IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(dynamicIP.text), Int32.Parse(dynamicPort.text));
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try
		{
			socket.Connect(iPEndPoint);
			socket.ReceiveTimeout = 5000;
			return true;
		}
		catch
		{
			return false;
		}
	}
	public void SendPetition(string message)
	{
		if (!ConnectToServer())
			return;
		socket.Send(System.Text.Encoding.ASCII.GetBytes(message));
	}
	public string WaitForAnswer()
	{
		if (!socket.Connected)
			return "No connection stablished";
		byte[] rawAnswer = new byte[120];
		socket.Receive(rawAnswer);
		string message = System.Text.Encoding.ASCII.GetString(rawAnswer);
		socket.Close();
		return message;
	}
}
