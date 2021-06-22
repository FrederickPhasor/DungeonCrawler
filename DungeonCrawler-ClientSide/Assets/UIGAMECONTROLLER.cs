using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGAMECONTROLLER : MonoBehaviour
{
	[SerializeField] GameObject inventoryObject;
	[SerializeField] GameObject MapGameObject;

	public void SwapMenus()
	{
		inventoryObject.SetActive(!inventoryObject.activeInHierarchy);
		MapGameObject.SetActive(!inventoryObject.activeInHierarchy);
	}
}
