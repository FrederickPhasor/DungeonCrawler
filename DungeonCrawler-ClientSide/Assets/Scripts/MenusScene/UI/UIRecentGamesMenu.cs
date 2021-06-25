using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;

public class UIRecentGamesMenu : MonoBehaviour
{
    public List<string> GamesList;
    [SerializeField] TMP_InputField StartDateField;
    [SerializeField] TMP_InputField EndDateField;
    [SerializeField] GameObject GamesListGO;
    [SerializeField] GameObject nextPageButton, previousPageButton;
    string date;
    string updateGamesString;
    bool updateGamesBool;
    bool recentListUpdated;
    int currentPage;



    /*const int yearMaxValue = 9999;
    const int monthMaxValue = 12;
    const int dayMaxValue = 31;
    const int yearMinValue = 2000;
    const int monthMinValue = 1;
    const int dayMinValue = 1;*/


    // Start is called before the first frame update
    void Start()
    {
        currentPage = 0;
        GamesList = new List<string>();
        updateGamesBool = false;
    }

    // Update is called once per frame
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

            if (GamesList.Count > GamesListGO.transform.childCount && Mathf.Abs((currentPage * GamesListGO.transform.childCount) - GamesList.Count) > GamesListGO.transform.childCount)
            {
                nextPageButton.SetActive(true);
            }
            else
                nextPageButton.SetActive(false);
            DisplayGames();
            recentListUpdated = false;
        }
        if (updateGamesBool)
        {
            //pastGames.text = updateGames;
        }

    }

    private void OnEnable()
    {
        ServerController.PastGamesEvent += UpdateGames;
    }
    private void OnDisable()
    {
        ServerController.PastGamesEvent -= UpdateGames;
    }

    private void DisplayGames()
    {
        int numberToDisplay;
        if (GamesList.Count >= GamesListGO.transform.childCount)
        {
            numberToDisplay = GamesListGO.transform.childCount;
        }
        else
        {
            numberToDisplay = GamesList.Count;
        }
        Debug.Log("Number to display is : " + numberToDisplay);
        for (int i = 0; i < numberToDisplay; i++)
        {
            GamesListGO.transform.GetChild(i).gameObject.SetActive(true);
            GamesListGO.transform.GetChild(i).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GamesList[i + currentPage * GamesListGO.transform.childCount];
        }
    }

    

    public void SearchGames()
    {
        ServerController.server.Ask("14/" + StartDateField.text + "/" + EndDateField.text);    
    }

    void UpdateGames(string games)
    {
        recentListUpdated = true;
        try
        {
            Debug.Log("What is left is " + games);
            string[] parts = games.Split('/');
            int ammount = parts.Count() / 5;

            for (int i = 0; i < ammount; i++)
            {
                for (int j = 0; j < 5; j++)
                    if (j == 4)//date
                        Console.WriteLine(parts[((i * 5) + j)]);
                Console.WriteLine("---------------");
            }
            recentListUpdated = true;
        }
        catch
        {
            Debug.Log("Recent users list failed to update");
        }
    }

    void PastGamesUpdate()
    {
        //"19/1/1/1/Ganador/2021-06-11/2/1/2/Perdedor/2021-06-11/3/1/3/Perdedor/2021-06-11/4/1/4/Perdedor/2021-06-11/5/2/1/Perdedor/2021-06-16/6/2/2/Perdedor/2021-06-16/7/2/3/Ganador/2021-06-16/8/2/4/Perdedor/2021-06-16/";

		string[] parts1 = updateGamesString.Split('/');
		int ammount = parts1.Count() / 5;

		for (int i = 0; i < ammount; i++)
		{
			for (int j = 0; j < 5; j++)
                if (j == 4)//date
                    Console.WriteLine(parts1[((i * 5) + j)]);
			Console.WriteLine("---------------");
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

    public int CheckCorrectDate(string date)//YYYY-MM-DD
    {
        int year;
        int month;
        int day;
        if (date.Length != 10)
        {
            string[] parts = name.Split('-');
            if (parts.Count() != 3)
                return -1;
            else if (parts[0].Length != 4 || parts[1].Length != 2 || parts[2].Length != 2)
                return -1;
            else if (Int32.TryParse(parts[0], out year) == true && Int32.TryParse(parts[1], out month) == true && Int32.TryParse(parts[2], out day) == true)
            {
                return 0;
            }
            else
                return 0;

        }
        else
            return -1;
    }
}

