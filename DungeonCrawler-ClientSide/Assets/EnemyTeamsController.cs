using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTeamsController : MonoBehaviour
{
  
    [SerializeField] List<GameObject> enemyTeamsGameObjects = new List<GameObject>();
    [SerializeField] Transform corridorSpawnPoint, roomSpawnPoint;
    public TeamController hostTeam;
    public delegate void EnterAFight(GameObject group);
    public static event EnterAFight ATeamEnterTheFight;
    // Start is called before the first frame update
    void Start()
    {
        
    }

	private void OnEnable()
	{
        ServerController.SomeoneEnteredMyRoomEvent += AddGroupTrigger;
        ServerController.GetCurrenntGroupsInCoordsEvent += AddMultipleGroups;
    }
    // Update is called once per frame
    List<string> groups;
 
    void Update()
    {
        if (GameManager.sceneManager.groupsUpdated)
        {
            groups = GameManager.sceneManager.groups;
            
            foreach (var group in groups)
            {
                foreach(GameObject enemyGroupGo in enemyTeamsGameObjects)
				{
					if (enemyGroupGo.GetComponent<EnemyGroup>().Alive == false)
					{
                        enemyGroupGo.GetComponent<EnemyGroup>().groupIndex = group.Split(new[] { ':' }, 2)[0];
                        string[] players = group.Split(new[] { ':' }, 2)[1].Split('|');
                        enemyGroupGo.GetComponent<EnemyGroup>().Alive = true;
						foreach (string playerName in players)
						{
							enemyGroupGo.GetComponent<EnemyGroup>().AddPlayer(playerName);
						}
						break;
					}
				}
            }
            GameManager.sceneManager.groupsUpdated = false;
        }
		if (enemyTeamsUpdated)//Someone enters your room
		{
            foreach(GameObject teamGo in enemyTeamsGameObjects)
			{
                if (teamGo.GetComponent<EnemyGroup>().groupIndex == tempId)
				{
                    teamGo.SetActive(true);
                    ATeamEnterTheFight(teamGo);
                }
			}
		}
		if (multipleAdded)
		{
            while(currentlyConfrontedTeams.Count > 0)
			{
                string targetIndexGroup = currentlyConfrontedTeams.Pop();
                foreach (GameObject teamGo in enemyTeamsGameObjects)
                {
                    if (teamGo.GetComponent<EnemyGroup>().groupIndex == targetIndexGroup)
                    {
                        teamGo.SetActive(true);
                       ATeamEnterTheFight(teamGo);
                    }
                }
            }       
        }
    }
    bool enemyTeamsUpdated = false;
    string tempId;
    public void AddGroupTrigger(string id)
	{
        tempId = id;
        enemyTeamsUpdated = true;
	}
    Stack<string> currentlyConfrontedTeams = new Stack<string>();
    bool multipleAdded = false;
    void AddMultipleGroups(string indixes)
	{
        string[] parts = indixes.Split('/');
        parts[parts.Length - 1] = string.Empty; ;
        foreach(string index in parts)
		{
            if(index != "")
            currentlyConfrontedTeams.Push(index);
		}
        multipleAdded = true;
	}
}
