using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;



public class UISettingsMenu : MonoBehaviour
{
    [SerializeField] TMP_InputField password;
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] GameObject SettingsMenuGameObject;
    [Header("Pop Ups")]
    [SerializeField] GameObject popUpDeletePrefab;


    string name; 
    bool displayDelete;


    // Start is called before the first frame update
    void Start()
    {
        displayDelete = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (displayDelete)
        {
            DeletePopUpApear();
            displayDelete = false;
        }
    }

    public void DeletePopUpApear()
    {
        GameObject newPopUp = Instantiate(popUpDeletePrefab, SettingsMenuGameObject.transform);
        newPopUp.GetComponent<UserDeletePopUpControler>().popUpName = name;
        newPopUp.GetComponent<UserDeletePopUpControler>().popUpPass = password.text;
    }

    public void DeleteUserAttemp()
    {
        if (password.text != "")
            displayDelete = true;
            //ServerController.server.Ask($"3/{name}/{password.text}");
    }

    public void SetName()
    {
        name = PlayerData.pData.GetName();
        playerName.text = name;
    }

}
