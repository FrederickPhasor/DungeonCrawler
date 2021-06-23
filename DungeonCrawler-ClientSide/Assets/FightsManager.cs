using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class FightsManager : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI roundCounter;
	[SerializeField] TextMeshProUGUI timerText;
	private float currenTimeLeft;
	int turnNumber;
	bool counting = false;
	public List<GameObject> enemyTeamsFighting = new List<GameObject>();
	[SerializeField] GameObject ownTeam;

	public delegate void EndOfRound();
	public static event EndOfRound EndOfRoundEvent;
	private void OnEnable()
	{
		EnemyTeamsController.ATeamEnterTheFight += JoinAFight;
		ServerController.FightStateUpdateEvent += FightStateUpdateTrigger;
		
	}
	private void OnDisable()
	{
		EnemyTeamsController.ATeamEnterTheFight -= JoinAFight;
		ServerController.FightStateUpdateEvent -= FightStateUpdateTrigger;
	}
	
	Queue<string> teamsOrder = new Queue<string>();
	bool fightStateUpdated = false;
	public void FightStateUpdateTrigger(string rawInfo)
	{
		//   rondaActual / OrdenDeLosTurnos:jugadorQueJuega /
		string[] parts = rawInfo.Split(new[] { '/' }, 2);
		turnNumber = Convert.ToInt32(parts[0]);
		string[] groupsActionsOrder = parts[1].Split('_');
		foreach(string group in groupsActionsOrder)
		{
			teamsOrder.Enqueue(group);
		}
		fightStateUpdated = true;
	}
	bool fightHappening = false;
	[SerializeField] List<GameObject> fightPositionsCorridor = new List<GameObject>();
	[SerializeField] List<GameObject> fightPositionsRoom = new List<GameObject>();
	[SerializeField] TeamController ownTeamController;
	public void JoinAFight(GameObject team)
	{
		int location = ownTeamController.currentRoom.type;
		if (fightHappening == false)
		{
			fightHappening = true;
			
			if (location == 1)
			{
				ownTeam.transform.position = fightPositionsRoom[0].transform.position;
			}
			else
			{
				ownTeam.transform.position = fightPositionsCorridor[0].transform.position;
			}
		}
		for (int i = 1; i < 3; i++)//añade un grupo a la vez a la pelea.
		{
			if (location == 1)
			{
				if (fightPositionsRoom[i].GetComponent<FightingPos>().inUse == false)
				{
					fightPositionsRoom[i].GetComponent<FightingPos>().inUse = true;
					team.transform.position = fightPositionsRoom[i].transform.position;
					break;
				}
			}
			else
			{
				if(fightPositionsCorridor[i].GetComponent<FightingPos>().inUse == false)
				{
					fightPositionsCorridor[i].GetComponent<FightingPos>().inUse = true;
					team.transform.position = fightPositionsCorridor[i].transform.position;
					break;
				}
			}
		}
	}
	[SerializeField]float timerPerRound;

	private void Start()
	{
		currenTimeLeft = timerPerRound;
		turnNumber = 0;
	}
	private void Update()
	{
		if (fightStateUpdated)
		{
			if(teamsOrder.Count > 0)
			{
				Foo(teamsOrder.Dequeue());
				fightStateUpdated = false;
				counting = true;
			}
			else
			{
				fightStateUpdated = false;
				counting = false;
			}
		}
		if (counting)
		{
			roundCounter.text = turnNumber.ToString();
			currenTimeLeft -= Time.deltaTime;
			timerText.text = Mathf.Round(currenTimeLeft).ToString();
			if (currenTimeLeft <= 0.0f)
			{
				TimerEnded();//A player has finished this tourn, the next goes
				currenTimeLeft = timerPerRound;
			}
		}
		

	}
	void TimerEnded()
	{
		currenTimeLeft = timerPerRound;
		fightStateUpdated = true;
	}
	[SerializeField] InteractionsHandler interactions;
	void Foo(string a)
	{
		//Check if we are the first group
		string[] parts = a.Split(':'); //Group/PlayerInsideThatGroup that plays
		if(PlayerData.pData.mygroupIndex.ToString() ==  parts[0])//Si entramos aquí es por que nos toca jugar
		{
			Debug.Log("Tell server : " + interactions.GetSelectedUser()+ "/" + DamageSelection(interactions.skillSelected) + "/");
			ServerController.server.Ask($"11/{interactions.GetSelectedUser()}/{DamageSelection(interactions.skillSelected)}/");
		}
		else
		{
			//Se debería indicar de alguna forma a quien le toca exactamente
		}
	}
	int DamageSelection(int skill)
	{
		switch (skill)
		{
			case 0: return 10;
			case 1: return -10; //heal
			default: return 0;
		}
	}
}
