using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;


public class MapController : AbstractMapGenerator
{
    
    [SerializeField]
    [Range(0f, 1f)]
    float emptySpacePercentage = 0.63f;
    [SerializeField]
    [Range(4, 6)]
    int amountGroups = 6;
    public Room[,] allSquares;
    public HashSet<Room> roomList = new HashSet<Room>();
    public List<Room> RoomList = new List<Room>();
    public List<Room> corridors = new List<Room>();
    public static MapController map;
    public Sprite straightSprite, LShapeSprite, threeWaySprite, fourWaySprite, roomSprite, clearedRoomSprite;
    Color transparent = Color.white;

    public static Room currentRoom;
    public static bool curentRoomUpdate;
    int oldX = 0;//Inizializar estas vars a la pos inicial
    int oldY = 0;
    Color currentRoomColor = new Color(0.8207547f, 0.5246393f, 0f, 1f);


    public delegate void ClickInRoom(Room[,] allSquares);
    public static event ClickInRoom RoomsGenerationDoneEvent;



    public void Start()
    {
        curentRoomUpdate = false;
        var numberSeed = NumbersIn(seed);
        transparent.a = 0f;
        CreateMap(19, 9);

    }

	public void Update()
	{
        if(curentRoomUpdate)
        {
            UpdateCurrentRoom();
            curentRoomUpdate = false;
        }

    }


