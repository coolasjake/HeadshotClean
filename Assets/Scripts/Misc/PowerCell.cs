using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HSTrigger))]
public class PowerCell : MonoBehaviour {

	public AbilityModules Module;
	public bool Tutorial = true;

	void OnEnable () {
		GetComponent<HSTrigger> ().Events += ActivateModule;
	}

	void OnDisable () {
		GetComponent<HSTrigger> ().Events -= ActivateModule;
	}

	void ActivateModule () {
		Movement.ThePlayer.GetComponent<ModuleController> ().ActivateModule (Module);
		if (Tutorial)
			FindObjectOfType<TutorialRoomManager> ().OpenNextDoor ();
	}
}