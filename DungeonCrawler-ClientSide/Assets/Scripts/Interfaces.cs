using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable {
	void OnClicked();
}
public interface IEnemy {
	void GetDamaged(int amount);
}

   
