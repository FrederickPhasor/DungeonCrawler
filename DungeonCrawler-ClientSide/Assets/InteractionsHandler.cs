using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InteractionsHandler : MonoBehaviour
{
    public Camera myCamera;

    private void Update()
    {
		if (Input.GetMouseButtonDown(0))
		{
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                 selectedTarget = hit.collider.gameObject.GetComponent<CharacterData>().GetName();
            }
        }
    }
    
    public int skillSelected;
    public void SelectSkill(int num)
	{
        skillSelected = num;
	}
    string selectedTarget;
    public string GetSelectedUser()
	{
        return selectedTarget;
	}


}
