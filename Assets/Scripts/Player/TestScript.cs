using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		//Rotate camera
		float rotationY = -Input.GetAxis ("Mouse Y") * 2;
		//Quaternion RotQ = new Quaternion ();
		//RotQ.eulerAngles = new Vector3 (ClampSmooth (MainCamera.transform.localRotation.eulerAngles.x + rotationY, -Clamp, Clamp, ClampAdjustmentSpeed * Time.deltaTime), 0, 0);
		//RotQ.eulerAngles = new Vector3 (transform.localRotation.eulerAngles.x + rotationY, 0, 0);
		//transform.localRotation = RotQ;

		transform.localRotation *= Quaternion.AngleAxis (rotationY, transform.right);
	}
}
