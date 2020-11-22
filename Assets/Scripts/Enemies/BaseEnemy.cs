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
	Working
}

public class BaseEnemy : Shootable {

	private static float AwakenChance = 0.1f;
	private static float TurretChance = 0.4f;

	protected bool Died = false;
	protected int RaycastShootingMask;
	protected int RaycastLookingMask;

	protected AudioManager SFXPlayer;

	void Start () {
		//Laser will be stopped by:
		//Default, Player, Ground, Window.
		RaycastShootingMask = 1 << 0 | 1 << 8 | 1 << 10 | 1 << 11;
		//Sight will be stopped by:
		//Default, Player, Ground.
		RaycastLookingMask = 1 << 0 | 1 << 8 | 1 << 9 | 1 << 10 | 1 << 17;
		if (Random.value < AwakenChance && !GetComponent<WhiteRobot> ()) {
			Instantiate (Resources.Load<GameObject> ("Prefabs/Laser Enemy"), transform.position, new Quaternion ());
			Destroy (gameObject);
		} else if (Random.value < TurretChance && !GetComponent<WhiteRobot> ()) {
			Instantiate (Resources.Load<GameObject> ("Prefabs/Turret Enemy"), transform.position, new Quaternion ());
			Destroy (gameObject);
		}
		SFXPlayer = GetComponent<AudioManager> ();
	}

	public void Headshot() {
		AchievementTracker.GunKills += 1;
		AchievementTracker.EnemyDied ();
		Hit (3);
		//Die ();
	}

	public override void Hit(float Damage) {
		Health -= Damage;
		if (Health <= 0) {
			AchievementTracker.GunKills += 1;
			AchievementTracker.EnemyDied ();
			Die ();
		}
	}

	public virtual void Die() {
		if (!Died) {
			EnemyCounter.BasicEnemiesKilled += 1;
			if (GetComponent<WhiteRobot> ())
				EnemyCounter.WhiteRabbitFound = true;
			EnemyCounter.UpdateScoreboard ();
			Instantiate (Resources.Load<GameObject> ("Prefabs/DeadBody"), transform.position, transform.rotation);
		}
		Died = true;
		Destroy (gameObject);
	}
}
