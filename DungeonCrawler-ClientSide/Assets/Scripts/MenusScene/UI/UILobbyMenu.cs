using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UILobbyMenu : MonoBehaviour
{
	[Header("References to Gameobjects")]
	[SerializeField] GameObject LobbyMenuGameObject;
	[SerializeField] GameObject LogInMenuGameObject;
	[SerializeField] List<GameObject> subMenus;
	[SerializeField] List<GameObject> Players;
	[SerializeField] GameObject leaveGroupObject;
	[Header("Pop Up")]
	[SerializeField] GameObject popUpPrefab;
	string whoInvitedUsName;
	bool partnersUpdate;
	bool display;
	private void Start()
	{
		LobbyMenuGameObject.SetActive(false);
		partnersUpdate = false;
		dissolve = false;
		display = false;
		LobbyButton();
	}
	private void OnEnable()
	{
		ServerController.ModifyPartnersEvent += TriggerPartnerUpdate;
		ServerController.InvitationReceivedEvent += TriggerInvitationPopUp;
		ServerController.GroupDissolvedEvent += TriggerDissolve;
	}
	private void OnDisable()
	{
		ServerController.ModifyPartnersEvent -= TriggerPartnerUpdate;
		ServerController.InvitationReceivedEvent -= TriggerInvitationPopUp;
		ServerController.GroupDissolvedEvent -= TriggerDissolve;
	}
	private void Update()
	{
		if (display)
		{
			PopUpApear();
			display = false;
		}
		if (dissolve)
		{
			DissolveGroup();
			dissolve = false;
			return;
		}
		if (partnersUpdate)
		{
			partnersUpdate = false;
			DissolveGroup();
			foreach(string partnerName in partnersNames)
			{
				AddAPartner(partnerName);
			}
		}
	}
	public List<string> partnersNames = new List<string>();
	void TriggerPartnerUpdate(string updatedGroup)
	{
		partnersNames.Clear();
		foreach(string name in updatedGroup.Split('/'))
		{
				partnersNames.Add(name);
		}
		partnersUpdate = true;
	}
	void TriggerInvitationPopUp(string originUsername)
	{
		whoInvitedUsName = originUsername;
		display = true;
	}
	bool dissolve;
	void TriggerDissolve()
	{
		dissolve = true;
	}
	public void PopUpApear()
	{
		GameObject newPopUp = Instantiate(popUpPrefab, LobbyMenuGameObject.transform);
		newPopUp.GetComponent<PopUpControler>().popUpName = whoInvitedUsName;
	}
	void DeletePartner()
	{
		//Debug.Log("We are going to delete a user  : "+ partnerName);
		//foreach (GameObject player in Players)
		//{
		//	if (player.activeInHierarchy)
		//	{
		//		print(player.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
		//		print(player.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == partnerName);
		//		if (player.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == partnerName)	
		//		{
		//			Debug.Log("We foud the guy");
		//			player.SetActive(false);
		//		}
		//	}
		//}
	}
	public void AddAPartner(string partnerName)
	{
		foreach (GameObject player in Players)
		{
			if (player.activeInHierarchy == false)
			{
				player.SetActive(true);
				player.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = partnerName + "\n";
				return;
			}
		}
	}
	void DissolveGroup()
	{
		foreach (GameObject player in Players)
		{
			player.SetActive(false);
		}
	}
	public void GoBackButton()
	{
		foreach(GameObject menu in subMenus)
		{
			menu.SetActive(false);
		}
		subMenus[0].SetActive(true);
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
	public void ExitButton()
	{
		LogInMenuGameObject.SetActive(true);
		transform.GetComponent<UILogInMenu>().enabled = true;
		LobbyMenuGameObject.SetActive(false);
		this.enabled = false;
	}
}


