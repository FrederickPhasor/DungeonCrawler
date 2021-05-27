using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIHostCell : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI hostUsername;
	private void Update()
	{
		hostUsername.text = PlayerData.pData.GetName();
	}
}
