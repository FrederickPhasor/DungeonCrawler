using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;

public class ServerConnection : MonoBehaviour
{
	Socket socket;
	Thread listen;
	ServerPetitions petitions;
	IPAddress address = IPAddress.Parse("192.168.56.103");
	[SerializeField] TMP_InputField dynamicIP;
	[SerializeField] TMP_InputField dynamicPort;
	void Start()
	{
		petitions = GetComponent<ServerPetitions>();
		dynamicIP.text = "192.168.56.103";
		dynamicPort.text = "9012";
	}
	public void ConnectToServer()
	{
		IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(dynamicIP.text), Int32.Parse(dynamicPort.text));
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try
		{
			socket.Connect(iPEndPoint);
			//socket.ReceiveTimeout = 2000;
		}
		catch
		{
			Debug.Log("Something went wrong with the server connection.");
		}

		ThreadStart ts = delegate { ListenToServer(); };
		listen = new Thread(ts);
		listen.Start();
	}
	public void SendPetition(string message)
	{
		socket.Send(System.Text.Encoding.ASCII.GetBytes(message));
	}
	public void WaitForAnswer()
	{
		byte[] rawAnswer = new byte[120];
		socket.Receive(rawAnswer);
		string[] parts = System.Text.Encoding.ASCII.GetString(rawAnswer).Split(new[] { '/' }, 2);
		int num = Convert.ToInt32(parts[0]);
		string message = parts[1];
		if (num == 6)
		{
			petitions.ShowInDebugScreen(message);
		}
		else petitions.ShowInDebugScreen(message);
	}

	public void ListenToServer()
	{
		while (true)
		{
			WaitForAnswer();
		}
	}
	public void DisconnectFromServer()
	{
		listen.Abort();
		socket.Close();
	}
}