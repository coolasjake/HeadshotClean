using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaSegmentTrigger : MonoBehaviour {

	public List<GameObject> ObjectsToUnload = new List<GameObject>();
	public List<GameObject> ObjectsToLoad = new List<GameObject>();

	void OnTriggerEnter (Collider col) {
		Movement Player = col.gameObject.GetComponentInParent<Movement> ();
		if (Player) {
			foreach (GameObject GO in ObjectsToUnload)
				GO.SetActive (false);
			foreach (GameObject GO in ObjectsToLoad)
				GO.SetActive (true);
		}
	}
}
