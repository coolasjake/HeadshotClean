using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionTester : Shootable {

	public bool PartOfGroup = false;

	public int MaxWaitTime = 90;
	public int MinWaitTime = 15;

	public int Fouls = 0;
	public int Hits = 0;
	public float TotalReactionTime = 0;

	private int TimeSinceReady = 0;
	private int TimeToReady = 150;
	private bool Ready = false;
	private bool Foul = false;

	public bool SetReady {
		set { Ready = value; }
	}

	public override void Hit (float Damage) {
		if (PartOfGroup) {
			GetComponentInParent<ReactionGroup> ().BlockHit (Ready);
			Ready = false;
			return;
		}

		if (Ready) {
			Hits += 1;
			TotalReactionTime += TimeSinceReady;
			TimeSinceReady = 0;
			TimeToReady = Random.Range (MinWaitTime, MaxWaitTime);
			Ready = false;
		} else {
			Foul = true;
			Fouls += 1;
			TimeToReady = Random.Range (MinWaitTime, MaxWaitTime);
		}
	}

	void FixedUpdate () {
		if (PartOfGroup)
			return;

		if (Ready) {
			TimeSinceReady += 1;
		} else {
			TimeToReady -= 1;
			if (TimeToReady <= 0) {
				Ready = true;
				Foul = false;
			}
		}
		if (Ready)
			GetComponent<Renderer> ().material.color = new Color (0, 1, 0);
		else {
			if (Foul)
				GetComponent<Renderer> ().material.color = new Color (1, 0, 0);
			else
				GetComponent<Renderer> ().material.color = Color.HSVToRGB ((((TotalReactionTime / Hits) - 15f) / 50f) + 0.5f, 1, 1);
		}
	}
}
