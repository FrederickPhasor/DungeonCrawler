using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RegisterPopUpControler : MonoBehaviour
{

    [SerializeField] Button ButtonOK;
    [SerializeField] TextMeshProUGUI displayText;

    // Start is called before the first frame update
    void Start()
    {
        displayText.text = "You have registered successfully!";
    }
    public void PopUpClose()
    {
        Destroy(this.gameObject);
    }
}