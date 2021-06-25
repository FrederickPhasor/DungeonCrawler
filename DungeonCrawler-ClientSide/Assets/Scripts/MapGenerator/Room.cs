using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room 
{
    public int[] teams;
    public List<Monster> monsters;
    public List<Treasure> treasures;
    public string ID_room;//01
    public int posX;//0
    public int posY;//1
    public int type; //0 if empty, 1 if room, 2 if corridor
}

[System.Serializable]
public class Monster
{
    string name;
    int health;
    int id;
}

[System.Serializable]
public class Treasure
{
    string name;
}
