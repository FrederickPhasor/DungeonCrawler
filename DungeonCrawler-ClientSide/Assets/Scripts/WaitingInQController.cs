using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaitingInQController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI displayText;
	[SerializeField] TextMeshProUGUI waitingTime;
	int currentGroups;
	
	private void Start()
	{
		currentGroups = 1;
	}
	private void OnEnable()
	{
		ServerController.AddGroupEvent += AddGroup;
	}
	private void OnDisable()
	{
		ServerController.AddGroupEvent -= AddGroup;
	}
	public void AddGroup(int amount)
	{
		currentGroups += amount;

		if (PlayerData.pData.groupIndexSet == false)
		{
			PlayerData.pData.mygroupIndex = currentGroups - 1;
		}
		updated = true;
	}
	bool waitingInQ = false;

	public void EnterQ()
	{
		ServerController.server.Ask("7/");
		displayText.text = "Buscando partida...";
		waitingInQ = true;
	}
	bool updated = false;
	public float targetTime = 120.0f;
	private void Update()
	{
		if (updated)
		{
			displayText.text = $"Esperando grupos : {currentGroups}/4 ";
			if (currentGroups == 4)
			{
				waitingInQ = false;
				updated = false;
			}
		}
		if (waitingInQ)
		{
			targetTime -= Time.deltaTime;
			waitingTime.text = "Tiempo de espera : " + Mathf.Round(targetTime) + " segundos";
			if (targetTime <= 0.0f)
			{
				TimerEnded();
				waitingInQ = false;
				waitingTime.text = "Iniciando partida...";
			}
		}
	}
	void TimerEnded()
	{
		//Start the game
		ServerController.server.Ask("8/");
	}
}
