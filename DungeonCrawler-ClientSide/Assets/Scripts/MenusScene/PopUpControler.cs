using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PopUpControler : MonoBehaviour
{
  
    [SerializeField] Button ButtonYes;
    [SerializeField] Button ButtonNO;
    [SerializeField] TextMeshProUGUI displayText;
    public string popUpName;

    // Start is called before the first frame update
    void Start()
    {
        displayText.text = "Te ha invitado a jugar : " + popUpName;
    }
    public void AnswerYes()
	{
        ServerController.server.Ask("5/Y/" + popUpName);
        PopUpClose();

    }
    public void AnswerNo()
	{
        ServerController.server.Ask($"5/N/{popUpName}");
        PopUpClose();
    }
    public void PopUpClose()
    {
        Destroy(this.gameObject);
    }
}
