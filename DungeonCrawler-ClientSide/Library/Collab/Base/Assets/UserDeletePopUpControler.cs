using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserDeletePopUpControler : MonoBehaviour
{
    [SerializeField] Button ButtonDelete;
    [SerializeField] Button ButtonCancel;
    public string popUpName;
    public string popUpPass;

    // Start is called before the first frame update
    void Start()
    {
    }
    public void AnswerDelete()
    {
        ServerController.server.Ask($"3/{popUpName}/{popUpPass}");
        ServerController.server.DisconnectFromServer();
        Application.Quit();
    }
    public void AnswerCancel()
    {
        PopUpClose();
    }
    public void PopUpClose()
    {
        Destroy(this.gameObject);
    }
}
