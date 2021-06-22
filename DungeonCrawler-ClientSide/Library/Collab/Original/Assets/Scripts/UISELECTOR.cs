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
    Room[,] roomList;

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

                    //Debug.Log("Hit " + result.gameObject.name);
                    Debug.Log("Coordinates " + coordX + ":" + coordY);
                    Debug.Log("Room Type " + roomList[coordX, coordY].type);
                }
                if (result.gameObject.name.Contains("Skill"))
                {
                    Debug.Log("Hit " + result.gameObject.name);
                }
                //mapHolder.transform.GetChild(19 * row + col).gameObject.GetComponent<Image>().color = Color.gray;
            }
        }
    }

    private void OnEnable()
    {
        MapController.ClickInRoomEvent += ClickInRoom;
    }
    private void OnDisable()
    {
        MapController.ClickInRoomEvent -= ClickInRoom;
    }
    private void ClickInRoom(Room[,] rooms)
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
}
