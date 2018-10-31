using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHealer : MonoBehaviour {

	void OnTriggerStay (Collider Col) {
		
		Movement Player = Col.GetComponentInParent<Movement> ();
		Debug.Log ("Player? :" + Col.gameObject.name);
		if (Player) {
			if (Player.Health < 100)
				Player.Hit(-0.1f);
		}
	}
}
