using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTeamsController : MonoBehaviour
{
  
    [SerializeField] List<GameObject> enemyTeamsGameObjects = new List<GameObject>();
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
				}
			}
		}
		if (multipleAdded)
		{
            foreach (GameObject enemyTeam in enemyTeamsGameObjects)
            {
                if (enemyTeam.activeInHierarchy == false)
                {
                    string groupIndex = currentlyConfrontedTeams[0];
                    currentlyConfrontedTeams.Remove(groupIndex);
                    enemyTeam.SetActive(true);

                    //Pon nombre y stats al equipo recién spawneado
                    return;
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
    List<string> currentlyConfrontedTeams = new List<string>();
    bool multipleAdded = false;
    void AddMultipleGroups(string indixes)
	{
        string[] parts = indixes.Split('/');
        foreach(string index in parts)
		{
            if(index != "")
            currentlyConfrontedTeams.Add(index);
		}
        multipleAdded = true;
	}
}
