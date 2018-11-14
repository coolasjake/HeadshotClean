using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleController : MonoBehaviour {

	private Gun gun;
	private PlayerAbility phase;
	private PlayerAbility gravity;
	private PlayerAbility stealth;
	private PlayerAbility teleport;

	public bool Tutorial = true;
	public bool DebugMode = false;

	// Use this for initialization
	void Start () {
		if (DebugMode)
			return;

		gun = GetComponentInChildren<Gun> ();
		phase = GetComponentInChildren<Phasing> ();
		gravity = GetComponentInChildren<Gravity> ();
		stealth = GetComponentInChildren<Invisibility> ();
		teleport = GetComponentInChildren<Teleport> ();

		if (Tutorial) {
			gun.Disabled = true;
			gun.HideGun ();
			phase.Disabled = true;
			gravity.Disabled = true;
			stealth.Disabled = true;
			teleport.Disabled = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void DeactivateModule (AbilityModules Module) {
		if (DebugMode)
			return;
		//Debug.Log ("Deactivating " + Module.ToString ());
		if (Module == AbilityModules.Gun) {
			gun.Disabled = true;
			gun.HideGun ();
		} else if (Module == AbilityModules.Phase)
			phase.Disabled = true;
		else if (Module == AbilityModules.Gravity)
			gravity.Disabled = true;
		else if (Module == AbilityModules.Stealth)
			stealth.Disabled = true;
		else if (Module == AbilityModules.Teleport)
			teleport.Disabled = true;
	}

	public void ActivateModule (AbilityModules Module) {
		if (DebugMode)
			return;
		//Debug.Log ("Activating " + Module.ToString ());
		if (Module == AbilityModules.Gun) {
			gun.Disabled = false;
			gun.RevealGun ();
		} else if (Module == AbilityModules.Phase)
			phase.Disabled = false;
		else if (Module == AbilityModules.Gravity)
			gravity.Disabled = false;
		else if (Module == AbilityModules.Stealth)
			stealth.Disabled = false;
		else if (Module == AbilityModules.Teleport)
			teleport.Disabled = false;
	}

	public void ToggleModule (AbilityModules Module) {
		if (DebugMode)
			return;
		//Debug.Log ("Activating " + Module.ToString ());
		if (Module == AbilityModules.Gun) {
			gun.Disabled = !gun.Disabled;
			if (gun.Disabled)
				gun.HideGun ();
			else
				gun.RevealGun ();
		} else if (Module == AbilityModules.Phase)
			phase.Disabled = !phase.Disabled;
		else if (Module == AbilityModules.Gravity)
			gravity.Disabled = !gravity.Disabled;
		else if (Module == AbilityModules.Stealth)
			stealth.Disabled = !stealth.Disabled;
		else if (Module == AbilityModules.Teleport)
			teleport.Disabled = !teleport.Disabled;
	}

	public void AcivateAllModules () {
		if (DebugMode)
			return;
		gun.Disabled = false;
		gun.RevealGun ();
		phase.Disabled = false;
		gravity.Disabled = false;
		stealth.Disabled = false;
		teleport.Disabled = false;

		/*
		ActivateModule (PowerCellModules.Gun);
		ActivateModule (PowerCellModules.Phase);
		ActivateModule (PowerCellModules.Gravity);
		ActivateModule (PowerCellModules.Stealth);
		ActivateModule (PowerCellModules.Teleport);
		*/
	}
}

public enum AbilityModules {
	Gun,
	Stealth,
	Gravity,
	Phase,
	Teleport,
	None
}