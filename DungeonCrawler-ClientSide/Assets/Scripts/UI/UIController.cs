using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIController : MonoBehaviour
{
	[SerializeField] GameObject onlineUsersList, onlineUsersMenuGameObject, recentPlayersList, loginMenu, lobbyMenu, waitingSign, PopUpPrefab, PlayerHost;
	[SerializeField] List<GameObject> Players;
	[SerializeField] ServerPetitions serverPetitions;
	public GameObject chatPanel, textObject;
	public TMP_InputField chatBox;
	string[] onlineUsersContent, recentPlayersContent;
	string popUpText, Grouptext, newMSG;
	bool display, popupDisplay, updateGroup;
	public int maxMessages = 25;
	public Color playerMessage, infoMessage;
	[SerializeField] GameObject searchGameButton;
	private void Start()
	{
		display = false;
		error = false;
		popupDisplay = false;
		updateGroup = false;
		loginMenu.SetActive(true);
		lobbyMenu.SetActive(false);
		showOnlineUsersMenu = false;
		onlineUsersMenuGameObject.SetActive(false);
		int j = 0;
		while (j < onlineUsersList.transform.childCount)
		{
			onlineUsersList.transform.GetChild(j).gameObject.SetActive(false);
			j++;
		}
	}
	private void OnEnable()
	{
		
	}
	private void Update()
	{
		
		if (error)
		{
			Debug.Log("There was an error");
			//displayError
			error = false;
		}
		if (display)
		{
			DisplayOnlineUsers();
			display = false;
		}
		if (popupDisplay)
		{
			PopUpApear();
			popupDisplay = false;
		}
		if (updateGroup)
		{
			MakeGroup();
			updateGroup = false;
		}
	}
	
	bool showOnlineUsersMenu;
	public void ActiveOnlineUsersMenu()
	{
		showOnlineUsersMenu = !showOnlineUsersMenu;
		onlineUsersMenuGameObject.SetActive(showOnlineUsersMenu);
		lobbyMenu.SetActive(!showOnlineUsersMenu);
	}

	public void SetOnlineUsers(string rawList)
	{
		Debug.Log("Updated players list :" + rawList);
		onlineUsersContent = rawList.Split('/');
		display = true;
	}

	public void SetRecentPlayers(string rawList)
	{
		recentPlayersContent = rawList.Split('/');
		display = true;
	}

	public void WaitingSign(bool state)
	{
		//Debug.Log("Waiting sign state : " + state);
		waitingSign.SetActive(state);
	}
	bool logIn;
	public void LoggedIn()
	{
		Debug.Log("UI LOGGED ID");
		logIn = true;
		loginMenu.SetActive(false);
		lobbyMenu.SetActive(true);
	}

	public void PopUpWaiting(string name)
	{
		popUpText = name;
		popupDisplay = true;
	}

	public void PopUpApear()
	{
		GameObject newPopUp = Instantiate(PopUpPrefab, lobbyMenu.transform);
		newPopUp.GetComponent<PopUpControler>().popUpName = popUpText;
	}

	public void MakeGroupWaiting(string names)
	{
		if (names == "FULL")
		{
			PopUpWaiting("The lobby is full");
		}
		Debug.Log("New group :" + names);
		Grouptext = names;
		updateGroup = true;
	}

	[SerializeField] GameObject leaveGroupObject;
	public void MakeGroup()
	{
		string[] names = Grouptext.Split('/');
		leaveGroupObject.SetActive(names.Length > 1);
		if(names[0] == "EMPTY")
		{
			foreach (GameObject player in Players)
			{
				Debug.Log("Desactivando personajes");
				player.SetActive(false);
			}
		}
		else
		{
			//int removeIndex = Array.IndexOf(names, PlayerData.pData.playerName);
			//string[] UpdatedNames = new string[names.Length - 1];
			//for (int i = 0, j = 0; i < UpdatedNames.Length; i++, j++)
			//{
			//	if (i == removeIndex)
			//	{
			//		j++;
			//	}
			//	UpdatedNames[i] = names[j];
			//}
			//names = UpdatedNames;
			//Debug.Log("Longitud de nomrbes es : " + names.Length);

			//foreach (GameObject player in Players)
			//{
			//	Debug.Log("Desactivando personajes");
			//	player.SetActive(false);
			//}
			//for (int i = 0; i < names.Length - 1; i++)
			//{
			//	Players[i].SetActive(true);
			//	Players[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = names[i];
			//	Debug.Log("Activando personajes necesarios : " + names[i]);
			//}
		}
		
	}

	public void LeaveGroup()
	{
		//serverPetitions.LeaveGroupServer();
		foreach (GameObject Player in Players)
		{
			Player.SetActive(false);
		}
	}

	bool error;
	public void ShowErrorMessage(string errorMsj)
	{
		error = true;
	}

	private void DisplayOnlineUsers()
	{
		//Delegar esto al objeto que contiene la lista y que haga ciclos de n en n en vez de todos cuanto tenga 
		//ya que no todos tienen espacio en pantalla a la vez. Primero se imprimiría 10 y si le da a siguiente los siguientes 10
		int numberToDisplay;
		numberToDisplay = onlineUsersContent.Length - 1 <= onlineUsersList.transform.childCount ? onlineUsersContent.Length - 1 : onlineUsersList.transform.childCount;
		//Debug.Log("Number to display : " + numberToDisplay);
		for (int i = 0; i < numberToDisplay; i++)
		{
			onlineUsersList.transform.GetChild(i).gameObject.SetActive(true);
			onlineUsersList.transform.GetChild(i).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = onlineUsersContent[i];
		}
		for (int j = 0; j < onlineUsersList.transform.childCount - numberToDisplay; j++)
		{
			//Debug.Log("Number to delete : " + (onlineUsersList.transform.childCount - numberToDisplay));
			onlineUsersList.transform.GetChild(numberToDisplay + j).gameObject.SetActive(false);
		}
	}
	
}

