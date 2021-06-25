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
    
}
