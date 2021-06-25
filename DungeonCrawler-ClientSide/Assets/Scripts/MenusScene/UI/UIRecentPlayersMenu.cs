using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UIRecentPlayersMenu : MonoBehaviour
{
    public List<string> RecentPlayersList;
    int currentPage;
    bool recentListUpdated;
    int operation;
    [SerializeField] GameObject RecentUsersList;
    [SerializeField] GameObject nextPageButton, previousPageButton;
    [SerializeField] TextMeshProUGUI pastGames;
    string updateGames;
    bool updateGamesBool;

    // Start is called before the first frame update
    void Start()
    {
        currentPage = 0;
        RecentPlayersList = new List<string>();
        updateGamesBool = false;
    }

    void Update()
    {
        if (recentListUpdated)
        {
            if (currentPage == 0)
            {
                previousPageButton.SetActive(false);
            }
            else
                previousPageButton.SetActive(true);

            if (RecentPlayersList.Count > RecentUsersList.transform.childCount && Mathf.Abs((currentPage * RecentUsersList.transform.childCount) - RecentPlayersList.Count) > RecentUsersList.transform.childCount)
            {
                nextPageButton.SetActive(true);
            }
            else
                nextPageButton.SetActive(false);
            DisplayOnlineUsers();
            recentListUpdated = false;
        }
        if (updateGamesBool)
        {
            pastGames.text = updateGames;
            updateGamesBool = false;
        }
    }
    private void OnEnable()
    {
        ServerController.RecentUsersUpdatedEvent += AllRecentUsersListUpdate;
        ServerController.RecentSinglePlayerGamesUpdateEvent += UpdateGamesStatus;
    }
    private void OnDisable()
    {
        ServerController.RecentUsersUpdatedEvent -= AllRecentUsersListUpdate;
        ServerController.RecentSinglePlayerGamesUpdateEvent -= UpdateGamesStatus;
    }

    void UpdateGamesStatus(string games)
    {
        updateGames = games;
        updateGamesBool = true;
    }

    void AllRecentUsersListUpdate(string playerNames)
    {
        recentListUpdated = true;
        try
        {
            Debug.Log("What is left is " + playerNames);
            foreach (string username in playerNames.Split('/'))
            {
                Debug.Log("Adding now : " + username);
                RecentPlayersList.Add(username);
            }
            recentListUpdated = true;
        }
        catch
        {
            Debug.Log("Recent users list failed to update");
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
        if (RecentPlayersList.Count >= RecentUsersList.transform.childCount)
        {
            numberToDisplay = RecentUsersList.transform.childCount;
        }
        else
        {
            numberToDisplay = RecentPlayersList.Count;
        }
        Debug.Log("Number to display is : " + numberToDisplay);
        for (int i = 0; i < numberToDisplay; i++)
        {
            RecentUsersList.transform.GetChild(i).gameObject.SetActive(true);
            RecentUsersList.transform.GetChild(i).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = RecentPlayersList[i + currentPage * RecentUsersList.transform.childCount];
        }

    }
}
