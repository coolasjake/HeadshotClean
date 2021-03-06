﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetectionIndicator : MonoBehaviour {
	
	public float IconSize = 50f;
	public Transform Target;

	private LookingEnemy EnemyScript;

	//private Text TextObject;
	private Image ImageReference;


	void Start() {
		//TextObject = GetComponent<Text> ();
		ImageReference = GetComponent<Image> ();
		EnemyScript = Target.GetComponent<LookingEnemy> ();
		//ChevronInstance = Instantiate (Resources.Load<GameObject>("Prefabs/Chevron"), transform);
	}

	// Update is called once per frame
	void Update () {
		if (Target == null) {
			Destroy (gameObject);
			return;
		}

		if (!Target.gameObject.activeInHierarchy) {
			transform.position = new Vector3 (0, 0, -100);
			return;
		}

		if (EnemyScript.State == AIState.Working && EnemyScript.DetectionProgress > 0) {
			transform.localScale = new Vector3 (1, EnemyScript.DetectionProgress, 1);
			ImageReference.color = Color.green;
		} else if (EnemyScript.State == AIState.Searching) {
			transform.localScale = new Vector3 (1, 1, 1);
			ImageReference.color = Color.magenta;
		} else if (EnemyScript.State == AIState.Staring) {
			transform.localScale = new Vector3 (1, 1, 1);
			ImageReference.color = Color.yellow;
		} else if (EnemyScript.State == AIState.Charging || EnemyScript.State == AIState.Firing) {
			transform.localScale = new Vector3 (1, 1, 1);
			ImageReference.color = Color.red;
		} else if (EnemyScript.State == AIState.Alarmed) {
			transform.localScale = new Vector3 (2, 2, 1);
			ImageReference.color = Color.black;
		} else {
			ImageReference.enabled = false;
			return;
		}

		Vector3 wantedPos = Camera.main.WorldToScreenPoint (Target.position + new Vector3(0, 2, 0));
		transform.position = wantedPos;

		bool OffScreen = false;
		float DesiredAngle = -1;
		//Vector3 DesiredLocation = new Vector3 (0, 0, 0);
		//DesiredLocation = new Vector3(Screen.width - 100, DesiredLocation.y, 0);
		//DesiredLocation = new Vector3(0 + 100, DesiredLocation.y, 0);

		ImageReference.enabled = true;
		if (transform.position.z < 0) {
			//TextObject.enabled = false;
			if (transform.position.x + IconSize < Screen.width / 2) {
				//On right
				transform.position = new Vector3 (Screen.width - IconSize, transform.position.y, transform.position.z);
				DesiredAngle = 90;
				OffScreen = true;
			} else {
				//On left
				transform.position = new Vector3 (0 + IconSize, transform.position.y, transform.position.z);
				DesiredAngle = 270;
				OffScreen = true;
			}

			if (transform.position.y + IconSize > Screen.height) {
				//Debug.Log ("OutsideScreen Top ?");
				transform.position = new Vector3 (transform.position.x, 0 + IconSize, transform.position.z);
				if (DesiredAngle == -1)
					DesiredAngle = 0;
				else {
					if (DesiredAngle == 270)
						DesiredAngle = 315;
					else
						DesiredAngle = 45;
				}
				OffScreen = true;
			} else if (transform.position.y - IconSize < 0) {
				//Debug.Log ("OutsideScreen Bottom");
				transform.position = new Vector3 (transform.position.x, Screen.height - IconSize, transform.position.z);
				if (DesiredAngle == -1)
					DesiredAngle = 180;
				else
					DesiredAngle = (DesiredAngle + 180) / 2;
				OffScreen = true;
			}
		} else {

			if (transform.position.x + IconSize > Screen.width) {
				//Debug.Log ("OutsideScreen Right");
				transform.position = new Vector3 (Screen.width - IconSize, transform.position.y, transform.position.z);
				DesiredAngle = 90;
				OffScreen = true;
			} else if (transform.position.x - IconSize < 0) {
				//Debug.Log ("OutsideScreen Left");
				transform.position = new Vector3 (0 + IconSize, transform.position.y, transform.position.z);
				DesiredAngle = 270;
				OffScreen = true;
			}

			if (transform.position.y + IconSize > Screen.height) {
				//Debug.Log ("OutsideScreen Top ?");
				transform.position = new Vector3 (transform.position.x, Screen.height - IconSize, transform.position.z);
				if (DesiredAngle == -1)
					DesiredAngle = 180;
				else
					DesiredAngle = (DesiredAngle + 180) / 2;
				OffScreen = true;
			} else if (transform.position.y - IconSize < 0) {
				//Debug.Log ("OutsideScreen Bottom");
				transform.position = new Vector3 (transform.position.x, 0 + IconSize, transform.position.z);
				if (DesiredAngle == -1)
					DesiredAngle = 0;
				else {
					if (DesiredAngle == 270)
						DesiredAngle = 315;
					else
						DesiredAngle = 45;
				}
				OffScreen = true;
			}
		}

		if (OffScreen) {
			//ChevronInstance.GetComponent<Image> ().enabled = true;
			transform.rotation = new Quaternion ();
			transform.Rotate (0, 0, DesiredAngle);
		} else {
			//ChevronInstance.GetComponent<Image> ().enabled = false;
			//transform.localPosition = new Vector3();
			transform.rotation = new Quaternion ();
		}
	}
}
