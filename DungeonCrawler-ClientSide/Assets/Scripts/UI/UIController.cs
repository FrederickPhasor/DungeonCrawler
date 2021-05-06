using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIController : MonoBehaviour
{
	[SerializeField] GameObject onlineUsersList;
	[SerializeField] GameObject recentPlayersList;
	string[] onlineUsersContent;
	string[] recentPlayersContent;
	[SerializeField]GameObject loginMenu;
	[SerializeField] GameObject lobbyMenu;
	[SerializeField] GameObject waitingSign;
	bool display;
	private void Start()
	{
		display = false;
		error = false;
		loginMenu.SetActive(true);
		lobbyMenu.SetActive(false);
		int j = 0;
		while (j < onlineUsersList.transform.childCount)
		{
			onlineUsersList.transform.GetChild(j).gameObject.SetActive(false);
			j++;
		}
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
		Debug.Log("Waiting sign state : " + state);
		waitingSign.SetActive(state);
	}
	public void LoggedIn()
	{
		Debug.Log("UI LOGGED ID");
		loginMenu.SetActive(false);
		lobbyMenu.SetActive(true);
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
		Debug.Log("Number to display : " + numberToDisplay);
		for (int i = 0; i < numberToDisplay ; i++)
		{
			onlineUsersList.transform.GetChild(i).gameObject.SetActive(true);
			onlineUsersList.transform.GetChild(i).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = onlineUsersContent[i];
		}
		for(int j = 0; j < onlineUsersList.transform.childCount - numberToDisplay; j++)
		{
			Debug.Log("Number to delete : " + (onlineUsersList.transform.childCount - numberToDisplay));
			onlineUsersList.transform.GetChild(numberToDisplay + j).gameObject.SetActive(false);
		}
	
	}
}
