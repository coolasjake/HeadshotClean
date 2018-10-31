using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionGroup : Group {
	
	public int MaxWaitTime = 90;
	public int MinWaitTime = 15;

	public int Fouls = 0;
	public float Hits = 0;
	public float TotalReactionTime = 0;

	private int TimeSinceReady = 0;
	private int TimeToReady = 150;
	private bool Ready = false;
	private bool Foul = false;

	private int ChosenTarget = 0;

	public override void CallCommand (ButtonCommands Command)
	{
		if (Command == ButtonCommands.Restart) {
			TotalReactionTime = 0;
			Hits = 0;
			Fouls = 0;
			Foul = false;
			Ready = false;
			TimeToReady = 150;
		}
	}

	public void BlockHit(bool Correct) {
		Debug.Log("Average RT frames: " + (TotalReactionTime/Hits));
		if (Correct && Ready) {
			Debug.Log("That RT was: " + (TimeSinceReady) + ", the total is: " + TotalReactionTime + ", number of hits is: " + Hits);
			Hits += 1;
			TotalReactionTime += TimeSinceReady;
			TimeSinceReady = 0;
			TimeToReady = Random.Range (MinWaitTime, MaxWaitTime);
			ChosenTarget = Mathf.FloorToInt(Random.value * GetComponentsInChildren<ReactionTester> ().Length);
			Ready = false;
		} else {
			Ready = false;
			Foul = true;
			Fouls += 1;
			TimeToReady = Random.Range (MinWaitTime, MaxWaitTime);
			ChosenTarget = Mathf.FloorToInt(Random.value * GetComponentsInChildren<ReactionTester> ().Length);
		}
		//Debug.Log("Average RT frames: " + (TotalReactionTime/Hits));
		Debug.Log("Average RT RBG value: " + 5f / ((TotalReactionTime/Hits) - 20f));
	}

	void FixedUpdate () {
		if (Ready) {
			TimeSinceReady += 1;
		} else {
			TimeToReady -= 1;
			if (TimeToReady <= 0) {
				Ready = true;
				Foul = false;
			}
		}
		if (Ready) {
			GetComponentsInChildren<ReactionTester> () [ChosenTarget].GetComponent<Renderer> ().material.color = new Color (0, 1, 0);
			GetComponentsInChildren<ReactionTester> () [ChosenTarget].SetReady = true;
		} else {
			if (Foul)
				ColourAllTargets (new Color (1, 0, 0));
			else
				ColourAllTargets (Color.HSVToRGB ((((TotalReactionTime / Hits) - 15f) / 70f) + 0.5f, 1, 1));//new Color (0, 3f / ((TotalReactionTime/Hits) - 25f), 3f / ((TotalReactionTime/Hits) - 25f)));
			
		}
	}

	private void ColourAllTargets (Color C) {
		
		foreach (ReactionTester Target in GetComponentsInChildren<ReactionTester>())
			Target.GetComponent<Renderer> ().material.color = C;
	}
}

//Average reacton time in frames 		= Total / Hits
//Minimum reaction time in frames 		= 6
//Assumed max reaction time in frames 	= 30

//((Total/Hits) - 25) / 5)

//x = (x - 15) / (40 - 15)
//15 -> 0    40 -> 1
