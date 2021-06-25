using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UIConnectedPlayersMenu : MonoBehaviour
{
	public List<string> connectedPlayersList;
	int currentPage;
	bool connectedListUpdated;
	int operation;
	[SerializeField] GameObject onlineUsersList;
	[SerializeField] GameObject nextPageButton, previousPageButton;
	private void Start()
	{
		currentPage = 0;
		connectedPlayersList = new List<string>();
	}
	private void OnEnable()
	{
		ServerController.OnlineUsersUpdatedEvent += AllOnlineUsersListUpdate;
		ServerController.SinglePlayerConnectionStateUpdateEvent += SinglePlayerUpdate; 
	}
	private void OnDisable()
	{
		ServerController.OnlineUsersUpdatedEvent -= AllOnlineUsersListUpdate;
		ServerController.SinglePlayerConnectionStateUpdateEvent -= SinglePlayerUpdate;
	}
	void SinglePlayerUpdate(string opAndName)
	{
		string[] parts = opAndName.Split('/');
		Debug.Log("SinglePlayerUpdate ::  " + opAndName);
		int order = Convert.ToInt32(parts[0]);
		if (order == 1)
		{
			connectedPlayersList.Add(parts[1]);
		}
		else
		{
			connectedPlayersList.Remove(parts[1]);
			//delete
		}
		connectedListUpdated = true;
	}
	void AllOnlineUsersListUpdate( string playerNames)
	{
		//  
		connectedListUpdated = true;
		string[] cleanString = playerNames.Split(new[] { '\0' }, 2);
		//numConnected/user1/user3/../
		string[] parts = cleanString[0].Split(new[] { '/' }, 2);
		//parts[1] son los usuarios, podría estar vacío
		int connectedNum = 0;
		try
		{
			connectedNum = Int32.Parse(parts[0]);

		}
		catch
		{

		}
		try
		{
			foreach (string username in parts[1].Split('/'))
			{
				if(username != "")
				{
					Debug.Log("Adding now : " + username);
					connectedPlayersList.Add(username);
				}
			}
			connectedListUpdated = true;
		}
		catch
		{
			Debug.Log("Online users list failed to update");
		}
		
	}
	private void Update()
	{
		if (connectedListUpdated)
		{
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
			connectedListUpdated = false;
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
	}
}
