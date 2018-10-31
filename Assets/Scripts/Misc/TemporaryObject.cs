using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryObject : MonoBehaviour {

	public float DestructionTime = 4f;

	// Use this for initialization
	void Start () {
		Destroy (gameObject, DestructionTime);
	}
}
