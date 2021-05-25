using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIInviteToPartyButton : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI targetUsername;
	[SerializeField] ServerPetitions server;

	//En un futuro hacer que el servidor nos diga si podemos invitarle o no a la partida
	//Por ejemplo no se podría invitar a alguien que ya tiene una partida en marcha.
	//O quizá por que hemos añadido una opción que al activarse en el menu nadie te pueda invitar
	
	public void InviteToParty()
	{
		ServerController.server.Ask($"2/{targetUsername.text}");
	}
}
