using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamController : MonoBehaviour
{
    [SerializeField] List<GameObject> players;
    [SerializeField] float movementSpeed;
	float direction;

	private void Update()
	{
		//Keep moving in the last direction pressed as long as another key is pressed
		//for instance, 1 is right, -1 is left and 0 is static.
		MoveTeam();
	}
	void MoveTeam()
	{
		float nextStep = transform.position.x + movementSpeed * direction * Time.deltaTime;
		transform.position = new Vector3(nextStep, transform.position.y, transform.position.z);
	}
	public void SetTeamDirection(string dir)
	{
		if (dir == "1")
		{
			direction = 1;
		}
		else if(dir == "-1")
		{
			direction = -1;
		}
		else
		{
			direction = 0;
		}
	}
}
