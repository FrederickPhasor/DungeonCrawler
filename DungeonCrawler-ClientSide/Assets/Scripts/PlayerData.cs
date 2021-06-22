using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class PlayerData : MonoBehaviour
{
    public static PlayerData pData;
    string playerName;
    public  bool isLeader;
    public bool inGame;
    public string startingRoom;
    public List<CharacterData> teamPlayers = new List<CharacterData>();
    public CharacterData playerCharacter;
	
    public void SetPartners(List<string> partnersNames)
	{
        teamPlayers.Clear();
        foreach(string name in partnersNames)
		{
            CharacterData username = new CharacterData();
            username.SetName(name);
            teamPlayers.Add(username);
		}
	}
	void Awake()
    {
        playerCharacter = GetComponent<CharacterData>();
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
        playerCharacter.SetName(username);
    }

}
