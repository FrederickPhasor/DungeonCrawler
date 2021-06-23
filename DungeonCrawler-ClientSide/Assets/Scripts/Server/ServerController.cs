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
	public delegate void AllRecentUserListUpdate(string names);
	public static event AllRecentUserListUpdate RecentUsersUpdatedEvent;
	public delegate void FightStateUpdate(string fightState);
	public static event FightStateUpdate FightStateUpdateEvent;
	public delegate void GetDamaged(string WhoAndamount);
	public static event GetDamaged GetDamagedEvent;
	public delegate void SomeoneDied(string whoAsked);
	public static event SomeoneDied SomeoneDiedEvent;
	public delegate void RecentSinglePlayerUpdate(string name);
	public static event RecentSinglePlayerUpdate RecentSinglePlayerGamesUpdateEvent;
	public delegate void PastGamesUpdate(string name);
	public static event PastGamesUpdate PastGamesEvent;

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
					SomeoneEnteredMyRoomEvent(parts[1].Split(new[] { '/' }, 2)[0]);
					break;

				case 13: //Hemos entrado a una habitación y estaba vacía, aprovechar quizá para dar algún premio o spawnear algo para pelear or wtver
					GetCurrenntGroupsInCoordsEvent(parts[1]);
					break;
				case 14://amountDamaged
					int amount = Convert.ToInt32(parts[1]);
					PlayerData.pData.health -= amount;
					break;
				case 15://lista jugadores recientes
					RecentUsersUpdatedEvent(parts[1]);
					break;
				case 16://resultados partidas vs 1 jugador
					RecentSinglePlayerGamesUpdateEvent(parts[1]);
					break;
				case 17:
					//Update fight state // grupo1/grupo2/grupo3/grupo4/|rondaActual/OrdenDeLosTurnos:jugadorQueJuega/
					//Los ordenes de turno son una cadena como 1:2_0:2_4:1_2:5 lo que indica que primero va el grupo 1 y dentro de ese grupo 1 le toca al jugador 1 o whatever
					string[] multipleParts = parts[1].Split(new[] { '|' }, 2);
					GetCurrenntGroupsInCoordsEvent(multipleParts[0]);//Pasarle los grupos
					FightStateUpdateEvent(multipleParts[1]);
					//Una vez ese jugador confirme que su jugada está preparada o bien que se le ha acabado el tiempo de acción, se mantiene en espera su acción
					//Se pasa al grupo 0 jugador 2, hace su acción o se agota el tiempo y se pone en espera su acción
					//Una vez se ha completado el ciclo entero, el servidor enviará en el caso 18 la cadena con la combinación de acciones tomadas y se calculará el resultado.

					break;
				case 18://Cadena de acciones de pelea, como máximo 4 acciones por turno
						//cada acción es un comando simple, un jugador del gr
					break;
				case 19://lista partidas recientes
					PastGamesEvent(parts[1]);
					break;
				case 20://Someone died, could be us
					string target = parts[1];//The one who died
					SomeoneDiedEvent(target.Split(new[] { '\0' }, 2)[0]);
					break;
				default:
					break;
			}
		}
	}
}
