using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
        ServerController.SomeoneDiedEvent += CheckWhoDiedTrigger;
        ServerController.GetDamagedEvent += GetDamaged;
    }
	private void OnDisable()
    {
        ServerController.SomeoneEnteredMyRoomEvent -= AddGroupTrigger;
        ServerController.GetCurrenntGroupsInCoordsEvent -= AddMultipleGroups;
        ServerController.SomeoneDiedEvent -= CheckWhoDiedTrigger;
        ServerController.GetDamagedEvent -= GetDamaged;
    }
    string userDamaged;
    int damagedAmount;
    bool someoneWasDamaged = false;
    public void GetDamaged(string amountAndTarget)
    {
        //damage/target
        string[] parts = amountAndTarget.Split(new[] { '/' }, 2);
        userDamaged = parts[1];
        damagedAmount = Convert.ToInt32(parts[0]);
        someoneWasDamaged = true;
    }
    string deathOneTemp;
    bool deathOcurred;
    public void CheckWhoDiedTrigger(string who)
	{
        deathOneTemp =who;
        if (who != PlayerData.pData.GetName())
		{
            deathOcurred = true;
        }
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
            if(currentlyConfrontedTeams.Count > 0)
			{
                foreach (GameObject teamGo in enemyTeamsGameObjects)
                {
                    string targetIndex = currentlyConfrontedTeams.Pop();
                    if (teamGo.GetComponent<EnemyGroup>().groupIndex == targetIndex)
                    {
                        teamGo.SetActive(true);
                        ATeamEnterTheFight(teamGo);
                    }
                }
                enemyTeamsUpdated = false;
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
		if (deathOcurred)
		{
            //Check everyfucking one 
            foreach(GameObject enemyTeam in enemyTeamsGameObjects)
			{
               for(int i = 0; i < enemyTeam.transform.childCount; i++)
				{
                    
       
				}
			}
            deathOcurred = false;
		}
		if (someoneWasDamaged)
		{
          Hurt(userDamaged, damagedAmount);
           someoneWasDamaged = false;
        }
    }
    void Hurt(string userDamaged, int amount)
	{
        Debug.Log("The user we want to hurt is " + userDamaged);
        foreach(GameObject enemyTeam in enemyTeamsGameObjects)
		{
            for(int i = 0; i < enemyTeam.transform.childCount; i++)
			{
				if (enemyTeam.transform.GetChild(i).gameObject.activeInHierarchy)
				{
                    string enemyName = enemyTeam.transform.GetChild(i).GetComponent<CharacterData>().GetName();
                    if(userDamaged == enemyName)
					{
                        if(amount > 0)
						{
                            enemyTeam.transform.GetChild(i).GetChild(0).GetComponent<AnimationsCaller>().TriggerHurtAnimation();
                            Debug.Log("The user ACTUALLY hurt is " + enemyName);
                        }
						else
						{
                            enemyTeam.transform.GetChild(i).GetChild(0).GetComponent<AnimationsCaller>().GetHealedAnimation();
                        }
                        return;
                        
                    }
                }
			}
		}
        foreach(GameObject ourTeamPlayer in hostTeam.players)
		{

            if (ourTeamPlayer.activeInHierarchy)
			{
				string teamMateName = ourTeamPlayer.GetComponent<CharacterData>().GetName();
				if (teamMateName == userDamaged)
				{
                    Debug.Log("A teamMate is going to be hurt :" + teamMateName);

                    if (amount > 0)
					{
                        ourTeamPlayer.transform.GetChild(0).GetComponent<AnimationsCaller>().TriggerHurtAnimation();
                    }
					else
					{
                        ourTeamPlayer.transform.GetChild(0).GetComponent<AnimationsCaller>().GetHealedAnimation();
                       
                    }
				}
			}
		}

	}
    bool enemyTeamsUpdated = false;
    string tempId;
    public void AddGroupTrigger(string id)
	{
        currentlyConfrontedTeams.Push(id);
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
            if(index == "" || index == PlayerData.pData.mygroupIndex.ToString())
			{
               
			}
			else
			{
                currentlyConfrontedTeams.Push(index);
            }
        }
        multipleAdded = true;
	}
    public void AllyCurrentPlayer(int index)
	{
        currentUserPlaying= hostTeam.players[index];
    }
    [SerializeField] GameObject currentUserPlaying;
    public void SetCurrentPlayer(int index, int groupIndex)
	{
        foreach(GameObject enemyGroup in enemyTeamsGameObjects)
		{
            int currentPlaying = 0;
            for(int i = 0; i < 4; i++)
			{
				if (enemyGroup.transform.GetChild(i).gameObject.activeInHierarchy)
				{
                    currentPlaying++;
				}
			}
            if (currentPlaying <= index)
            {
                index = 0;
            }

            if (enemyGroup.activeInHierarchy)
			{
                int enemyTeamIndex;
				try
				{
                    enemyTeamIndex = Convert.ToInt32(enemyGroup.GetComponent<EnemyGroup>().groupIndex);
                    if(enemyTeamIndex == groupIndex)
					{
                        currentUserPlaying =  enemyGroup.GetComponent<EnemyGroup>().players[index].gameObject;
                        Debug.Log("CurrentPlayer is Enemy");
                    }
				}
				catch
				{

				}

           	}
		}
    }
}
