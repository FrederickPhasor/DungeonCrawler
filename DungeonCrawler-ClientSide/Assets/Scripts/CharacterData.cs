using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData : MonoBehaviour, IInteractable
{
    [SerializeField] string partnerName;
    [SerializeField] int health;
    [SerializeField] int armour;
    [SerializeField] int attackBonus;
    [SerializeField] int initiative;
    [SerializeField] string classe;
    private bool isAlive = false;
	public bool IsAlive { get => isAlive; set => isAlive = value; }
    public delegate void Death();
    public static event Death LocalDeathEvent;
    public void OnClicked()
	{

	}
    public string GetName()
    {
        return partnerName;
    }
    public void SetName(string name)
    {
         partnerName = name;
    }
    public void GetDamaged(int amount)
	{
        health -= amount;
	}
	private void Update()
	{
		if(health <= 0)
		{
            LocalDeathEvent();//LocalDeath
            ServerController.server.Ask("12/" + partnerName);
        }
	}
}
