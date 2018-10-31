using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityRep : MonoBehaviour {

	public Vector3 Down;
	private float NormalGravity;

	// Use this for initialization
	void Start () {
		NormalGravity = Physics.gravity.magnitude;
		Down = new Vector3 (0, -NormalGravity, 0);
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = Quaternion.LookRotation(Down);
		transform.localScale = new Vector3 (1, 1, Down.magnitude / NormalGravity);
	}
}