using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteGravity : MonoBehaviour {

	public Vector3 DirectionalGravity;
	private Rigidbody RB;
	private bool ReachedTerminalVelocity = false;
	private float StartTime = 0;

	private static float TerminalVelocity = 30;
	private static float MaxDuration = 10;

	// Use this for initialization
	void Start () {
		StartTime = Time.time;
		RB = GetComponent<Rigidbody> ();
		if (!RB)
			RB = gameObject.AddComponent<Rigidbody> ();
		RB.useGravity = false;
		RB.isKinematic = false;
		RB.collisionDetectionMode = CollisionDetectionMode.Continuous;
		if (GetComponent<MovingEnemy> ())
			GetComponent<MovingEnemy> ().DisableAgentMovement ();
		if (GetComponent<PathFollower> ())
			GetComponent<PathFollower> ().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > StartTime + MaxDuration) {
			RB.isKinematic = true;
			if (GetComponent<MovingEnemy> ())
				GetComponent<MovingEnemy> ().ReEnableAgentMovement ();
			if (GetComponent<PathFollower> ())
				GetComponent<PathFollower> ().enabled = true;
			Destroy (this);
		}

		RB.velocity += DirectionalGravity * Time.deltaTime;

		if (!ReachedTerminalVelocity)
			ReachedTerminalVelocity = RB.velocity.magnitude > TerminalVelocity;
	}

	void OnCollisionEnter (Collision col) {
		if (ReachedTerminalVelocity) {
			BaseEnemy Enemy = col.gameObject.GetComponentInParent<BaseEnemy> ();
			if (Enemy) {
				AchievementTracker.DoubleAntiGravityKills += 1;
				AchievementTracker.EnemyDied ();
				Enemy.Die ();
			}
			AchievementTracker.AntiGravityKills += 1;
			AchievementTracker.EnemyDied ();
			GetComponent<BaseEnemy> ().Die ();
			ReachedTerminalVelocity = false;
			Destroy (this);
		}
	}
}
