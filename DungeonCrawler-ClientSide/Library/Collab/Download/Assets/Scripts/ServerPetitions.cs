using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ServerPetitions : MonoBehaviour
{
    ServerController server;
    [SerializeField] TMP_InputField username;
    [SerializeField] TMP_InputField email;
    [SerializeField] TMP_InputField password;
    void Start()
    {
        server = GetComponent<ServerController>();
    }
    public void SignUp()
    {
        server.Ask($"1/{username.text}/{email.text}/{password.text}");
    }
    public void SignIn()
    {
        PlayerData.pData.playerName = username.text;
        server.Ask($"2/{username.text}/{password.text}/");
    }
    public void ChangePassword()
    {
        server.Ask($"3/{email.text}/{password.text}/");
    }
    public void GetRecentPlayers()
    {
        server.Ask($"5/{username.text}/");
    }
    public void Disconnect()
    {
        server.Ask("0/");
        LeaveGroupServer();
        server.DisconnectFromServer();
    }
    public void Invite(string name)
	{
        server.Ask($"8/{name}/");
	}  
    public void AnwserInviteYes(string who)
    {
        server.Ask($"7/Y/{who}/");
    }
    public void AnwserInviteNo(string who)
    {
        server.Ask($"7/N/{who}/");
    }
    public void LeaveGroupServer()
    {
        server.Ask("9/");
    }
    public void MessageToServer(string msg)
    {
        server.Ask($"10/{msg}/");
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }
}
