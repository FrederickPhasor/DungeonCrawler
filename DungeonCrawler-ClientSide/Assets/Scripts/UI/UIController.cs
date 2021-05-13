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
	[SerializeField] List<Message> messageList = new List<Message>();
	ServerPetitions serverPetitions;
	public GameObject chatPanel, textObject;
	public TMP_InputField chatBox;
	string[] onlineUsersContent, recentPlayersContent;
	string popUpText, Grouptext;
	bool display, popupDisplay, updateGroup;
	public int maxMessages = 25;
	public Color playerMessage, infoMessage;


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
	private void Update()
	{
		if (logIn)
		{
			PlayerHost.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerData.pData.playerName;
			logIn = false;
		}
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
		if (chatBox.text != "")
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				SendMessageToChat(PlayerData.pData.playerName + ": " + chatBox.text, Message.MessageType.playerMessage);
				chatBox.text = "";
			}
			else
			{
				if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
					chatBox.ActivateInputField();
			}

		}
		if (!chatBox.isFocused)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				SendMessageToChat("You Pressed Space", Message.MessageType.info);
				Debug.Log("Space");
			}
		}


	}

	public void SendMessageToChat(string text, Message.MessageType messageType)
	{
		if (messageList.Count >= maxMessages)
		{
			Destroy(messageList[0].textObject.gameObject);
			messageList.Remove(messageList[0]);
		}
		Message newMessage = new Message();
		newMessage.text = text;
		GameObject newText = Instantiate(textObject, chatPanel.transform);
		newMessage.textObject = newText.GetComponent<TextMeshProUGUI>();
		newMessage.textObject.text = newMessage.text;
		messageList.Add(newMessage);
		newMessage.textObject.color = MessageTypeColor(messageType);
	}

	Color MessageTypeColor(Message.MessageType messageType)
	{
		Color color = infoMessage;
		switch (messageType)
		{
			case Message.MessageType.playerMessage:
				color = playerMessage;
				break;
		}
		return color;
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
		Debug.Log("New group :" + names);
		Grouptext = names;
		updateGroup = true;
    }

	public void MakeGroup()
	{
		string [] names = Grouptext.Split('/');
		int removeIndex = Array.IndexOf(names, PlayerData.pData.playerName);
		// declare and define a new array one element shorter than the old array
		string[] UpdatedNames = new string[names.Length - 1];
		// loop from 0 to the length of the new array, with i being the position
		// in the new array, and j being the position in the old array
		for (int i = 0, j = 0; i < UpdatedNames.Length; i++, j++)
		{
			// if the index equals the one we want to remove, bump
			// j up by one to "skip" the value in the original array
			if (i == removeIndex)
            {
				j++;
            }
			// assign the good element from the original array to the
			// new array at the appropriate position
			UpdatedNames[i] = names[j];			
		}
		// overwrite the old array with the new one
		names = UpdatedNames;

		for (int i = 0 ; i < names.Length-1 ; i++)
        {
			Players[i].SetActive(true);
			Players[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = names[i];
		}		
	}

	public void LeaveGroup()
    {
		serverPetitions.LeaveGroupServer();
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
		numberToDisplay = onlineUsersContent.Length - 1  <= onlineUsersList.transform.childCount ? onlineUsersContent.Length- 1  : onlineUsersList.transform.childCount;
		//Debug.Log("Number to display : " + numberToDisplay);
		for (int i = 0; i < numberToDisplay ; i++)
		{
			onlineUsersList.transform.GetChild(i).gameObject.SetActive(true);
			onlineUsersList.transform.GetChild(i).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = onlineUsersContent[i];
		}
		for(int j = 0; j < onlineUsersList.transform.childCount - numberToDisplay; j++)
		{
			//Debug.Log("Number to delete : " + (onlineUsersList.transform.childCount - numberToDisplay));
			onlineUsersList.transform.GetChild(numberToDisplay + j).gameObject.SetActive(false);
		}
	
	}
}

[System.Serializable]
public class Message
{
	public string text;
	public TextMeshProUGUI textObject;
	public MessageType messageType;
	public enum MessageType
    {
		playerMessage,
		info
    }
}
