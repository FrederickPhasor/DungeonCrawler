using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UILobbyMenu : MonoBehaviour
{
	[Header("References to Gameobjects")]
	[SerializeField] GameObject LobbyMenuGameObject;
	[SerializeField] List<GameObject> subMenus;
	[SerializeField] List<GameObject> Players;
	[SerializeField] GameObject leaveGroupObject;
	[Header("Pop Up")]
	[SerializeField] GameObject popUpPrefab;
	string whoInvitedUsName;
	string  partnerNamesRaw;
	bool display;
	private void Start()
	{
		LobbyMenuGameObject.SetActive(false);
		partnersUpdated = false;
		display = false;
		LobbyButton();
	}
	private void OnEnable()
	{
		ServerController.PartnersUpdateEvent += SetPartnersNames;
		ServerController.InvitationReceivedEvent += TriggerInvitationPopUp;
	}
	private void OnDisable()
	{
		ServerController.PartnersUpdateEvent -= SetPartnersNames;
		ServerController.InvitationReceivedEvent -= TriggerInvitationPopUp;
	}
	private void Update()
	{
		if (partnersUpdated)
		{
			MakeGroup();
			partnersUpdated = false;
		}
		if (display)
		{
			PopUpApear();
			display = false;
		}
	}
	void SetPartnersNames(string names)
	{
		partnerNamesRaw = names;
		Debug.Log(names);
		partnersUpdated = true;
	}
	void TriggerInvitationPopUp(string originUsername)
	{
		whoInvitedUsName = originUsername;
		display = true;
	}
	public void PopUpApear()
	{
		GameObject newPopUp = Instantiate(popUpPrefab, LobbyMenuGameObject.transform);
		newPopUp.GetComponent<PopUpControler>().popUpName = whoInvitedUsName;
	}
	bool partnersUpdated;
	public string[] namesLOG;
	public void MakeGroup()
	{
		string[] names = partnerNamesRaw.Split('/');
		
		if (names[0] == "EMPTY")
		{
			foreach (GameObject player in Players)
			{
				player.SetActive(false);
			}
		}
		else
		{
			int removeIndex = Array.IndexOf(names, PlayerData.pData.GetName());
			string[] UpdatedNames = new string[names.Length - 1];
			for (int i = 0, j = 0; i < UpdatedNames.Length; i++, j++)
			{
				if (i == removeIndex)
				{
					j++;
				}
				UpdatedNames[i] = names[j];
			}
			names = UpdatedNames;
			namesLOG = names;
			foreach (GameObject player in Players)
			{
				player.SetActive(false);
			}
			for (int i = 0; i < names.Length - 1; i++)
			{
				Players[i].SetActive(true);
				Players[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = names[i];
			}
		}
	}
	public void GoBackButton()
	{
		foreach(GameObject menu in subMenus)
		{
			menu.SetActive(false);
		}
	}
	public void SearchForGame()
	{
		
	}
	void ActivateMenu(int index)
	{
		for(int i = 0; i < subMenus.Count; i++)
		{
			if (i == index)
			{
				subMenus[index].SetActive(true);
			}
			else
				subMenus[i].SetActive(false);
		}
	}
	public void LobbyButton()
	{
		ActivateMenu(0);
	}
	public void ShowConnectedPlayersButton()
	{
		ActivateMenu(1);
	}
	public void ShopButton()
	{
		ActivateMenu(2);
	}
	public void RecentGamesButton()
	{
		ActivateMenu(3);
	}
	public void HeroesButton()
	{
		ActivateMenu(4);
	}
	public void BountyHuntButton()
	{
		ActivateMenu(5);
	}
	public void SettingsButton()
	{
		ActivateMenu(6);
	}
}


