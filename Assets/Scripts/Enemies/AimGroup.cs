using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimGroup : Group {

	public int Hits = 0;

	public override void CallCommand (ButtonCommands Command)
	{
		//Do Nothing.
	}

	public void BlockHit(bool Correct) {
		int i = Mathf.FloorToInt(Random.value * GetComponentsInChildren<AimTester> ().Length);
		GetComponentsInChildren<AimTester> () [i].HitMe = true;

		if (Correct)
			Hits += 1;
		else {
			Hits = 0;
			foreach (AimTester AT in GetComponentsInChildren<AimTester> ()) {
				AT.HitMe = true;
			}
		}
	}
}
