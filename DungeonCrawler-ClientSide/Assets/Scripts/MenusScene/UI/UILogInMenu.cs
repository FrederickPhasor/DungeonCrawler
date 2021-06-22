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
	public delegate void ConnectionToServerStablished();
	public static event ConnectionToServerStablished ConnectionToServerStablishedEvent;
	bool signIn;
	public static bool nameUpdated;
	private void Update()
	{
		if (signIn)
		{
			Debug.Log("Loggin success");
			PlayerData.pData.SetName(username.text);
			PlayerData.pData.isLeader = true;
			LobbyMenuGameObject.SetActive(true);
			LogInMenuGameObject.SetActive(false);
			connectionIndicator.color = Color.red;
			signIn = false;
			nameUpdated = true;
			this.enabled = false;
		}
	}
	
	private void OnEnable()
	{
		Debug.Log("OnEnable went off");
		ServerController.SignedInEvent += SignInSuccess;
		connectionIndicator.color = Color.red;
		LogInMenuGameObject.SetActive(true);
		signIn = false;
		IPAddress.text = "192.168.56.104";
		PORT.text = "7000";
	}
	private void OnDisable()
	{
		ServerController.SignedInEvent -= SignInSuccess;
	}
	public void SignInAttemp()
	{
		if (password.text != "" && username.text != "")
			ServerController.server.Ask($"2/{username.text}/{password.text}");
	}
    public void AttemptConnectionToServer()
	{
        int result = ServerController.server.ConnectToServer(IPAddress.text, PORT.text);
		if (result == 0)
		{
			connectionIndicator.color = Color.green;
			ConnectionToServerStablishedEvent();
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
