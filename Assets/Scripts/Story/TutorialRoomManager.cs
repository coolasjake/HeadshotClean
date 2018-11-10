using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRoomManager : MonoBehaviour {

	public Door StealthDoor;
	public Door GravityDoor;
	public Door PhaseDoor;
	public Door TeleportDoor;

	public GameObject Everything;
	public GameObject Tutorial;

	private int NextDoor = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OpenNextDoor () {
		if (NextDoor == 0)
			StealthDoor.Unlock ();
		else if (NextDoor == 1)
			GravityDoor.Unlock ();
		else if (NextDoor == 2)
			PhaseDoor.Unlock ();
		else if (NextDoor == 3)
			TeleportDoor.Unlock ();
		/*
		*/
		NextDoor += 1;

		//if (NextDoor == 4)
		//	FindObjectOfType <ModuleController> ().AcivateAllModules ();
	}


	void OnTriggerEnter (Collider col) {
		Movement Player = col.gameObject.GetComponentInParent<Movement> ();
		if (Player) {
			Everything.SetActive (true);
			Tutorial.SetActive (false);
		}
	}
}
