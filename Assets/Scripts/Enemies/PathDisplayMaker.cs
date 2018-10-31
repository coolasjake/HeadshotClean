using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDisplayMaker : MonoBehaviour {

	private float TimeOfLastPoint = 0;

	public float PointDropRate = 1f;
	public GameObject PointObject;
	
	// Update is called once per frame
	void Update () {
		if (Time.time > TimeOfLastPoint + PointDropRate) {
			TimeOfLastPoint = Time.time;
			Instantiate (PointObject, transform.position, new Quaternion ());
		}
	}
}
