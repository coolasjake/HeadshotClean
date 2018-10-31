using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCube : MonoBehaviour {

	public bool Lit = false;
	public GravityTrialController GTC;
	public Renderer Button1;
	public Renderer Button2;
	public Renderer Button3;


	public void ChangeToRed () {
		Lit = false;
		Button1.material.color = Color.red;
		Button2.material.color = Color.red;
		Button3.material.color = Color.red;
	}

	public void ChangeToBlack () {
		Lit = true;
		Button1.material.color = Color.black;
		Button2.material.color = Color.black;
		Button3.material.color = Color.black;
	}

	private void LightUp () {
		Lit = true;
		Button1.material.color = Color.green;
		Button2.material.color = Color.green;
		Button3.material.color = Color.green;
		GTC.ButtonActivated ();
	}

	void OnCollisionEnter (Collision col) {
		if (col.gameObject.tag == "Player") {
			if (Lit == false)
				LightUp ();
		}
	}
}
