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
    [SerializeField] TextMeshProUGUI onlineUsersText;
    void Start()
    {
        server = GetComponent<ServerConnection>();
    }
    public void SignUp()
	{
        server.SendPetition($"1/{username.text}/{email.text}/{password.text}");
        Debug.Log(server.WaitForAnswer());
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
        server.SendPetition($"6/");
        
        ShowInDebugScreen(server.WaitForAnswer());


    }
    public void GetRecentPlayers()
	{
        server.SendPetition($"5/{username.text}/");
        ShowInDebugScreen(server.WaitForAnswer());
        
    }
    public void Disconnect()
	{
        server.SendPetition("0/");
        server.DisconnectFromServer();
	}
    void ShowInDebugScreen(string content){
        Debug.Log(content);
        onlineUsersText.text = "";
        string[] splitContent = content.Split('/');
        foreach (string word in splitContent)
        {
            if (word != null)
            {
                onlineUsersText.text += word + '\n';
            }
        }
    }
}
