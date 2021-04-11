using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ServerPetitions : MonoBehaviour
{
    ServerConnection server;
    [SerializeField] TMP_InputField username;
    [SerializeField] TMP_InputField email;
    [SerializeField] TMP_InputField password;
    void Start()
    {
        server = GetComponent<ServerConnection>();
    }
    public void SignUp()
	{
        server.SendPetition($"1/{username.text}/{email.text}/{password.text}");
	}
    public void SignIn()
	{
        server.SendPetition($"2/{username.text}/{password.text}/");
    }
    public void ChangePassword()
	{
        server.SendPetition($"3/{email.text}/{password.text}/");

    }
    public void ChangeEmail()
	{
        //server.SendPetition($"4/{oldEmail.text}/{newEmail.text}/{currentPassword.text}/");

    }
    public void GetAllOnlineUsers()
	{

	}
    public void GetRecentPlayers()
	{
        server.SendPetition($"5/{username.text}/");
        Debug.Log(server.WaitForAnswer());

    }
   
}
