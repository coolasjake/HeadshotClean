using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HSEvent : MonoBehaviour {

	public HSTrigger Trigger;
	public HSEventType Type;

	//Common Options:
	public float Delay;

	//Movement Options:
	public Vector3 Transformation;
	public float Duration;

	private float StartTime;
	private Vector3 OriginalPosition;
	private Quaternion OriginalRotation;


	void OnEnable () {
		if (Trigger != null) {
			Trigger.Events += TriggerEvent;
		}
	}

	void OnDisable () {
		if (Trigger != null)
			Trigger.Events -= TriggerEvent;
	}

	public void TriggerEvent () {
		StartTime = Time.time;

		//Just in case.
		OriginalPosition = transform.position;
		OriginalRotation = transform.localRotation;

		if (Type == HSEventType.Move)
			StartCoroutine ("MoveEvent");
		else if (Type == HSEventType.Rotate)
			StartCoroutine ("RotateEvent");
		else if (Type == HSEventType.Destroy)
				StartCoroutine ("DestructionEvent");
	}

	public IEnumerator MoveEvent () {
		bool Done = false;
		bool Started = false;
		while (!Done) {
			if (Started) {
				float RemainingTime = (StartTime + Delay) + Duration - Time.time;
				float Distance = Vector3.Distance (transform.position, OriginalPosition + Transformation);

				if (RemainingTime <= 0)
					transform.position = OriginalPosition + Transformation;
				else
					transform.position = Vector3.MoveTowards (transform.position, OriginalPosition + Transformation, (Distance / RemainingTime) * Time.deltaTime);
				
				if (Distance <= 0 || RemainingTime <= 0)
					Done = true;

				yield return null;
			} else if (Time.time > StartTime + Delay) {
				Started = true;
				OriginalPosition = transform.position;
				OriginalRotation = transform.localRotation;
			} else
				yield return new WaitForSeconds (0.03f);
		}
	}

	public IEnumerator RotateEvent () {
		bool Done = false;
		bool Started = false;
		while (!Done) {
			//Only perform event if the delay time is up
			if (Started) {
				Quaternion NewRot = new Quaternion ();
				NewRot.eulerAngles = OriginalRotation.eulerAngles + Transformation;

				float RemainingTime = (StartTime + Delay) + Duration - Time.time;

				if (RemainingTime <= 0) {
					transform.localRotation = NewRot;
					Done = true;
				} else
					transform.localRotation = Quaternion.Lerp (OriginalRotation, NewRot, (Duration - RemainingTime) / Duration);

				yield return null;
			} else if (Time.time > StartTime + Delay) {
				//Check if the delay time is up, and set starting position and rotation.
				Started = true;
				OriginalPosition = transform.position;
				OriginalRotation = transform.localRotation;
			} else
				yield return new WaitForSeconds (0.03f);
		}
	}

	public IEnumerator DestructionEvent () {
		bool Done = false;
		while (!Done) {
			if (Time.time > StartTime + Delay) {
				Destroy (gameObject);
				Done = true;
			} else
				yield return new WaitForSeconds (0.1f);
		}
	}
}

public enum HSEventType {
	Move,
	Destroy,
	Rotate
}