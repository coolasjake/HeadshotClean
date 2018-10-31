using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		float rotationX = Input.GetAxis ("Mouse X");
		transform.localRotation *= Quaternion.AngleAxis(rotationX, Vector3.up);

		float rotationY = -Input.GetAxis ("Mouse Y");
		transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.right);

		float Speed = 5;
		if (Input.GetKey (KeyCode.LeftShift))
			Speed = Speed * 2;

		if (Input.GetKey (KeyCode.W))
			transform.Translate (transform.forward * Time.deltaTime * Speed);
		if (Input.GetKey (KeyCode.S))
			transform.Translate (-transform.forward * Time.deltaTime * Speed);
		if (Input.GetKey (KeyCode.D))
			transform.Translate (transform.right * Time.deltaTime * Speed);
		if (Input.GetKey (KeyCode.A))
			transform.Translate (-transform.right * Time.deltaTime * Speed);
		if (Input.GetKey (KeyCode.E))
			transform.Translate (transform.up * Time.deltaTime * Speed);
		if (Input.GetKey (KeyCode.Q))
			transform.Translate (-transform.up * Time.deltaTime * Speed);


	}
}
