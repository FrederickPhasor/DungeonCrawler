using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIConnectedPlayersMenu : MonoBehaviour
{
	List<string> connectedPlayersList;
	int currentPage;
	bool updated;
	int operation;
	string username;
	[SerializeField] GameObject onlineUsersList;
	[SerializeField] GameObject nextPageButton, previousPageButton;
	private void Start()
	{
		currentPage = 0;
		connectedPlayersList = new List<string>();
	}
	private void OnEnable()
	{
		ServerController.OnlineUsersUpdatedEvent += TriggerOnlineUsersUpdate;
	}
	private void OnDisable()
	{
		ServerController.OnlineUsersUpdatedEvent -= TriggerOnlineUsersUpdate;
	}
	void TriggerOnlineUsersUpdate(int op, string playerName)
	{
		username = playerName;

		operation = op;

		updated = true;
	}
	private void Update()
	{
		if (updated)
		{
			if(operation == 1)
			{
				connectedPlayersList.Add(username);
			}
			else
			{
				connectedPlayersList.Remove(username);
			}
			if (currentPage == 0)
			{
				previousPageButton.SetActive(false);
			}
			else
				previousPageButton.SetActive(true);
			if (connectedPlayersList.Count > onlineUsersList.transform.childCount && Mathf.Abs((currentPage * onlineUsersList.transform.childCount) - connectedPlayersList.Count) > onlineUsersList.transform.childCount)
			{
				nextPageButton.SetActive(true);
			}
			else
				nextPageButton.SetActive(false);
			DisplayOnlineUsers();
			updated = false;
		}
	}
	public void GoNextPage()
	{
		currentPage++;
	}
	public void GoPreviousPage()
	{
		currentPage--;
	}
	private void DisplayOnlineUsers()
	{
		int numberToDisplay;
		if(connectedPlayersList.Count  >= onlineUsersList.transform.childCount)
		{
			numberToDisplay = onlineUsersList.transform.childCount;
		}
		else
		{
			numberToDisplay = connectedPlayersList.Count;
		}
		Debug.Log("Number to display is : " + numberToDisplay);
		for (int i = 0; i < numberToDisplay; i++)
		{
			onlineUsersList.transform.GetChild(i).gameObject.SetActive(true);
			onlineUsersList.transform.GetChild(i).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = connectedPlayersList[i + currentPage * onlineUsersList.transform.childCount];
		}
		//for (int j = 0; j < onlineUsersList.transform.childCount - numberToDisplay; j++)
		//{
		//	onlineUsersList.transform.GetChild(numberToDisplay + j).gameObject.SetActive(false);
		//}
	}
}
