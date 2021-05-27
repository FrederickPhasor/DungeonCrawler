using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public class MapController : MonoBehaviour
{
    
    [SerializeField]
    [Range(0.1f, 1)]
    float emptyRoom = 0.2f;
    [SerializeField] GameObject roomGameObject, mapHolder;
    [SerializeField]
    [Range(1, 10000000)]
    int seed = 5034143;//size emptyRoom 
 


    public void Start()
    {
        CreateMap(19,9);
    }
	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
            CreateMap(19, 9);
		}
	}

	public void CreateMap(int sizeX, int sizeY)
    {
        Room[,] rooms;
        System.Random roomRand = new System.Random(seed);
        rooms = new Room[sizeX, sizeY];
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                rooms[x, y] = new Room();
            }
        }        
        int emptyRoomCount = Mathf.RoundToInt(rooms.Length * emptyRoom);
        bool lastRoom = false;

        for (int col = 0; col < sizeX; col++)
        {
            for (int row = 0; row < sizeY; row++)
            {
                if( (col == 0 && (row == 0 || row == 8)) || (col == 18 && (row == 0 || row == 8)) )//Set corner rooms
                {
                    lastRoom = SetRoom(rooms, col, row);
                }
                else if( (col == 0 && row == 1) || (col == 1 && row == 0) || 
                         (col == 0 && row == 7) || (col == 1 && row == 8) || 
                         (col == 17 && row == 0) || (col == 18 && row == 2) ||
                         (col == 17 && row == 8) || (col == 18 && row == 7))//Set next to corners corridor
                {
                    lastRoom = SetCorridor(rooms, col, row);
                }
                else if (roomRand.NextDouble() >= emptyRoom && lastRoom == false )///Room
                {  
                    if(col - 1 >= 0 && rooms[col - 1, row].type == 1)//if we on second column and room left of it
				    {
                        lastRoom = SetCorridor(rooms, col, row);
                    }
				    else
				    {
                        lastRoom = SetRoom(rooms, col, row);
                    }                    
                }
                else ///Corridor
                {
                    lastRoom = SetCorridor(rooms, col, row);
                }
                rooms[col, row].ID_room = $"{col.ToString()}{row.ToString()}";
            }
        }
    }

    private bool SetCorridor(Room[,] rooms, int col, int row)
    {
        bool lastRoom;
        rooms[col, row].type = 0;
        mapHolder.transform.GetChild(19 * row + col).gameObject.GetComponent<Image>().color = Color.red;
        lastRoom = false;
        return lastRoom;
    }

    private bool SetRoom(Room[,] rooms, int col, int row)
    {
        bool lastRoom;
        rooms[col, row].type = 1;
        mapHolder.transform.GetChild(19 * row + col).gameObject.GetComponent<Image>().color = Color.blue;
        lastRoom = true;
        return lastRoom;
    }
}
