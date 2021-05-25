using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIInviteToPartyButton : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI targetUsername;
	[SerializeField] ServerPetitions server;

	//En un futuro hacer que el servidor nos diga si podemos invitarle o no a la partida
	//Por ejemplo no se podr�a invitar a alguien que ya tiene una partida en marcha.
	//O quiz� por que hemos a�adido una opci�n que al activarse en el menu nadie te pueda invitar
	
	public void InviteToParty()
	{
		ServerController.server.Ask($"2/{targetUsername.text}");
	}
}
