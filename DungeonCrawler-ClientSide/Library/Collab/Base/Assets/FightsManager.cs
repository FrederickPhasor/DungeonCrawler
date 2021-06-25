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
		CombatChecker.CombatEnteredEvent += StartFight;
	}
	private void OnDisable()
	{
		CombatChecker.CombatEnteredEvent -= StartFight;
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
	void StartFight(GameObject enemyTeam)
	{
		if (fighting)//we are already in a fight, add another group to the fight
		{

		}
		else//We are not fighting yet, start one
		{
			turnNumber = 0;
			fighting = true;
			enemyTeamsFighting.Add(enemyTeam);
		}
	}
}
