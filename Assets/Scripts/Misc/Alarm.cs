using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alarm : MonoBehaviour {

	public float DestructionTime = 4f;
	public float Radius = 10f;

	void Start () {
		SphereCollider SC = gameObject.AddComponent<SphereCollider> ();

		SC.radius = Radius;
		SC.isTrigger = true;
		gameObject.layer = 15; //NoPlayerCollisions - otherwise it stuffs up phasing.

		Destroy (gameObject, DestructionTime);
	}

	void OnTriggerEnter (Collider col) {
		LookingEnemy LE = col.GetComponentInParent<LookingEnemy> ();
		if (LE) {
			LE.Alert ();
		}
	}
}
