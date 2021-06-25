using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DebugChangeRoom : MonoBehaviour
{
	[SerializeField] UISELECTOR mapData;
	[SerializeField]  TMP_InputField input;
	[SerializeField] Transform corridorSpawnPoint, roomSpawnPoint;
	[SerializeField] Transform teamTransform;
	public void GoToRoomDebug()
	{
		Room destinationRoom = mapData.GetRoom(input.text);
		if (destinationRoom.type == 1)
		{//Rooms
			ServerController.server.Ask("10/" + destinationRoom.ID_room);
			
		}
		else if (destinationRoom.type == 2)//Corridorss
		{
			ServerController.server.Ask("10/" + destinationRoom.ID_room);
		}
	}
}
