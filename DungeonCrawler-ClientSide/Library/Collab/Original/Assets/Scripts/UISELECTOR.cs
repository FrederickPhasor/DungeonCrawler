using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
public class UISELECTOR : MonoBehaviour
{
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    public static Room[,] roomList;
    public delegate void DestinationSelected(Room room);
    public static event DestinationSelected DestinationSelectedEvent;
    void Update()
    {

        //Check if the left Mouse button is clicked
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //Set up the new Pointer Event
            m_PointerEventData = new PointerEventData(m_EventSystem);
            //Set the Pointer Event Position to that of the mouse position
            m_PointerEventData.position = Input.mousePosition;
            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();
            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);
            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.name.Contains("RoomBoxUI"))
                {
                    string name = result.gameObject.name;
                    string[] parts = name.Split(new[] { ' ' }, 2);
                    string[] coord = parts[1].Split(new[] { '-' }, 2);
                    int coordX = Convert.ToInt32(coord[0]);
                    int coordY = Convert.ToInt32(coord[1]);
                    DestinationSelectedEvent(roomList[coordX, coordY]);
                    print(roomList[coordX, coordY]);
                }
                if (result.gameObject.name.Contains("Skill"))
                {
                    Debug.Log("Hit " + result.gameObject.name);
                }
            }
        }
    }

    private void OnEnable()
    {
        MapController.RoomsGenerationDoneEvent += RoomsGeneratedList;
    }
    private void OnDisable()
    {
        MapController.RoomsGenerationDoneEvent -= RoomsGeneratedList;
    }
    private void RoomsGeneratedList(Room[,] rooms)
    {
        roomList = rooms;
    }
    private void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();
    }
    public Room GetRoom(string roomId)
	{
        foreach(Room room in roomList)
		{
            if(room.ID_room == roomId)
			{
                return room;
			}
		}
        return null;
	}
    public Room GetRoomByCords(int x, int y)
	{
        return roomList[x, y];
	}
}
