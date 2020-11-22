using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : Shootable {

	public override void Hit(float Damage) {
		GetComponentInParent<BaseEnemy> ().Headshot();
	}
}
