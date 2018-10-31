using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propellor : MonoBehaviour {

	public float RPS = 1;

	// Update is called once per frame
	void Update () {
		Quaternion New = new Quaternion ();
		New.eulerAngles = transform.localRotation.eulerAngles + new Vector3 (0, Time.deltaTime * RPS * 360, 0);
		transform.localRotation = New;
	}
}
