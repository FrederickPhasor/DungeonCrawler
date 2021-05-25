using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIConnectedPlayersMenu : MonoBehaviour
{
	string[] onlineUsersContent;
	int currentPage;
	bool updated;
	[SerializeField] GameObject onlineUsersList;
	[SerializeField] GameObject nextPageButton, previousPageButton;
	private void Start()
	{
		currentPage = 0;
	}
	private void OnEnable()
	{
		ServerController.OnlineUsersUpdatedEvent += TriggerOnlineUsersUpdate;
	}
	private void OnDisable()
	{
		ServerController.OnlineUsersUpdatedEvent -= TriggerOnlineUsersUpdate;
	}
	void TriggerOnlineUsersUpdate(string connectedPlayersString)
	{
		onlineUsersContent = connectedPlayersString.Split('/');
		updated = true;
	}
	private void Update()
	{
		if (updated)
		{
			if (currentPage == 0)
			{
				previousPageButton.SetActive(false);
			}
			else
				previousPageButton.SetActive(true);
			if (onlineUsersContent.Length > onlineUsersList.transform.childCount && Mathf.Abs((currentPage * onlineUsersList.transform.childCount) - onlineUsersContent.Length) > onlineUsersList.transform.childCount)
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
		numberToDisplay = onlineUsersContent.Length - 1 <= onlineUsersList.transform.childCount ? onlineUsersContent.Length - 1 : onlineUsersList.transform.childCount;
		if(onlineUsersContent.Length - 1 <= onlineUsersList.transform.childCount)
		{
			numberToDisplay = onlineUsersContent.Length - 1;
		}
		else
		{
			numberToDisplay = onlineUsersList.transform.childCount;
		}
		for (int i = 0; i < numberToDisplay; i++)
		{
			onlineUsersList.transform.GetChild(i).gameObject.SetActive(true);
			onlineUsersList.transform.GetChild(i).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = onlineUsersContent[i + currentPage * onlineUsersList.transform.childCount];
		}
		for (int j = 0; j < onlineUsersList.transform.childCount - numberToDisplay; j++)
		{
			onlineUsersList.transform.GetChild(numberToDisplay + j).gameObject.SetActive(false);
		}
	}
}