    public void CreateMap(int sizeX, int sizeY)
    {
        System.Random roomRand = new System.Random(seed);
        allSquares = new Room[sizeX, sizeY];
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                allSquares[x, y] = new Room();
            }
        }
        //int emptyRoomCount = Mathf.RoundToInt(rooms.Length * corridorPercentage);
        bool lastRoom = false;      


        for (int col = 0; col < sizeX; col++)
        {
            for (int row = 0; row < sizeY; row++)
            {
                switch (amountGroups)
                {
                    case 4:
                        if ((col == 0 && (row == 0 || row == 8)) || (col == 18 && (row == 0 || row == 8)))//Set corner rooms
                        {
                            SetRoom(allSquares, col, row);
                            roomList.Add(allSquares[col, row]);
                            break;
                        }
                        else if ((col == 0 && row == 1) || (col == 1 && row == 0) ||
                                 (col == 0 && row == 7) || (col == 1 && row == 8) ||
                                 (col == 17 && row == 0) || (col == 18 && row == 1) ||
                                 (col == 17 && row == 8) || (col == 18 && row == 7))//Set next to corners corridor
                        {
                            SetEmpty(allSquares, col, row);
                            break;
                        }
                        goto default;
                    case 5:
                        if (col == 9 && row == 0)//Set Top mid Room
                        {
                            SetRoom(allSquares, col, row);
                            roomList.Add(allSquares[col, row]);
                            break;
                        }
                        else if ((col == 8 && row == 0) || (col == 9 && row == 1) || (col == 10 && row == 0))
                        {
                            SetEmpty(allSquares, col, row);
                            break;
                        }
                        goto case 4;
                    case 6:
                        if (col == 9 && row == 8)//Set Bottom mid Room
                        {
                            SetRoom(allSquares, col, row);
                            roomList.Add(allSquares[col, row]);
                            break;
                        }
                        else if ((col == 8 && row == 8) || (col == 9 && row == 7) || (col == 10 && row == 8))
                        {
                            SetEmpty(allSquares, col, row);
                            break;
                        }
                        goto case 5;
                    default:
                        if (roomRand.NextDouble() >= emptySpacePercentage && lastRoom == false)///Room
                        {
                            if (col - 1 >= 0 && allSquares[col - 1, row].type == 1)//if we on second column and room left of it
                            {
                                lastRoom = SetEmpty(allSquares, col, row);
                            }
                            else
                            {
                                lastRoom = SetRoom(allSquares, col, row);
                                roomList.Add(allSquares[col, row]);
                            }
                        }
                        else ///Corridor
                        {
                            lastRoom = SetEmpty(allSquares, col, row);
                        }
                        /*if (roomRand.NextDouble() <= emptyRoomPercentage && col !=0 && col != 18 && row != 0 || row != 8)
                        {
                           SetCorridor(rooms, col, row);
                        }*/
                        break;
                }
                allSquares[col, row].ID_room = $"{col.ToString()}{row.ToString()}";
                allSquares[col, row].posX = col;
                allSquares[col, row].posY = row;

            }
        }
        List<Room> tempRoomList = new List<Room>();
        foreach (var room in roomList)
        {
            tempRoomList.Add(room);
        }

        corridors = ConnectRooms(tempRoomList, allSquares);

        List<Room> sortedCorridors = corridors.Except(roomList).ToList();
        corridors.Clear();
        corridors = sortedCorridors;

        //Array.Clear(allSquares, 0, allSquares.Length);
        foreach (var room in sortedCorridors)
        {
            int col = room.posX;
            int row = room.posY;
            SetCorridor(allSquares, col, row);
        }
        RoomList = roomList.ToList();
        foreach (var room in roomList)
        {
            int col = room.posX;
            int row = room.posY;
            SetRoom(allSquares, col, row);
        }
        RoomsGenerationDoneEvent(allSquares);
        MakeConditionals(allSquares, corridors);
    }
            

    private bool SetEmpty(Room[,] rooms, int col, int row)
    {
        bool lastRoom;
        rooms[col, row].type = 0;
        mapHolder.transform.GetChild(19 * row + col).gameObject.GetComponent<Image>().color = transparent;

        lastRoom = false;
        return lastRoom;
    }

    private bool SetRoom(Room[,] rooms, int col, int row)
    {
        bool lastRoom;
        rooms[col, row].type = 1;
        mapHolder.transform.GetChild(19 * row + col).gameObject.GetComponent<Image>().sprite = roomSprite;
        lastRoom = true;
        return lastRoom;
    }    

    private void SetCorridor(Room[,] rooms, int col, int row)
    {
        rooms[col, row].type = 2;
        mapHolder.transform.GetChild(19 * row + col).gameObject.GetComponent<Image>().color = Color.grey;
    }

    public int[] NumbersIn(int value)
    {
        var numbers = new Stack<int>();

        for (; value > 0; value /= 10)
            numbers.Push(value % 10);

        return numbers.ToArray();
    }

    private List<Room> ConnectRooms(List<Room> tempRoomList, Room[,] allSquares)
    {
        List<Room> corridors = new List<Room>();
        var currentRoom = tempRoomList[0];
        tempRoomList.Remove(currentRoom);
        while (tempRoomList.Count >0)
        {
            Room next = FindNextRoom(currentRoom, tempRoomList);
            tempRoomList.Remove(next);
            List<Room> newCorridor = CreateCorridor(currentRoom, next, allSquares);
            currentRoom = next;
            corridors.AddRange(newCorridor);
        }
        return corridors;
    }

    private List<Room> CreateCorridor(Room currentRoom, Room next, Room[,] allSquares)
    {
        List<Room> corridor = new List<Room>();
        var current = currentRoom;
        corridor.Add(current);
        while (current.posY != next.posY)
        {
            if (next.posY > current.posY)
            {
                current = allSquares[current.posX, current.posY + 1];
            }
            if (next.posY < current.posY)
            {
                current = allSquares[current.posX, current.posY - 1];
            }
            corridor.Add(current);
        }
        while(current.posX != next.posX)
        {
            if(next.posX > current.posX)
            {
                current = allSquares[current.posX + 1, current.posY];
            }
            if (next.posX < current.posX)
            {
                current = allSquares[current.posX - 1, current.posY];
            }
            corridor.Add(current);
        }
        return corridor;
    }

    private Room FindNextRoom(Room currentRoom, List<Room> roomList)
    {
        Room closest = roomList[0];
        float distance = float.MaxValue;
        foreach (var room in roomList)
        {
            float currentDistance = GetCurrentDistance(room, currentRoom);
            if(currentDistance<distance)
            {
                distance = currentDistance;
                closest = room;
            }
        }
        return closest;
    }

    private float GetCurrentDistance(Room room, Room currentRoom)
    {
        Vector2Int roomVec = Vector2Int.zero;
        Vector2Int currentRoomVec = Vector2Int.zero;
        roomVec.Set(room.posX, room.posY);
        currentRoomVec.Set(currentRoom.posX, currentRoom.posY);
        float distance = Vector2.Distance(roomVec, currentRoomVec);
        return distance;
    }

    private void MakeConditionals(Room[,] allSquares, List<Room> corridors)
    {
        foreach (var corridor in corridors)
        {
            string neighboursBinarytype = "";

            var neighbourPositionX = corridor.posX;
            var neighbourPositionY = corridor.posY - 1;
            if (neighbourPositionX >= 0 && neighbourPositionY >= 0 && neighbourPositionX <= 18 && neighbourPositionY <= 8)
            {
                if (allSquares[neighbourPositionX, neighbourPositionY].type != 0)
                {
                    neighboursBinarytype += "1";
                }
                else
                {
                    neighboursBinarytype += "0";
                }
            }
            else
            {
                neighboursBinarytype += "0";
            }
            neighbourPositionX = corridor.posX + 1;
            neighbourPositionY = corridor.posY;
            if (neighbourPositionX >= 0 && neighbourPositionY >= 0 && neighbourPositionX <= 18 && neighbourPositionY <= 8)
            {
                if (allSquares[neighbourPositionX, neighbourPositionY].type != 0)
                {
                    neighboursBinarytype += "1";
                }
                else
                {
                    neighboursBinarytype += "0";
                }
            }
            else
            {
                neighboursBinarytype += "0";
            }
            neighbourPositionX = corridor.posX;
            neighbourPositionY = corridor.posY + 1;
            if (neighbourPositionX >= 0 && neighbourPositionY >= 0 && neighbourPositionX <= 18 && neighbourPositionY <= 8)
            {
                if (allSquares[neighbourPositionX, neighbourPositionY].type != 0)
                {
                    neighboursBinarytype += "1";
                }
                else
                {
                    neighboursBinarytype += "0";
                }
            }
            else
            {
                neighboursBinarytype += "0";
            }
            neighbourPositionX = corridor.posX - 1;
            neighbourPositionY = corridor.posY;
            if (neighbourPositionX >= 0 && neighbourPositionY >= 0 && neighbourPositionX <= 18 && neighbourPositionY <= 8)
            {
                if (allSquares[neighbourPositionX, neighbourPositionY].type != 0)
                {
                    neighboursBinarytype += "1";
                }
                else
                {
                    neighboursBinarytype += "0";
                }
            }
            else
            {
                neighboursBinarytype += "0";
            }
            SelectConditionalModel(corridor, neighboursBinarytype);
        }
    }

    private void SelectConditionalModel(Room corridor, string binarytype)
    {
        int typeAsInt = Convert.ToInt32(binarytype,2);
        if (ConditionalByteTypes.corridorStraight.Contains(typeAsInt))
        {
            mapHolder.transform.GetChild(19 * corridor.posY + corridor.posX).gameObject.GetComponent<Image>().sprite = straightSprite;
            if (typeAsInt == 10)
                mapHolder.transform.GetChild(19 * corridor.posY + corridor.posX).gameObject.GetComponent<Image>().transform.Rotate(Vector3.forward * 90);
        }
        if (ConditionalByteTypes.corridorLShape.Contains(typeAsInt))
        {
            mapHolder.transform.GetChild(19 * corridor.posY + corridor.posX).gameObject.GetComponent<Image>().sprite = LShapeSprite;
            if (typeAsInt == 12)
                mapHolder.transform.GetChild(19 * corridor.posY + corridor.posX).gameObject.GetComponent<Image>().transform.Rotate(Vector3.forward * 180);
            if (typeAsInt == 9)
                mapHolder.transform.GetChild(19 * corridor.posY + corridor.posX).gameObject.GetComponent<Image>().transform.Rotate(Vector3.back * 90);
            if (typeAsInt == 6) 
                mapHolder.transform.GetChild(19 * corridor.posY + corridor.posX).gameObject.GetComponent<Image>().transform.Rotate(Vector3.forward * 90);
        }
        if (ConditionalByteTypes.corridor3Way.Contains(typeAsInt))
        {
            mapHolder.transform.GetChild(19 * corridor.posY + corridor.posX).gameObject.GetComponent<Image>().sprite = threeWaySprite;
            if (typeAsInt == 13)
                mapHolder.transform.GetChild(19 * corridor.posY + corridor.posX).gameObject.GetComponent<Image>().transform.Rotate(Vector3.forward * 180);
            if (typeAsInt == 14)
                mapHolder.transform.GetChild(19 * corridor.posY + corridor.posX).gameObject.GetComponent<Image>().transform.Rotate(Vector3.forward * 90);
            if (typeAsInt == 11)
                mapHolder.transform.GetChild(19 * corridor.posY + corridor.posX).gameObject.GetComponent<Image>().transform.Rotate(Vector3.back * 90);
        }
        if (ConditionalByteTypes.corridor4Way.Contains(typeAsInt))
        {
            mapHolder.transform.GetChild(19 * corridor.posY + corridor.posX).gameObject.GetComponent<Image>().sprite = fourWaySprite;
        }
    }
    protected override void CreateNewMap()
    {
        roomList.Clear();
        corridors.Clear();
        CreateMap(19, 9);
    }

    private void UpdateCurrentRoom()
    {        
        int x = currentRoom.posX;
        int y = currentRoom.posY;
        mapHolder.transform.GetChild(19 * oldY + oldX).gameObject.GetComponent<Image>().color = Color.grey;
        mapHolder.transform.GetChild(19 * y + x).gameObject.GetComponent<Image>().color = currentRoomColor;
        oldY = y;
        oldX = x;
    }
}
