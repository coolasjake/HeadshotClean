using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerChecker : MonoBehaviour {

	//An object that can be created by an object to detect trigger collisions for it at certain points.

	public string Owner = "Noone";
	public bool Triggered = false;

	void OnTriggerStay (Collider col) {
			Triggered = true;
	}

	void OnTriggerExit (Collider col) {
			Triggered = false;
	}
}
