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
		for(int i = 0; i < 4; i++)
		{
			ownTeam.transform.GetChild(i).gameObject.SetActive(false);
		}
		foreach(string partner in PlayerData.pData.teamPlayers )
		{
			bool found = false;
			int i = 0;
			while (found == false)
			{
				if (ownTeam.transform.GetChild(i).gameObject.activeInHierarchy == false)
				{
					found = true;
					ownTeam.transform.GetChild(i).gameObject.SetActive(true);
					ownTeam.transform.GetChild(i).gameObject.GetComponent<CharacterData>().SetName(partner);
				}
				else
					i++;
				if (i > 4)
				{
					break;
				}
			}
		}
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
		string cleanString = rawInfo.Split(new[] { '\0' }, 2)[0];
		//   rondaActual / OrdenDeLosTurnos:jugadorQueJuega /
		string[] cleanStringParts = cleanString.Split(new[] { '/' }, 2);
		try
		{
			turnNumber = Convert.ToInt32(cleanStringParts[0]);
		}
		catch { Debug.LogError("Error al convertir a entero :: Fightsmanager"); }
		
		string[] groupsActionsOrder = cleanStringParts[1].Split('_');
		groupsActionsOrder[groupsActionsOrder.Length - 1] = groupsActionsOrder[groupsActionsOrder.Length - 1].Split(new[] {'/'},2)[0] ;
		foreach (string group in groupsActionsOrder)
		{
			if(group != "")
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
			if (teamsOrder.Count > 0)
			{
				Foo(teamsOrder.Dequeue());
				fightStateUpdated = false;
				StartCoroutine(StartFighting());
			}
			else
			{
				fightStateUpdated = false;
				counting = false;
				//Si hemos sido el último equipo, avisar al servidor para la siguiente ronda.
				Debug.Log("La ronda se ha acabado");
				Debug.Log("Esperando a que el servidor de paso a la siguiente ronda");

				if (ourTurn)
				{
					ServerController.server.Ask("13/");
				}

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
		if (nextRoundDebug)
		{
			GoNextRound();
			nextRoundDebug = false;
		}
		
	}
	IEnumerator StartFighting()//Esto es solo para dar tiempo al jugador a entender que están a punto de empezar a pelear
	{
		Debug.Log("Starting fight in 5");
		yield return new WaitForSeconds(5);
		counting = true;
		Debug.Log("FightStarted!");

	}
	void GoNextRound()
	{
		turnNumber++;
		fightStateUpdated = true;
		counting = true;
	}
	public bool nextRoundDebug = false;
	void TimerEnded()
	{
		currenTimeLeft = timerPerRound;
		if (ourTurn)
		{
			int damage = DamageSelection(interactions.skillSelected);
			string target = interactions.GetSelectedUser();
			ServerController.server.Ask($"11/{damage}/{target}/");
			Debug.Log("We are attacking : " + target + "with : " + damage);
		}
		fightStateUpdated = true;
		counting = false;
	}
	public bool ourTurn; string playerTurn;
	[SerializeField] InteractionsHandler interactions;
	void Foo(string a)
	{
		//Check if we are the first group
		ourTurn = false;
		string[] parts = a.Split(':'); //Group:PlayerInsideThatGroup that plays
		int teamIndexToCompare = PlayerData.pData.temporalTeamIndex == -1 ? PlayerData.pData.mygroupIndex : PlayerData.pData.temporalTeamIndex;
		int nextPlayer = Convert.ToInt32(parts[1]);
		if (teamIndexToCompare.ToString() ==  parts[0])//Si entramos aquí es por que nos toca jugar (grupo)
		{
			int currentPlayersInMyTeam = PlayerData.pData.teamPlayers.Count;
			if(currentPlayersInMyTeam <= nextPlayer)
			{
				nextPlayer = 0;
			}
			GetComponent<EnemyTeamsController>().AllyCurrentPlayer(nextPlayer);
			string playerName = PlayerData.pData.teamPlayers[nextPlayer];
			Debug.Log("Le toca jugar a " + playerName);
			if (playerName == PlayerData.pData.GetName())//Si entra aquí es que nos toca a nosotros
			{
				Debug.LogWarning("Te toca jugar en 5 segundos");
				ourTurn = true;
			}
			else
			{
				Debug.LogWarning("Un compañero tuyo está jugando : " + parts[1]);
				
			}
		}
		else
		{
			GetComponent<EnemyTeamsController>().SetCurrentPlayer(teamIndexToCompare, Convert.ToInt32(parts[1]));
		}
	}

	int DamageSelection(int skill)
	{
		return skill switch
		{
			0 => 10,
			1 => -10,//heal
			2 => 0,//say Hi
			_ => 0,
		};
	}
}
