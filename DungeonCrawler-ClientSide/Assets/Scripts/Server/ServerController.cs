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
	public delegate void InvitationToPlayReceived(string whoInvitedUsName);
	public static event InvitationToPlayReceived InvitationReceivedEvent;
	
	public delegate void NewMessageReceived(string msg);
	public static event NewMessageReceived NewMessageReceivedEvent;

	public delegate void GroupDissolved();
	public static event GroupDissolved GroupDissolvedEvent;
	public delegate void ModifyPartners(int op, string name);
	public static event ModifyPartners ModifyPartner;
	public delegate void OnlineUsersUpdated(int op, string name);
	public static event OnlineUsersUpdated OnlineUsersUpdatedEvent;

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
	private void OnApplicationQuit()
	{
		Ask("-1/");
	}
	public void DisconnectFromServer()
	{
		socket.Close();
		listenForServer.Abort();
		socket.Shutdown(SocketShutdown.Both);
	}
	public void Ask(string message)
	{
		if(socket != null)
		{
			if (socket.Connected)
			{
				Debug.Log("Vamos a mandar: " + message);
				socket.Send(System.Text.Encoding.ASCII.GetBytes(message));
			}
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
			Debug.Log("We received : " + System.Text.Encoding.ASCII.GetString(rawAnswer));
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
				case 1://sign in result
					int res = Convert.ToInt32(parts[1]);
					if (res == 1)
					{
						SignedInEvent();
					}
					else
					{
						//Show the player loggin has failed
					}
					break;
				case 2://Receive invitation to group
					InvitationReceivedEvent(parts[1]);//This should be the username of who invited us
					break;
				case 3: //You could now join group or someone could not join to you!
				
					break;
				case 4://Grupo actualizado
					
					break;
				case 5://Group you were on was dissolved
					GroupDissolvedEvent();

					break;
				case 6: //Add or remove user from connected list
					string[] subparts = parts[1].Split(new[] { '/' }, 2);
					OnlineUsersUpdatedEvent(Convert.ToInt32(subparts[0]), subparts[1]);
					break;
				case 7: //Add or remove a partner
					string[] subparts2 = parts[1].Split(new[] { '/' }, 2);
					ModifyPartner(Convert.ToInt32(subparts2[0]), subparts2[1]);
					break;
				case 8: //Error al enviar mensaje privado
					
					break;
				default:
					break;
			}
		}
	}
}
