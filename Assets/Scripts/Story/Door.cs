using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

	public GameObject Bar;

	private float AutoCloseTime = 2;
	private float OpenSpeed = 4;

	private float OpenTime = 0;
	private bool Locked = true;
	private bool Opening = false;
	private bool Open = false;
	private bool Closing = false;

	private Rigidbody RB;
	private Transform DoorTransform;

	void Start () {
		RB = GetComponentInChildren <Rigidbody> ();
		DoorTransform = RB.GetComponent<Transform> ();
	}

	// Update is called once per frame
	void Update () {
		if (Opening && DoorTransform.localPosition.y > 4.5f) {
			DoorTransform.localPosition = new Vector3 (0, 4.5f, 0);
			Open = true;
			RB.velocity = new Vector3 (0, 0, 0);
		} else if (Closing && DoorTransform.localPosition.y < 0) {
			DoorTransform.localPosition = new Vector3 (0, 0, 0);
			RB.velocity = new Vector3 (0, 0, 0);
		}

		if (Open && Time.time > OpenTime + AutoCloseTime) {
			CloseDoor ();
		}
	}

	public void OpenDoor () {
		Closing = false;
		OpenTime = Time.time;
		if (Opening == false && Open == false) {
			Opening = true;
			RB.velocity = new Vector3 (0, OpenSpeed, 0);
		}
	}

	public void CloseDoor () {
		Closing = true;
		Opening = false;
		Open = false;
		RB.velocity = new Vector3 (0, -OpenSpeed, 0);
	}

	public void Unlock () {
		Locked = false;
		if (Bar != null)
			Destroy (Bar);
	}

	void OnTriggerStay (Collider col) {
		Movement Player = col.gameObject.GetComponentInParent<Movement> ();
		if (Player && !Locked)
			OpenDoor ();
	}
}
