using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestPointer : MonoBehaviour {

	public float ChevronSize = 50f;
	public Transform Target;
	public string Message;
	public int Distance;
	public int HideDistance;

	public QuestManager Manager;

	private GameObject ChevronInstance;

	private Text TextObject;


	void Start() {
		TextObject = GetComponent<Text> ();
		ChevronInstance = Instantiate (Resources.Load<GameObject>("Prefabs/Chevron"), transform);
		ChevronInstance.GetComponent<RectTransform>().sizeDelta = new Vector2(ChevronSize, ChevronSize);
		//ChevronInstance.GetComponent<Image> ().enabled = false;
		Manager = FindObjectOfType<QuestManager> ();
	}

	// Update is called once per frame
	void Update () {
		if (Target == null) {
			Manager.CheckQuests();
			Destroy (gameObject);
			return;
		}

		Vector3 wantedPos = Camera.main.WorldToScreenPoint (Target.position + new Vector3(0, 4, 0));
		transform.position = wantedPos;


		Distance = Mathf.RoundToInt(Vector3.Distance (Target.position, Movement.ThePlayer.transform.position));
		float DistanceScale;
		if (Distance > 10) {
			TextObject.text = Message + "\n" + Distance + "m";
			if (Distance < 80f)
				DistanceScale = 1f - ((Distance - 10f) / 100f);
			else
				DistanceScale = 1f / 4f;
		} else {
			TextObject.text = Message;
			DistanceScale = 1;
		}
		transform.localScale = new Vector3 (DistanceScale, DistanceScale, 1);

		bool OffScreen = false;
		float DesiredAngle = -1;
		//Vector3 DesiredLocation = new Vector3 (0, 0, 0);
		//DesiredLocation = new Vector3(Screen.width - 100, DesiredLocation.y, 0);
		//DesiredLocation = new Vector3(0 + 100, DesiredLocation.y, 0);

		TextObject.enabled = true;
		if (transform.position.z < 0) {
			TextObject.enabled = false;
			if (transform.position.x + ChevronSize < Screen.width / 2) {
				//On right
				ChevronInstance.transform.position = new Vector3 (Screen.width - ChevronSize, transform.position.y, ChevronInstance.transform.position.z);
				DesiredAngle = 90;
				OffScreen = true;
			} else {
				//On left
				ChevronInstance.transform.position = new Vector3 (0 + ChevronSize, transform.position.y, ChevronInstance.transform.position.z);
				DesiredAngle = 270;
				OffScreen = true;
			}

			if (transform.position.y + ChevronSize > Screen.height) {
				//Debug.Log ("OutsideScreen Top ?");
				ChevronInstance.transform.position = new Vector3 (ChevronInstance.transform.position.x, 0 + ChevronSize, ChevronInstance.transform.position.z);
				if (DesiredAngle == -1)
					DesiredAngle = 0;
				else {
					if (DesiredAngle == 270)
						DesiredAngle = 315;
					else
						DesiredAngle = 45;
				}
				OffScreen = true;
			} else if (transform.position.y - ChevronSize < 0) {
				//Debug.Log ("OutsideScreen Bottom");
				ChevronInstance.transform.position = new Vector3 (ChevronInstance.transform.position.x, Screen.height - ChevronSize, ChevronInstance.transform.position.z);
				if (DesiredAngle == -1)
					DesiredAngle = 180;
				else
					DesiredAngle = (DesiredAngle + 180) / 2;
				OffScreen = true;
			}
		} else {

			if (transform.position.x + ChevronSize > Screen.width) {
				//Debug.Log ("OutsideScreen Right");
				ChevronInstance.transform.position = new Vector3 (Screen.width - ChevronSize, transform.position.y, ChevronInstance.transform.position.z);
				DesiredAngle = 90;
				OffScreen = true;
			} else if (transform.position.x - ChevronSize < 0) {
				//Debug.Log ("OutsideScreen Left");
				ChevronInstance.transform.position = new Vector3 (0 + ChevronSize, transform.position.y, ChevronInstance.transform.position.z);
				DesiredAngle = 270;
				OffScreen = true;
			}

			if (transform.position.y + ChevronSize > Screen.height) {
				//Debug.Log ("OutsideScreen Top ?");
				ChevronInstance.transform.position = new Vector3 (ChevronInstance.transform.position.x, Screen.height - ChevronSize, ChevronInstance.transform.position.z);
				if (DesiredAngle == -1)
					DesiredAngle = 180;
				else
					DesiredAngle = (DesiredAngle + 180) / 2;
				OffScreen = true;
			} else if (transform.position.y - ChevronSize < 0) {
				//Debug.Log ("OutsideScreen Bottom");
				ChevronInstance.transform.position = new Vector3 (ChevronInstance.transform.position.x, 0 + ChevronSize, ChevronInstance.transform.position.z);
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
			ChevronInstance.transform.rotation = new Quaternion ();
			ChevronInstance.transform.Rotate (0, 0, DesiredAngle);
		} else {
			//ChevronInstance.GetComponent<Image> ().enabled = false;
			ChevronInstance.transform.localPosition = new Vector3();
			ChevronInstance.transform.rotation = new Quaternion ();
		}
	}
}
