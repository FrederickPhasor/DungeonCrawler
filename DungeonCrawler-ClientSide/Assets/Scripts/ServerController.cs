using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
public class ServerController : MonoBehaviour
{
	Socket socket;
	
	[SerializeField] TMP_InputField dynamicIP;
	[SerializeField] TMP_InputField dynamicPort;
	Thread listenForServer;
	bool waitingForServer, loggedIn;
	[SerializeField] UIController UI;
	delegate void EmptyDelegate();
	void Start()
	{
		dynamicIP.text = "192.168.56.103";
		dynamicPort.text = "9063";
		listenForServer = new Thread(ListenForServer);
		ConnectToServer();
		
	}
	private void Update()
	{
		UI.WaitingSign(waitingForServer);
		if (loggedIn)
		{
			UI.LoggedIn();
			loggedIn = false;
		}
		
	}
	void ConnectToServer()
	{
		IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(dynamicIP.text), Int32.Parse(dynamicPort.text));
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try
		{
			socket.Connect(iPEndPoint);
			listenForServer.Start();

		}
		catch
		{
			Debug.Log("Something went wrong with the server connection.");
		}
	}
	void ListenForServer()
	{
		while (true)
		{
			//Debug.Log("we are at the thread");
			byte[] rawAnswer = new byte[420];
			socket.Receive(rawAnswer);
			waitingForServer = false;
			string[] parts = System.Text.Encoding.ASCII.GetString(rawAnswer).Split(new[] { '/' }, 2);
			int num;
			Debug.Log("Part0:" + parts[0]);
			try
			{
				num = Convert.ToInt32(parts[0]); //What we ordered								
			}
            catch
            {
				Debug.LogError("Thhis shit was null!");
				num = -1;
			}
			switch (num)
			{
				case 1://sign up
					//Mostrar en pantalla un correcto inicio de sesión
					break;
				case 2://sign in
					int res = Convert.ToInt32(parts[1]);
					if (res == 0)
					{
						loggedIn = true;
						//Debug.Log("Sign In correct");
					}
					else
					{
						loggedIn = false;
						UI.ShowErrorMessage("Error al iniciar sesión");
					}
					break;
				case 5://Get recent games played by someone
					UI.SetRecentPlayers(parts[1]);
					break;
				case 6:
					UI.SetOnlineUsers(parts[1]);
					break;
				case 7: //nos han invitado
					UI.PopUpWaiting(parts[1]);
					break;
				case 8://Grupo actualizado
					Debug.Log("Recibido Group Update");
					UI.MakeGroupWaiting(parts[1]);
					// 8/nombre1/nombre2/etc 
					break;
				case 9:
					//9/n
					break;
				default:
					break;
				
			}
		}
	}
	public void Ask(string message)
	{
		if(socket.Connected)
			socket.Send(System.Text.Encoding.ASCII.GetBytes(message));
	}
	public void DisconnectFromServer()
	{
		listenForServer.Abort();
		socket.Shutdown(SocketShutdown.Both);
		socket.Close();
	}
}
