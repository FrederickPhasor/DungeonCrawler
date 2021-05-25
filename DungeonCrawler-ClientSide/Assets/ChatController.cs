using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
public class ChatController : MonoBehaviour
{
	[Header("Chat")]
	public Color playerMessage, infoMessage;
	public GameObject chatPanel, textObject;
	public TMP_InputField chatBox;
	[SerializeField] List<Message> messageList = new List<Message>();
	public int maxMessages = 25;
	string newMSG;
	bool chatUpdate;
	private void Update()
	{
		if (chatBox.text != "")
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				chatBox.text = "";
			}
			else
			{
				if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
					chatBox.ActivateInputField();
			}

		}
	}
	public void MessageWaiting(string text)
	{
		newMSG = text;
		chatUpdate = true;

		if (chatUpdate)
		{
			PrepareChatMSG();
			chatUpdate = false;
		}
	}
public void SendMessageToChat(string text, Message.MessageType messageType){
	if (messageList.Count >= maxMessages)
	{
		Destroy(messageList[0].textObject.gameObject);
		messageList.Remove(messageList[0]);
	}
	Message newMessage = new Message();
	newMessage.text = text;
	GameObject newText = Instantiate(textObject, chatPanel.transform);
	newMessage.textObject = newText.GetComponent<TextMeshProUGUI>();
	newMessage.textObject.text = newMessage.text;
	messageList.Add(newMessage);
	newMessage.textObject.color = MessageTypeColor(messageType);
}
private void PrepareChatMSG()
{
	string[] parts = newMSG.Split(new[] { '/' }, 2);
	int num = Convert.ToInt32(parts[0]);
	switch (num)
	{
		case 1://General Chat
			SendMessageToChat(parts[1], Message.MessageType.playerMessage);
			break;
		case 2:
			break;
	}
}

Color MessageTypeColor(Message.MessageType messageType)
{
	Color color = infoMessage;
	switch (messageType)
	{
		case Message.MessageType.playerMessage:
			color = playerMessage;
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
			playerMessage,
			info
		}
	}
}


