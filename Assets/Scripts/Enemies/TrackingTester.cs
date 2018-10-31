using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingTester : Shooteable {

	public int Leniency = 3;
	public int Points = 0;
	public float Possible = 0;
	public float Consecutive = 0;
	public float HighestConsecutive = 0;

	private int UpdatesSinceLastHit = 0;

	public override void Hit (float Damage) {
		Points += 1;
		UpdatesSinceLastHit = 0;
	}

	void FixedUpdate () {
		Possible += 1;
		if (UpdatesSinceLastHit < Leniency)
			Consecutive += 1;
		else {
			if (Consecutive > HighestConsecutive)
				HighestConsecutive = Consecutive;
			Consecutive = 0;
		}
		UpdatesSinceLastHit += 1;

		GetComponent<Renderer> ().material.color = new Color (Consecutive/100, Points/Possible, Consecutive/HighestConsecutive);
	}
}
