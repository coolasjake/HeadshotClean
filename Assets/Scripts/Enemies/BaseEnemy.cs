using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AIState {
	Alarmed,
	Staring,
	Charging,
	Firing,
	Searching,
	Working,
    Special
}

public class BaseEnemy : Shootable {
	protected bool died = false;
    [Header("Base")]
    [SerializeField]
	protected LayerMask raycastShootingMask;
    [SerializeField]
    protected LayerMask raycastLookingMask;

	protected AudioManager SFXPlayer;

	void Start () {
		SFXPlayer = GetComponent<AudioManager> ();
	}

	public void Headshot() {
		Hit (3);
		//Die ();
	}

	public override void Hit(float Damage) {
		_health -= Damage;
		if (_health <= 0) {
			Die ();
		}
	}

	public virtual void Die() {
		if (!died) {
			EnemyCounter.BasicEnemiesKilled += 1;
			EnemyCounter.UpdateScoreboard ();
			Instantiate (Resources.Load<GameObject> ("Prefabs/Enemies/DeadBody"), transform.position, transform.rotation);
		}
		died = true;
		Destroy (gameObject);
	}
}
