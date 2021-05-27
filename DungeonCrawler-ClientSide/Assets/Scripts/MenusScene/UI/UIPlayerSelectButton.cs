using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIPlayerSelectButton : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI playerUsername;
	
	public void UpdatePlayerInfo()
	{
		//Crear una notificación del servidor donde se nos de esta info cada vez que cambia
		playerUsername.text = transform.GetChild(0).transform.GetComponent<TextMeshProUGUI>().text; //Pone el nombre
	}
}
