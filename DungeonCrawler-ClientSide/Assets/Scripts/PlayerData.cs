using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class PlayerData : MonoBehaviour
{
    public static PlayerData pData;
    string playerName;
    public  bool isLeader;
     void Awake()
    {
        isLeader = true;
        if (pData != null)
            GameObject.Destroy(pData);
        else
            pData = this;
        DontDestroyOnLoad(this);
    }
    public string GetName()
	{
        return playerName;
	}
    public void SetName(string username)
	{
        playerName = username;
	}
}
