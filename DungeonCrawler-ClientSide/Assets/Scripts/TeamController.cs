using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class TeamController : MonoBehaviour
{
    public  List<GameObject> players;
    [SerializeField] float movementSpeed;
	[SerializeField] float direction = 0;
	[SerializeField] TextMeshProUGUI availableRoomsText;
	bool canMove = true;
	[SerializeField] UISELECTOR mapData;

	private void OnEnable()
	{
		CombatChecker.CombatEnteredEvent += EnterFight;
		ServerController.SetRoomEvent += SetRoomDestination;
		ServerController.ChangeDirectionEvent += SetTeamDirection;
		UISELECTOR.DestinationSelectedEvent += DestinationSelect;

	}
	[SerializeField] GameObject localHostGo;
	private void Start()
	{
		currentRoom.ID_room = PlayerData.pData.startingRoom;
		MapController.currentRoom = currentRoom;
		MapController.curentRoomUpdate = true;
		for (int i = 0; i < PlayerData.pData.teamPlayers.Count; i++)
		{
			players[i].SetActive(true);
		}
		CheckForAvailableRooms();
		localHostGo.SetActive(true);
	}
	private void OnDisable()
	{
		CombatChecker.CombatEnteredEvent -= EnterFight;
		ServerController.SetRoomEvent -= SetRoomDestination;
		ServerController.ChangeDirectionEvent -= SetTeamDirection;
		UISELECTOR.DestinationSelectedEvent -= DestinationSelect;
	}
	public Room currentRoom;
	public Room desiredRoom;
	void SetRoomDestination(string roomCords) //018
	{
		currentRoom = mapData.GetRoom(roomCords);
		MapController.currentRoom = currentRoom;
		MapController.curentRoomUpdate = true;
	}
	void EnterFight(GameObject enemyTeam)
	{
		//Play animation or something
	}


	private void Update()
	{
		if (PlayerData.pData.isLeader)
		{
			if (Input.GetKeyDown(KeyCode.D))
			{
				ServerController.server.Ask("9/1");
			}
			else if (Input.GetKeyDown(KeyCode.A))
			{
				ServerController.server.Ask("9/-1");
			}
			else if (Input.GetKeyUp(KeyCode.D))
			{
				ServerController.server.Ask("9/0");
			}
			else if (Input.GetKeyUp(KeyCode.A))
			{
				ServerController.server.Ask("9/0");
			}
		}
		MoveTeam();
		
	}
	void MoveTeam()
	{
		if (canMove)
		{
			float nextStep = transform.position.x + movementSpeed * direction * Time.deltaTime;
			transform.position = new Vector3(nextStep, transform.position.y, transform.position.z);
			foreach (GameObject player in players)
			{
				if(player.activeInHierarchy)
					player.transform.GetChild(0).GetComponent<Animator>().SetBool("Walking", Mathf.Abs(direction) == 1);
			}
		}
	}
	public void SetTeamDirection(string dir)
	{
		int number = Int32.Parse(dir[0].ToString());
		if (number != 2)
		{
			direction = number;
			return;
		}
		direction = -1;
	}
	public void CanMove(bool state)
	{
		canMove = state;
	}
	
	void DestinationSelect(Room desiredRoom)
	{
		CheckForAvailableRooms();
		if (availableRooms.Contains(desiredRoom) && canChangeRoom)
		{
			Debug.Log("The room detected is available");
			//Ask the server that we are entering this position

			currentRoom = desiredRoom;
			if (currentRoom.type == 1){//Rooms
				currentRoom = desiredRoom;
				ServerController.server.Ask("10/" + desiredRoom.ID_room);
				GoToRoom();
			}
			else if(currentRoom.type == 2)//Corridorss
			{
				currentRoom = desiredRoom;
				ServerController.server.Ask("10/" + desiredRoom.ID_room);
				GoToCorridor();
				
			}
			MapController.currentRoom = currentRoom;
			MapController.curentRoomUpdate = true;
			CheckForAvailableRooms();
		}
		else
		{
			Debug.Log("The room detected is  NOT available");

		}
		//Check if we can go there, if it is so, accept destination and do stuff
		//string currentPos = ;  

	}
	[SerializeField] Transform corridorSpawnPoint;
	void GoToCorridor()
	{
		transform.position = corridorSpawnPoint.position;
	}
	[SerializeField] Transform roomSpawnPoint;
	void GoToRoom()	
	{
		transform.position = roomSpawnPoint.position;
	}
	public List<Room> availableRooms = new List<Room>();
	void CheckForAvailableRooms()
	{
		availableRooms.Clear();
		int currentX, currentY;
		currentX = currentRoom.posX;
		currentY = currentRoom.posY;
		Room north, south, east, west;

		if (currentX > 0 && currentX < 18)
		{
			if(currentY > 0 && currentY < 8)
			{
				//We can check in every direction
				north = mapData.GetRoomByCords(currentX, currentY - 1);
				south = mapData.GetRoomByCords(currentX, currentY + 1);
				east  = mapData.GetRoomByCords(currentX + 1, currentY );
				west  = mapData.GetRoomByCords(currentX - 1, currentY );
				if (north.type == 1 || north.type == 2)
				{
					availableRooms.Add(north);
				}
				if (south.type == 1 || south.type == 2)
				{
					availableRooms.Add(south);
				}
				if (east.type == 1 || east.type == 2)
				{
					availableRooms.Add(east);
				}
				if (west.type == 1 || west.type == 2)
				{
					availableRooms.Add(west);
				}
			}
		}
		else if (currentX == 0)
		{
			if(currentY == 0)
			{
				south = mapData.GetRoomByCords(currentX, currentY + 1);
				east = mapData.GetRoomByCords(currentX + 1, currentY);
			
				if (south.type == 1 || south.type == 2)
				{
					availableRooms.Add(south);
				}
				if (east.type == 1 || east.type == 2)
				{
					availableRooms.Add(east);
				}
				
				// we can check for south and east
			}
			else if(currentY < 8)
			{
				north = mapData.GetRoomByCords(currentX, currentY - 1);
				east = mapData.GetRoomByCords(currentX + 1, currentY);
				south = mapData.GetRoomByCords(currentX, currentY + 1);
				if (north.type == 1 || north.type == 2)
				{
					availableRooms.Add(north);
				}
				if (south.type == 1 || south.type == 2)
				{
					availableRooms.Add(south);
				}
				if (east.type == 1 || east.type == 2)
				{
					availableRooms.Add(east);
				}
				
				//we can check for north, east and south
			}
			else if( currentY == 8)
			{
				north = mapData.GetRoomByCords(currentX, currentY - 1);
				east = mapData.GetRoomByCords(currentX + 1, currentY);
				if (north.type == 1 || north.type == 2)
				{
					availableRooms.Add(north);
				}
				
				if (east.type == 1 || east.type == 2)
				{
					availableRooms.Add(east);
				}
				
				//we can check north and east
			}
		}
		else
		{
			if (currentY == 0)
			{
				west = mapData.GetRoomByCords(currentX - 1, currentY);
				south = mapData.GetRoomByCords(currentX, currentY + 1);
			
				if (south.type == 1 || south.type == 2)
				{
					availableRooms.Add(south);
				}
			
				if (west.type == 1 || west.type == 2)
				{
					availableRooms.Add(west);
				}
				//we can check for west and south
			}
			else if (currentY < 8)
			{
				north = mapData.GetRoomByCords(currentX, currentY - 1);
				west = mapData.GetRoomByCords(currentX - 1, currentY);
				south = mapData.GetRoomByCords(currentX, currentY + 1);
				if (north.type == 1 || north.type == 2)
				{
					availableRooms.Add(north);
				}
				if (south.type == 1 || south.type == 2)
				{
					availableRooms.Add(south);
				}
				if (west.type == 1 || west.type == 2)
				{
					availableRooms.Add(west);
				}
				//we can check for north, west and south
			}
			else if (currentY == 8)
			{
				west = mapData.GetRoomByCords(currentX - 1, currentY);
				north = mapData.GetRoomByCords(currentX, currentY - 1);
				if (north.type == 1 || north.type == 2)
				{
					availableRooms.Add(north);
				}
				if (west.type == 1 || west.type == 2)
				{
					availableRooms.Add(west);
				}
				// we can only check for west and north
			}

		}
	}
	bool canChangeRoom = false;
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.name.Contains("Point"))
		{
			canChangeRoom = true;
		}
	}
	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.name.Contains("Point"))

		{
			canChangeRoom = false;
		}
	}
}
