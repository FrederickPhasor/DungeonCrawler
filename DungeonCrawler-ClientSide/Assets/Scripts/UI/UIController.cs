using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] GameObject slideOptionMenu;
    public void OpenOptionsPanel()
	{
        anim.SetBool("Active", !anim.GetBool("Active"));
	}
    bool active;
    public void ToggleChangeEmailMenu()
	{
        active = true;
        slideOptionMenu.SetActive(active);
        slideOptionMenu.GetComponent<Animator>().SetBool("Active", !slideOptionMenu.GetComponent<Animator>().GetBool("Active"));
	}
    public void DeactivateEmailMenu()
	{
        slideOptionMenu.SetActive(false);
	}
    
}
