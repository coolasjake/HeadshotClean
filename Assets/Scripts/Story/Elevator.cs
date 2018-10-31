using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour {

	public float Speed = 1f;
	public float MaxHeight = 9;
	public TutorialRoomManager TRM;
	private float StartHeight;

	// Use this for initialization
	void Start () {
		StartHeight = transform.localPosition.y;
		GetComponent<Rigidbody> ().velocity = new Vector3 (0, Speed, 0);
		if (TRM == null)
			TRM = FindObjectOfType<TutorialRoomManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.localPosition.y > StartHeight + MaxHeight) {
			transform.localPosition = new Vector3 (0, StartHeight + MaxHeight, 0);
			TRM.OpenNextDoor ();
			Destroy (GetComponent<Rigidbody>());
			Destroy (this);
		}
	}
}
