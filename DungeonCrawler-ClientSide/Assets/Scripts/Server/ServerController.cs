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
	public static ServerController server;
	Socket socket;
	Thread listenForServer;
	bool waitingForServer, loggedIn;
	public delegate void SignInSucess();
	public static event SignInSucess SignedInEvent;
	public delegate void OnlineUsersUpdate(string playerList);
	public static event OnlineUsersUpdate PartnersUpdateEvent;
	public delegate void InvitationToPlayReceived(string whoInvitedUsName);
	public static event InvitationToPlayReceived InvitationReceivedEvent;
	public delegate void OnlineUsersUpdated(string connectedPlayers);
	public static event OnlineUsersUpdated OnlineUsersUpdatedEvent;
	public delegate void NewMessageReceived(string msg);
	public static event NewMessageReceived NewMessageReceivedEvent;

	void Start()
	{
		server = this;
		listenForServer = new Thread(ListenForServer);
	}
	public int ConnectToServer(string IPaddress, string PORT)
	{
		IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(IPaddress), Int32.Parse(PORT));
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try
		{
			socket.Connect(iPEndPoint);
			listenForServer.Start();
			return 0;
		}
		catch
		{
			return -1;
		}
		
	}
	public void DisconnectFromServer()
	{
		listenForServer.Abort();
		socket.Shutdown(SocketShutdown.Both);
		socket.Close();
	}
	public void Ask(string message)
	{
		if (socket.Connected)
		{
			Debug.Log("Vamos a mandar " + message);
			socket.Send(System.Text.Encoding.ASCII.GetBytes(message));

		}
	}
	void ListenForServer()
	{
		while (true)
		{
			byte[] rawAnswer = new byte[990];
			socket.Receive(rawAnswer);
			waitingForServer = false;
			string[] parts = System.Text.Encoding.ASCII.GetString(rawAnswer).Split(new[] { '/' }, 2);
			int num;
			try
			{
				num = Convert.ToInt32(parts[0]); //What we ordered								
			}
			catch
			{
				num = -1;
			}
			switch (num)
			{
				case 1://sign in
					int res = Convert.ToInt32(parts[1]);
					if (res == 0)
					{
						SignedInEvent();
					}
					break;
				case 2://online users updated
					OnlineUsersUpdatedEvent(parts[1]);
					break;
				case 3: //nos han invitado
					InvitationReceivedEvent(parts[1]);
					break;
				case 4://Grupo actualizado
					Debug.Log("Recibido Group Update" + parts[1]);
					PartnersUpdateEvent(parts[1]);
					//UI.MakeGroupWaiting(parts[1]);
					// 8/nombre1/nombre2/etc 
					break;
				case 5://Message recived MsgType/msg
					Debug.Log("Recibido message" + parts[1]);
					NewMessageReceivedEvent(parts[1]);
					break;
				case 6: //Asignación lider 
					break;
				case 7: //Mover el equipo
					break;
				case 8: //Error al enviar mensaje privado
					
					break;
				default:
					break;
			}
		}
	}
}
