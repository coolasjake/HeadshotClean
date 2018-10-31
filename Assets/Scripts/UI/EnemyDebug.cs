using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDebug : MonoBehaviour {

	public float IconSize = 20f;
	public Transform Target;

	private LookingEnemy EnemyScript;

	private Text TextObject;
	//private Image ImageReference;


	void Start() {
		TextObject = GetComponent<Text> ();
		//ImageReference = GetComponent<Image> ();
		EnemyScript = Target.GetComponent<LookingEnemy> ();
		//ChevronInstance = Instantiate (Resources.Load<GameObject>("Prefabs/Chevron"), transform);
	}

	// Update is called once per frame
	void Update () {
		if (Target == null) {
			Destroy (gameObject);
			return;
		}

		TextObject.text = EnemyScript.State.ToString();

		/*
		if (EnemyScript.DetectionProgress > 0) {
			if (EnemyScript.DetectionProgress >= 2) {
				transform.localScale = new Vector3 (1, 1, 1);
				ImageReference.color = Color.red;
			} else if (EnemyScript.DetectionProgress >= 1) {
				transform.localScale = new Vector3 (1, 1, 1);
				ImageReference.color = Color.yellow;
			} else {
				transform.localScale = new Vector3 (1, EnemyScript.DetectionProgress, 1);
				ImageReference.color = Color.green;
			}
		} else {
			ImageReference.enabled = false;
			return;
		}
		if (EnemyScript.DetectionProgress > 0 && EnemyScript.DetectionProgress < LookingEnemy.MinimumDetectionTime) {
			//TextObject.text = "(-) " + (EnemyScript.DetectionProgress) / LookingEnemy.MinimumDetectionTime;
			transform.localScale = new Vector3(1, (EnemyScript.DetectionProgress) / LookingEnemy.MinimumDetectionTime, 1);
			ImageReference.color = Color.yellow;
		} else if (Time.time < EnemyScript.LastSawPlayer + LookingEnemy.ForgetTime) {
			transform.localScale = new Vector3 (1, (EnemyScript.LastSawPlayer - Time.time + LookingEnemy.ForgetTime) / LookingEnemy.ForgetTime, 1);
			ImageReference.color = Color.red;
		} else {
			ImageReference.enabled = false;
			return;
		}
		*/

		Vector3 wantedPos = Camera.main.WorldToScreenPoint (Target.position + new Vector3(0, 2, 0));
		transform.position = wantedPos;

		bool OffScreen = false;
		float DesiredAngle = -1;
		//Vector3 DesiredLocation = new Vector3 (0, 0, 0);
		//DesiredLocation = new Vector3(Screen.width - 100, DesiredLocation.y, 0);
		//DesiredLocation = new Vector3(0 + 100, DesiredLocation.y, 0);

		TextObject.enabled = true;
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
