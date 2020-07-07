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
            gun.Disable();
			phase.Disable();
            gravity.Disable();
            stealth.Disable();
            teleport.Disable();
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void DeactivateModule (AbilityModules Module) {
		if (DebugMode)
			return;
        //Debug.Log ("Deactivating " + Module.ToString ());
        if (Module == AbilityModules.Gun)
            gun.Disable();
        else if (Module == AbilityModules.Phase)
            phase.Disable();
        else if (Module == AbilityModules.Gravity)
            gravity.Disable();
        else if (Module == AbilityModules.Stealth)
            stealth.Disable();
        else if (Module == AbilityModules.Teleport)
            teleport.Disable();
    }

	public void ActivateModule (AbilityModules Module) {
		if (DebugMode)
			return;
		//Debug.Log ("Activating " + Module.ToString ());
		if (Module == AbilityModules.Gun)
            gun.Enable();
        else if (Module == AbilityModules.Phase)
			phase.Enable();
        else if (Module == AbilityModules.Gravity)
			gravity.Enable();
        else if (Module == AbilityModules.Stealth)
			stealth.Enable();
        else if (Module == AbilityModules.Teleport)
			teleport.Enable();
    }

	public void AcivateAllModules () {
		if (DebugMode)
			return;
		gun.Enable();
		phase.Enable();
        gravity.Enable();
        stealth.Enable();
        teleport.Enable();

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