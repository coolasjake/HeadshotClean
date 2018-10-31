using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailFloor : MonoBehaviour {

	public GravityTrialController GTC;

	public void DangerMode () {
		GetComponent<Renderer> ().material.color = Color.red;
	}

	public void NormalMode () {
		GetComponent<Renderer> ().material.color = Color.gray;
	}

	public void VictoryMode () {
		GetComponent<Renderer> ().material.color = Color.green;
	}

	void OnCollisionEnter (Collision col) {
		if (col.gameObject.tag == "Player")
			GTC.FailChallenge ();
	}
}
