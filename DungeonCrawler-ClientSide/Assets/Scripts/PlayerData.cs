using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class PlayerData : MonoBehaviour
{
    public static PlayerData pData;
    string playerName;
    public  bool isLeader;
    public bool inGame;
	private void Update()
	{
    }
	void Awake()
    {
        playerName = "OfflineName";
        isLeader = true;
        inGame = false;
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
