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
	public delegate void SignInSucess();
	public static event SignInSucess SignedInEvent;
	public delegate void InvitationToPlayReceived(string whoInvitedUsName);
	public static event InvitationToPlayReceived InvitationReceivedEvent;
	
	public delegate void NewMessageReceived(string msg);
	public static event NewMessageReceived NewMessageReceivedEvent;

	public delegate void GroupDissolved();
	public static event GroupDissolved GroupDissolvedEvent;
	public delegate void ModifyPartners(string name);
	public static event ModifyPartners ModifyPartnersEvent;
	public delegate void AllOnlineUserListUpdate(string names);
	public static event AllOnlineUserListUpdate OnlineUsersUpdatedEvent;
	public delegate void SinglePlayerUpdate(string name);
	public static event SinglePlayerUpdate SinglePlayerConnectionStateUpdateEvent;

	
	void OnEnable()
	{
		server = this;
		UILogInMenu.ConnectionToServerStablishedEvent += ClearDirect;
	}
	private void OnDisable()
	{
		UILogInMenu.ConnectionToServerStablishedEvent -= ClearDirect;
	}
	void ClearDirect()
	{
		server = this;
		listenForServer = new Thread(ListenForServer);
	}
	public int ConnectToServer(string IPaddress, string PORT)
	{
		ClearDirect();
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
		DisconnectFromServer();
	}
	public void DisconnectFromServer()
	{
		try
		{
			listenForServer.Abort();
		}
		catch
		{
			Debug.Log("Thread aborting was messy");
		}
		Ask("-1/");
		socket.Shutdown(SocketShutdown.Both);
		socket.Close();
		//Application.Quit();
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
			Debug.Log("Waiting for server");
			byte[] rawAnswer = new byte[1024];
			socket.Receive(rawAnswer);
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
				case 1://Loggin answer
					SignedInEvent();
					OnlineUsersUpdatedEvent(parts[1]);
					break;
				case 2://Receive invitation to group
					InvitationReceivedEvent(parts[1]);//This should be the username of who invited us
					break;
				case 3: //Chat
					NewMessageReceivedEvent(parts[1]);
					break;
				case 4://Add connected player 
					   //1 if add 0 if delete
					SinglePlayerConnectionStateUpdateEvent(parts[1]);
					break;
				case 5://Update partners 
					ModifyPartnersEvent(parts[1]);
					break;
				case 6:
					//Group you were on was dissolved
							GroupDissolvedEvent();
					break;
				case 8: //Error al enviar mensaje privado
					
					break;

				default:
					break;
			}
		}
	}
}
