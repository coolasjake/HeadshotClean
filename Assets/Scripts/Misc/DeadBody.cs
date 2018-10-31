using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBody : DistinctObject {

	public float DestructionTime = 4f;

	void Start () {
		GameObject GO = Instantiate (new GameObject (), transform);
		GO.AddComponent<Alarm> ();

		Destroy (gameObject, DestructionTime);
	}
}
