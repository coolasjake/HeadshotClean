using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour {

	public float BobHeight = 1;
	public float BobTime = 1;
	public float MaxBobSpeed = 2;

	private float StartBobTime = 0;
	//private float BobTimeStamp = 0;
	private bool GoingUp = true;
	
	// Update is called once per frame
	void Update () {
		float XTime = (StartBobTime - Time.time) - (BobTime/2);
		float MoveDist = Time.deltaTime * ((((StartBobTime - Time.time) - (BobTime / 2)) / BobTime) * BobHeight * Time.deltaTime);
		MoveDist = -(MaxBobSpeed * Mathf.Pow(XTime, 2)) + MaxBobSpeed;
		if (GoingUp) {
			transform.position += new Vector3 (0, MoveDist, 0);
			if (Time.time > StartBobTime + BobTime) {
				StartBobTime = Time.time;
				GoingUp = false;
			}
		} else {
			transform.position -= new Vector3 (0, MoveDist, 0);
			if (Time.time > StartBobTime + BobTime) {
				StartBobTime = Time.time;
				GoingUp = true;
			}
		}
	}
}
