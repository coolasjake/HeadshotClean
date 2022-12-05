using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abilities {
	public bool Shoot;
	public bool Phase;
	public bool Die;
}

public class ActionRecorder : MonoBehaviour {

	private List<Vector3> Positions = new List<Vector3> ();
	private List<Quaternion> PlayerRotations = new List<Quaternion> ();
	private List<Quaternion> GunRotations = new List<Quaternion> ();
	private List<Abilities> Actions = new List<Abilities> ();
	private int Step = 0;

	private Abilities LastFrameActions = new Abilities ();

	public bool Recording = false;
	public bool Playing = false;

	// Use this for initialization
	void Start () {
		
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.P)) {
			if (Recording) {
				Play ();
				GetComponent<PlayerMovement> ().enabled = false;
			} else if (Playing) {
				GetComponent<PlayerMovement> ().enabled = true;
				Playing = false;
			} else {
				Recording = true;
				Step = 0;
				Positions = new List<Vector3> ();
				PlayerRotations = new List<Quaternion> ();
				GunRotations = new List<Quaternion> ();
				Actions = new List<Abilities> ();
			}
		}

		if (Recording) {
			//Record actions that occur once but aren't synchronised with fixed update.
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (Recording) {
			Positions.Add (transform.position);
			PlayerRotations.Add (transform.rotation);
			GunRotations.Add (GetComponentInChildren<Camera> ().transform.rotation);
		} else if (Playing) {
			if (Step < Positions.Count) {
				transform.position = Positions [Step];
				transform.rotation = PlayerRotations [Step];
				GetComponentInChildren<Camera> ().transform.rotation = GunRotations [Step];
				Step += 1;
			} else
				Step = 0;
		}
	}

	public void Play () {
		Playing = true;
		Recording = false;
	}
}
