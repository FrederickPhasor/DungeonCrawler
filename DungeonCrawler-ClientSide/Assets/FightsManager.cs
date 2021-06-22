using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FightsManager : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI roundCounter;
	[SerializeField] TextMeshProUGUI timerText;
	private float currenTimeLeft;
	int turnNumber;
	bool fighting;
	public List<GameObject> enemyTeamsFighting = new List<GameObject>();
	[SerializeField] GameObject ownTeam;

	public delegate void EndOfRound();
	public static event EndOfRound EndOfRoundEvent;
	private void OnEnable()
	{
		EnemyTeamsController.ATeamEnterTheFight += JoinAFight;
	}
	private void OnDisable()
	{
		EnemyTeamsController.ATeamEnterTheFight -= JoinAFight;


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
			fighting = true;
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
		for (int i = 1; i < 3; i++)
		{
			if (location == 1)
			{
				if (fightPositionsRoom[i].GetComponent<FightingPos>().inUse == false)
				{
					fightPositionsRoom[i].GetComponent<FightingPos>().inUse = true;
					team.transform.position = fightPositionsRoom[i].transform.position;
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
		if (fighting)
		{
			roundCounter.text = turnNumber.ToString();
			currenTimeLeft -= Time.deltaTime;
			timerText.text = Mathf.Round(currenTimeLeft).ToString();
			if (currenTimeLeft <= 0.0f)
			{
				TimerEnded();
				currenTimeLeft = timerPerRound;
			}
		}
	
	}
	void TimerEnded()
	{
		turnNumber++;
		timerText.text = turnNumber.ToString();
		if (EndOfRoundEvent != null)
		{
			EndOfRoundEvent();
		}
	}

}
