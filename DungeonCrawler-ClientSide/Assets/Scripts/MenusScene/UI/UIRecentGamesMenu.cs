using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;

public class UIRecentGamesMenu : MonoBehaviour
{
    [SerializeField] TMP_InputField StartDateField;
    [SerializeField] TMP_InputField EndDateField;
    string date;
    const int yearMaxValue = 9999;
    const int monthMaxValue = 12;
    const int dayMaxValue = 31;
    const int yearMinValue = 2000;
    const int monthMinValue = 1;
    const int dayMinValue = 1;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        ServerController.PastGamesEvent += PastGamesUpdate;        
    }
    private void OnDisable()
    {
        ServerController.PastGamesEvent -= PastGamesUpdate;
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
            else if(Int32.TryParse(parts[0], out year) == true && Int32.TryParse(parts[1], out month) == true && Int32.TryParse(parts[2], out day) == true)
            {
                return 0;
            }
            else
                return 0;

        }
        else
            return -1;
    }

    public void SearchGames()
    {
        ServerController.server.Ask("14/" + StartDateField.text + "/" + EndDateField.text);    
    }

    void PastGamesUpdate(string games)
    {

    }
}
