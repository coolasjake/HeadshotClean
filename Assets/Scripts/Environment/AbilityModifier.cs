using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityModifier : MonoBehaviour {

	public AbilityModules Ability;
	public ModOptions Mode;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnTriggerEnter (Collider col) {
		ModuleController Player = col.gameObject.GetComponentInParent<ModuleController> ();
		if (Mode == ModOptions.Disable)
			Player.DeactivateModule (Ability);
		else if (Mode == ModOptions.Enable)
			Player.ActivateModule (Ability);
		else
			Player.ToggleModule (Ability);
	}
}

public enum ModOptions {
	Disable,
	Enable,
	Toggle
}