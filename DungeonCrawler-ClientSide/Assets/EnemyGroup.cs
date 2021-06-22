using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
	public string groupIndex;
	private bool alive;
	public bool Alive { get => alive; set => alive = value;}
	
	public List<CharacterData> players = new List<CharacterData>();
	void Start()
	{

		Debug.Log("Start happened");
		for(int i = 0; i < transform.childCount; i++)
		{
			players.Add(transform.GetChild(i).gameObject.GetComponent<CharacterData>());
		}
		this.gameObject.SetActive(false);
	}
	private void Update()
	{
		if (alive)
		{
			GetComponent<BoxCollider2D>().enabled = true;
		}
		else
		{
			//Spawn a corpse image
			gameObject.SetActive(false);

		}
	}
	public void AddPlayer(string name)
	{
		if(name == "-1" || name == "")
		{
			return;
		}
		foreach(CharacterData player in players)
		{
			if(player.IsAlive == false)
			{
				player.IsAlive = true;
				player.SetName(name);
				player.gameObject.SetActive(true);
				break;
			}
		}
    }
	public void AliveStatus(bool alive)
    {
		this.alive = alive;
    }

}
