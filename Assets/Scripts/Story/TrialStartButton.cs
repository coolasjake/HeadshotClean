using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialStartButton : MonoBehaviour {

	public GravityTrialController GTC;
	public Transform Button;

	private void Depress () {
		Button.localScale = new Vector3 (1, 0.21f, 1);
		GetComponent<Collider> ().enabled = false;
	}

	public void PopUp () {
		Button.localScale = new Vector3 (1, 0.4f, 1);
		GetComponent<Collider> ().enabled = true;
	}

	void OnCollisionEnter (Collision col) {
		if (col.gameObject.tag == "Player") {
			GTC.StartChallenge ();
			Depress ();
		}
	}
}
