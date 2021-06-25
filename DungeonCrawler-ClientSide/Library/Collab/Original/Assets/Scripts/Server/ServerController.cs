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
	public delegate void AddGroups(int op);
	public static event AddGroups AddGroupEvent;
	public delegate void GameStart(int seed);
	public static event GameStart GameStartEvent;
	public delegate void SetRoom(string roomCoords);
	public static event SetRoom SetRoomEvent;
	public delegate void ChangeDirection(string direction);
	public static event ChangeDirection ChangeDirectionEvent;
	public delegate void EnemyListUpdate(string enemyList);
	public static event EnemyListUpdate EnemyListUpdateEvent;
	public delegate void SomeoneEntered(string indexOfGroup);
	public static event SomeoneEntered SomeoneEnteredMyRoomEvent;
	public delegate void GetCurrenntGroupsInCoords(string indexesGroups);
	public static event GetCurrenntGroupsInCoords GetCurrenntGroupsInCoordsEvent;

	private void Awake()
	{
		if (server != null)
			GameObject.Destroy(server);
		else
			server = this;
		DontDestroyOnLoad(this);
	}
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
			byte[] rawAnswer = new byte[5000];
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
                    string[] trimmed = parts[1].Split(new[] { '\0' }, 2);
					ModifyPartnersEvent(trimmed[0]);
					break;
				case 6:
					//Group you were on was dissolved
							GroupDissolvedEvent();
					break;
				case 8://Update enemyGroups position
					
					break;
				case 9://9/nGroupsAdd
					string desired = parts[1].Split('/')[0];
					int a = Int32.Parse(desired);
					AddGroupEvent(a);
				

				

					/*
					 * num
					 * if(num>0)
					 * GroupJoinedEvent(num);
					 * else
					 * GroupJoinedEvent(-1);
					 */

					break;
				case 10://seed/pos/
					string[] secondPart = parts[1].Split(new[] { '/' }, 2);//seed = secondPart[0]
					GameStartEvent(Int32.Parse(secondPart[0]));
					string[] thirdPart = secondPart[1].Split(new[] { '/' }, 2);// 
					Debug.Log("The coords are : " + thirdPart[0]);
					PlayerData.pData.startingRoom = thirdPart[0];
					EnemyListUpdateEvent(thirdPart[1]);//List of groups
					break;
				case 11://Direccion a la que mover el equipo
					ChangeDirectionEvent(parts[1]);
					break;
				case 12://Un grupo ha entrado a tu habitación actual
					SomeoneEnteredMyRoomEvent(parts[1]);
					break;

				case 13: //Obtenemos los que ya están dentro cuando nosotros somos los que entramos
					GetCurrenntGroupsInCoordsEvent(parts[1]);
					break;
				case 14://Damage
					break;
				default:
					break;
			}
		}
	}
}
