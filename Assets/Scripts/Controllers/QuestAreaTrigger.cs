using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestAreaTrigger : MonoBehaviour {

	void OnTriggerEnter (Collider col) {
		Destroy (gameObject);
	}
}
