using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
public class ChatController : MonoBehaviour
{
	[Header("Chat Colors")]
	[SerializeField] Color globalChatColor;
	[SerializeField] Color serverMessageColor;
	[SerializeField] Color whisperColor;
	[SerializeField] Color inGameGlobalColor;
	[SerializeField] Color groupMessageColor;
	[Header("Chat GameObjects References")]
	[Tooltip("Where the user types the message, must be a TMP_InputField")]
	public TMP_InputField chatInputBox;
	[Tooltip("A prefab containing a TMP text box")]
	[SerializeField] GameObject messagePrefab;
	[Tooltip("A panel with a grid gameobject that holds the messages prefabs as they appear")]
	public GameObject ParentGameObject;
	[Range(15,35)]
	[SerializeField] float chatScale;

	[SerializeField] List<Message> messageList = new List<Message>();
	public int maxMessages = 25;
	string newMSG;
	bool chatUpdate;
	[TextArea]
	public string helpMessage;
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			string[] parts = chatInputBox.text.Split(new[] { ' ' }, 2);
			string serverMsg;
			if (parts[0] == "/help" || parts[0] == "/h") //local
			{
				SendMessageToChat(helpMessage, Message.MessageType.localInfoMessage);
				return;
			}
			else if (parts[0] == "/All" || parts[0] == "/all" && PlayerData.pData.inGame == true)//All in the game
			{
				serverMsg = $"5/1/{parts[1]}";
				//SendMessageToChat($"{PlayerData.pData.GetName()}: " + parts[1], Message.MessageType.inGameGlobalMessage);
			}
			else if (parts[0] == "/All" || parts[0] == "/all" && PlayerData.pData.inGame == false)//All connected users
			{
				serverMsg = $"5/4/{parts[1]}";
				//SendMessageToChat("Tú: " + parts[1], Message.MessageType.globalMessage);
			}
			else if (parts[0] == "/W" || parts[0] == "/w")//whisper  name msg
			{
				string name = parts[1].Split(new[] { ' ' }, 2)[0]; //crear un caso del servidor en el que si no encuentra al usuario puesto devuelva un error y se muestre en chat.
				serverMsg = $"5/2/{name}/{parts[1].Split(new[] { ' ' }, 2)[1]}";
				//SendMessageToChat($"Whisper to {name}: " + parts[1].Split(new[] { ' ' }, 2)[1], Message.MessageType.whisperMessage);
			}
			else//group message
			{
				serverMsg = $"5/3/{chatInputBox.text}";
				//SendMessageToChat("Tú: " + chatInputBox.text, Message.MessageType.groupMessage);
			}
			ServerController.server.Ask(serverMsg);
			chatInputBox.text = "";
		}
		if (chatUpdate)
		{
			ReceiveMessage();
			chatUpdate = false;
		}
	}
	private void OnEnable()
	{
		ServerController.NewMessageReceivedEvent += MessageWaiting;
	}
	private void OnDisable()
	{
		ServerController.NewMessageReceivedEvent -= MessageWaiting;
	}
	public void MessageWaiting(string text)
	{
		newMSG = text;
		chatUpdate = true;		
	}
public void SendMessageToChat(string text, Message.MessageType messageType){
	if (messageList.Count >= maxMessages)
	{
		Destroy(messageList[0].textObject.gameObject);
		messageList.Remove(messageList[0]);
	}
	Message newMessage = new Message();
	newMessage.text = text;
	GameObject newText = Instantiate(messagePrefab, ParentGameObject.transform);
	newMessage.textObject = newText.GetComponent<TextMeshProUGUI>();
	newMessage.textObject.text = newMessage.text;
		newMessage.textObject.fontSize = chatScale;
	messageList.Add(newMessage);
	newMessage.textObject.color = MessageTypeColor(messageType);
}
private void ReceiveMessage()
{
	string[] parts = newMSG.Split(new[] { '/' }, 2);
	Debug.Log(parts[1]);
	int num = Convert.ToInt32(parts[0]);
	switch (num)
	{
		case 1://General chat, only people that is not playing
			
				string[] parts2 = parts[1].Split(new[] { '/' }, 2);
				SendMessageToChat(parts2[0] + ": "+parts2[1], Message.MessageType.inGameGlobalMessage);
				break;
		case 2:
			break;
		case 3: //groupchatting
				//string[] parts2 = parts[1].Split(new[] { '/' }, 2);
				//SendMessageToChat(parts2[0] + ": " + parts2[1],Message.MessageType.groupMessage);
			break;
	}
}

Color MessageTypeColor(Message.MessageType messageType)
{
	Color color = serverMessageColor;
	switch (messageType)
	{
		case Message.MessageType.inGameGlobalMessage:
			color = inGameGlobalColor;
				break;
			case Message.MessageType.localInfoMessage:
				color = serverMessageColor;
				break;
			case Message.MessageType.whisperMessage:
				color = whisperColor;
				break;
			case Message.MessageType.globalMessage:
				color = globalChatColor;
					break;
			case Message.MessageType.groupMessage:
				color = groupMessageColor;
				break;
	}
	return color;
}
	[System.Serializable]
	public class Message
	{
		public string text;
		public TextMeshProUGUI textObject;
		public MessageType messageType;
		public enum MessageType
		{
			groupMessage,
			whisperMessage,
			inGameGlobalMessage,
			globalMessage,
			localInfoMessage
		}
	}
}


