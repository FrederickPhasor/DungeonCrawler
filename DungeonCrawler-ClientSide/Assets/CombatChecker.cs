using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatChecker : MonoBehaviour
{
	public delegate void CombatEntered(GameObject enemyTeam);
	public static event CombatEntered CombatEnteredEvent;
	[SerializeField] float lerpValue;
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(collision.gameObject.GetComponent<EnemyGroup>() != null)
		{
			GameObject enemyTeam = collision.gameObject;
			CombatEnteredEvent?.Invoke(collision.gameObject);
			Vector3 desiredPosition = transform.position;
			enemyTeam.GetComponent<TeamController>().CanMove(false);
			while(Mathf.Abs((transform.position - enemyTeam.transform.position).magnitude) > 0.2f)
			{
				enemyTeam.transform.position = Vector3.Lerp(enemyTeam.transform.position, transform.position, lerpValue);
			}
			

		}
	}
}
