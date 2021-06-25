using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class PlayerData : MonoBehaviour
{
    public static PlayerData pData;
    string playerName;
    public int health = 30;
    public  bool isLeader;
    public bool inGame;
    public string startingRoom;
    public List<string> teamPlayers = new List<string>();
    public CharacterData playerCharacter;
    public int mygroupIndex;
    public bool groupIndexSet = false;
    public int temporalTeamIndex;
	private void OnEnable()
	{
      
	}
    public void FillTeam(int seed)
	{
        //we dont use seed

	}
	public void SetPartners(List<string> partnersNames)
	{
        teamPlayers.Clear();
        foreach(string name in partnersNames)
		{
             teamPlayers.Add(name);
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
        teamPlayers.Add(username);

    }
	private void Update()
	{
		if(health <= 0)
		{
            ServerController.server.Ask("12/" + playerName);
		}
	}

}
