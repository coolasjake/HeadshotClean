﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Movement))]
public class Invisibility : PlayerAbility {

	private Movement PM;
	private Image ScreenEffect;
	private ResourceMeter Meter;

	// Use this for initialization
	void Start () {
		PM = GetComponent<Movement> ();

        GameObject InvisibilityUI = UIManager.stat.LoadOrGetUI("Gravity");
        ScreenEffect = InvisibilityUI.GetComponentInChildren<Image> ();
		ScreenEffect.enabled = PM.Invisible;
		Meter = FindObjectOfType<Canvas> ().GetComponentsInChildren<ResourceMeter> () [1];
	}
	
	// Update is called once per frame
	void Update () {
		if (!Disabled && Input.GetButtonDown ("Invisibility")) {
			if (PM.Invisible) {
				PM.Invisible = false;
				ScreenEffect.enabled = false;
			} else if (Resource > MinToUse) {
				PM.Invisible = true;
				ScreenEffect.enabled = true;
			}
		}

        if (PM.Invisible)
        {
            ConsumeResource(Time.deltaTime);
            if (Resource <= 0)
            {
                PM.Invisible = false;
                ScreenEffect.enabled = false;
            }
        }
        else if (Resource < MaxResource)
            RegenerateResource();

		Meter.ChangeValue (Resource / MaxResource);
	}
}
