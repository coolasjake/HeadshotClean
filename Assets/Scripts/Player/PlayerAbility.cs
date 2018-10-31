using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAbility : MonoBehaviour {
	public bool Disabled = false;

	public float Resource = 0;
	public float MaxResource = 3;
	public float RegenRate = 0.5f;
	public float MinToUse = 1;
}
