﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : PlayerMovement {

	//public static Movement ThePlayer;

	// Use this for initialization
	void Start () {
		ThePlayer = this;
		MainCamera = GetComponentInChildren<Camera> ();
		RB = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {

		/*
		float rotationX = Input.GetAxis ("Mouse X");
		MainCamera.transform.localRotation *= Quaternion.AngleAxis(rotationX, Vector3.up);

		float rotationY = -Input.GetAxis ("Mouse Y");
		transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.right);
		*/


		float rotationX = -Input.GetAxis ("Mouse X") * Sensitivity;
		transform.localRotation *= Quaternion.AngleAxis(rotationX, Vector3.down);

		//Rotate camera
		if (_CameraAngle > 180)
			_CameraAngle = ClampAngleTo180 (_CameraAngle);

		bool CameraWasWithinClamp = (_CameraAngle <= 90) && (_CameraAngle >= -90);
		_CameraAngle -= Input.GetAxis ("Mouse Y") * Sensitivity;
		if (_CameraAngle > 90 || _CameraAngle < -90) {
			if (CameraWasWithinClamp)
				_CameraAngle = Mathf.Clamp (_CameraAngle, -90, 90);
			else {
				if (_CameraAngle > 90)
					_CameraAngle -= clampAdjustmentSpeed * Time.deltaTime;
				if (_CameraAngle < -90)
					_CameraAngle += clampAdjustmentSpeed * Time.deltaTime;
			}
		}

		Quaternion NewRot = new Quaternion ();
		NewRot.eulerAngles = new Vector3 (_CameraAngle, MainCamera.transform.localRotation.y, 0);
		MainCamera.transform.localRotation = NewRot;



		float Speed = 500;
		if (Input.GetKey (KeyCode.LeftShift))
			Speed = Speed * 2;

		RB.velocity = Vector3.zero;

		if (Input.GetKey (KeyCode.W))
			//transform.position += (MainCamera.transform.forward * Time.deltaTime * Speed);
			//transform.Translate (MainCamera.transform.forward * Time.deltaTime * Speed);
			RB.velocity += (MainCamera.transform.forward * Time.deltaTime * Speed);
		if (Input.GetKey (KeyCode.S))
			RB.velocity += (-MainCamera.transform.forward * Time.deltaTime * Speed);
		if (Input.GetKey (KeyCode.D))
			RB.velocity += (MainCamera.transform.right * Time.deltaTime * Speed);
		if (Input.GetKey (KeyCode.A))
			RB.velocity += (-MainCamera.transform.right * Time.deltaTime * Speed);
		if (Input.GetKey (KeyCode.E))
			RB.velocity += (MainCamera.transform.up * Time.deltaTime * Speed);
		if (Input.GetKey (KeyCode.Q))
			RB.velocity += (-MainCamera.transform.up * Time.deltaTime * Speed);


	}
}
