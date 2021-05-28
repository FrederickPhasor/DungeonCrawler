using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UILogInMenu : MonoBehaviour
{
    [SerializeField] TMP_InputField username;
    [SerializeField] TMP_InputField password;
    [SerializeField] TMP_InputField IPAddress;
    [SerializeField] TMP_InputField PORT;
	[SerializeField] Image connectionIndicator;
	[SerializeField] GameObject LobbyMenuGameObject;
	[SerializeField] GameObject LogInMenuGameObject;
	bool signIn;
	
	private void Start()
	{
		LogInMenuGameObject.SetActive(true);
		signIn = false;
		IPAddress.text = "192.168.56.103";
		PORT.text = "9093";
	}
	private void Update()
	{
		if (signIn)
		{
			PlayerData.pData.SetName(username.text);
			PlayerData.pData.isLeader = true;
			LobbyMenuGameObject.SetActive(true);
			LogInMenuGameObject.SetActive(false);
			signIn = false;
		}
	}
	private void OnEnable()
	{
		ServerController.SignedInEvent += SignInSuccess;
	}
	private void OnDisable()
	{
		ServerController.SignedInEvent -= SignInSuccess;
	}
	public void SignInAttemp()
	{
        ServerController.server.Ask($"1/{username.text}/{password.text}");
	}
    public void AttemptConnectionToServer()
	{
		connectionIndicator.color = Color.white;
        int result = ServerController.server.ConnectToServer(IPAddress.text, PORT.text);
		if (result == 0)
		{
			connectionIndicator.color = Color.green;
		}
		else
		{
			connectionIndicator.color = Color.red;
		}
	}
	public void SignInSuccess()
	{
		signIn = true;
	}
}
