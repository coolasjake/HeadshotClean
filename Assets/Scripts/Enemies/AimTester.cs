using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimTester : Shootable {

	public int TimeSinceHit = 0;
	public float FullyCooled = 255;
	public bool HitMe = true;

	public override void Hit (float Damage) {
		TimeSinceHit = 0;
		GetComponentInParent<AimGroup> ().BlockHit (HitMe);
		HitMe = false;
	}

	void FixedUpdate () {
		TimeSinceHit += 1;

		if (TimeSinceHit <= FullyCooled)
			if (HitMe)
			GetComponent<Renderer> ().material.color = new Color (TimeSinceHit / FullyCooled, 0, 0);
			else
				GetComponent<Renderer> ().material.color = new Color (TimeSinceHit / FullyCooled, 1, 1);
		else {
			if (HitMe)
				GetComponent<Renderer> ().material.color = new Color (1, 0, 0);
			else
				GetComponent<Renderer> ().material.color = new Color (1, 1, 1);
		}
		
	}
}
