using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HSCollisionTrigger : HSTrigger {

	void OnCollisionEnter (Collision col) {
		Movement Player = col.gameObject.GetComponentInParent<Movement> ();
		if (Player)
			Trigger ();
	}
}
