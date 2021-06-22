using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamController : MonoBehaviour
{
    [SerializeField] List<GameObject> players;
    [SerializeField] float movementSpeed;
	[SerializeField] float direction;
	int simultaneousCombats = 0;
	private void OnEnable()
	{
		CombatChecker.CombatEnteredEvent += EnterFight;
	}
	private void OnDisable()
	{
		//FightsManager.EndOfRoundEvent -=
		CombatChecker.CombatEnteredEvent -= EnterFight;
	}
	void EnterFight(GameObject enemyTeam)
	{
		//Play animation or something

	}


	private void Update()
	{
		MoveTeam();
	}
	void MoveTeam()
	{
		float nextStep = transform.position.x + movementSpeed * direction * Time.deltaTime;
		transform.position = new Vector3(nextStep, transform.position.y, transform.position.z);
		foreach(GameObject player in players)
		{
			player.transform.GetChild(0).GetComponent<Animator>().SetBool("Walking", Mathf.Abs(direction) == 1);
		}
	}
	public void SetTeamDirection(string dir)
	{

		if (dir == "1")
		{
			direction = 1;
		}
		else
		{
			direction = 0;
		}

	}
	public void SetDirection(int direction)
	{
		this.direction = direction;
	}
	public void CanMove(bool state)
	{
		
	}
}
