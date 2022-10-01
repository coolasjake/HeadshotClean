using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Shootable : MonoBehaviour {


    [Header("Shootable")]
    protected float _health = 1;
    [SerializeField]
	protected float maxHealth = 1;

    private void Start()
    {
        _health = maxHealth;
    }

    public virtual void Hit (float Damage) {
		_health -= Damage;
		if (_health <= 0)
			Destroy (gameObject);
	}

	public void Kill () {
		Hit (_health + 1);
	}
}
