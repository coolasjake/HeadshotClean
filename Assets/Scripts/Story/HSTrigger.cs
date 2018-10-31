using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HSTrigger : MonoBehaviour {

	public bool DestroyOnTrigger = true;
	public bool DestroyScriptOnTrigger = false;

	public delegate void EventsToTrigger ();
	public event EventsToTrigger Events;

	void Start () {
		Events += DestroyThis;
	}

	void DestroyThis () {
		if (DestroyOnTrigger)
			Destroy (gameObject);
		else if (DestroyScriptOnTrigger)
			Destroy (this);
	}

	protected void Trigger () {
		if (Events != null)
			Events ();
	}
}