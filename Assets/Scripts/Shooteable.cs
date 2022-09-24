using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Shootable : MonoBehaviour {


    [Header("Shootable")]
    public float Health = 3;
	public float MaxHealth = 3;

	public virtual void Hit (float Damage) {
		Health -= Damage;
		if (Health <= 0)
			Destroy (gameObject);
	}

	public void Kill () {
		Hit (Health + 1);
	}
}
