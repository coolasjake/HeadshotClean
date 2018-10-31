using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour {

	private float LastHit = -5;

	void OnTriggerEnter (Collider col) {
		Movement Player = col.gameObject.GetComponentInParent<Movement> ();
		if (Player && Time.time > LastHit + 0.1f) {
			LastHit = Time.time;
			Player.Kill ();
		}
	}
}
