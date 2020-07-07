using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAbility : MonoBehaviour {
	protected bool Disabled = false;

	public float Resource = 0;
	public float MaxResource = 3;
	public float RegenRate = 0.5f;
	public float MinToUse = 1;

    public virtual void Disable()
    {
        Disabled = true;
    }

    public virtual void Enable()
    {
        Disabled = true;
    }
}
