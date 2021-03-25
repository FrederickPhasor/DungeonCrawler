using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
public class ServerConnection : MonoBehaviour
{
	Socket socket;
	IPAddress address = IPAddress.Parse("192.168.56.102");
	[SerializeField] TMP_InputField nickname;
	[SerializeField] TMP_InputField email;
	[SerializeField] TextMeshProUGUI recentGamesDisplay;
	[SerializeField] TMP_InputField password;
	string message;
	void ConnectToServer()
	{
		IPEndPoint iPEndPoint = new IPEndPoint(address, 8564);
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try
		{
			socket.Connect(iPEndPoint);
			socket.ReceiveTimeout = 5000;
			Debug.Log("Se ha conectado correctamente al servidor");
		}
		catch(SocketException ex)
		{
			Debug.Log("Error al intentar conectar con el servidor" + ex.Message);
		}
	}
	string ListenForAnswer()
	{
		byte[] rawAnswer = new byte[70];
		socket.Receive(rawAnswer);
		string message = System.Text.Encoding.ASCII.GetString(rawAnswer);
		return message;
	}
	public void SignUp()
	{
		ConnectToServer();
		//1/username/password
		Debug.Log("We did a signUp");
		message = $"1/{nickname.text}/{email.text}/{password.text}";
		socket.Send(System.Text.Encoding.ASCII.GetBytes(message));
		//ListenForAnswer();
	}
	public void Login()
	{
		ConnectToServer();
		//1/username/password
		 message = $"2/{nickname.text}/{password.text}/";
		socket.Send(System.Text.Encoding.ASCII.GetBytes(message));
		string answer = ListenForAnswer();
		
	}
	public void ChangePassword()
	{
		ConnectToServer();
		Debug.Log("We did a password change");
		message = $"3/{email.text}/{password.text}/";
		socket.Send(System.Text.Encoding.ASCII.GetBytes(message));
		
	}
	[SerializeField] TMP_InputField oldEmail;
	[SerializeField] TMP_InputField newEmail;
	[SerializeField] TMP_InputField currentPassword;
	public void ChangeMail()
	{
		Debug.Log("We did a email change");
		ConnectToServer();
		message = $"4/{oldEmail.text}/{newEmail.text}/{currentPassword.text}/";
		socket.Send(System.Text.Encoding.ASCII.GetBytes(message));
	}
	public List<string> namesList = new List<string>();
	public string[] names;
	[SerializeField] TMP_InputField nameRequested;
	public void CheckRecentGames()
	{
		ConnectToServer();
		string name = nameRequested.text;
		message = "5/" + name;
		Debug.Log(message);
		socket.Send(System.Text.Encoding.ASCII.GetBytes(message));
		string answer = ListenForAnswer().Split('\0')[0];
	
		names = answer.Split('/');
		for(int i = 0; i < names.Length; i++)
		{
			namesList.Add(names[i]);
		}
		DisplayUsers();
	}
	[SerializeField]List<TextMeshProUGUI> namesPlaceHolders = new List<TextMeshProUGUI>();
	void DisplayUsers()
	{
		for (int i = 0; i < namesPlaceHolders.Count && i < namesList.Count; i++){
			namesPlaceHolders[i].text = namesList[i];
		}
	}
}
