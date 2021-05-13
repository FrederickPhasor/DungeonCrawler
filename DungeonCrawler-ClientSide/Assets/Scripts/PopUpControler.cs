using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PopUpControler : MonoBehaviour
{
    ServerPetitions serverPetitions;
    [SerializeField] Button ButtonYes;
    [SerializeField] Button ButtonNO;
    [SerializeField] TextMeshProUGUI displayText;
    public string popUpName;

    // Start is called before the first frame update
    void Start()
    {
        serverPetitions = FindObjectOfType<ServerPetitions>();
        displayText.text = "You have Recived an invite from " + popUpName;
    }
    public void PopUpClose()
    {
        Destroy(this.gameObject);
    }
    public void AnswerYes()
	{
        serverPetitions.AnwserInviteYes(popUpName);
	}
    public void AnswerNo()
	{
        serverPetitions.AnwserInviteNo(popUpName);
	}
}
