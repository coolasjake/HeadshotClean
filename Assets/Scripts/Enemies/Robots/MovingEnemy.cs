using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public abstract class MovingEnemy : LookingEnemy {

	//public Movement Target;
	private bool Disabled = false;

	protected NavMeshAgent AgentComponent;
	protected float LastRefresh = 0;
	//protected Transform Head;
	//protected float LastSawPlayer = 0;
	protected Vector3 RestLocation;
	//protected float StartedSeeingPlayer = 0;
	//protected bool CanSeePlayer = false;

	protected bool CreepyMode = false;
	[Range(0, 1)]
	public static float CreepyChance = 0.15f;
	public static bool PlaySounds = false;
	public static float WalkingHereChance = 0.1f;
	//public static float ForgetTime = 10;
	/// <summary>
	/// The refresh rate for pathfinding.
	/// </summary>
	public static float RefreshRate = 1f;


	// Use this for initialization
	void Start () {
		LEInitialise ();
		LastRefresh = Random.value * RefreshRate;
		RestLocation = transform.position;
		AgentComponent = GetComponent<NavMeshAgent> ();
		if (Random.value < WalkingHereChance)
			GetComponent<AudioSource> ().clip = Resources.Load<AudioClip> ("Sounds/WalkingHere");
		if (Random.value < CreepyChance) {
			CreepyMode = true;
			GetComponent<AudioSource> ().clip = Resources.Load<AudioClip> ("Sounds/Helooo");
		}
	}
	
	// Update is called once per frame
	void Update () {
		MovingEnemyUpdate ();
	}

	public void MovingEnemyUpdate () {
		
		DetectPlayer ();

		RotateHead ();

		//Skip all checks and motion (usually so another script can control them exclusively).
		if (Freeze)
			return;

		/*
		if (PlayerVisibility > 0.5f ) {//|| DetectionProgress > 1) { //TryToLookAtPlayer) {
			//Turn to face the player (lerp relative to PV).
			Head.transform.rotation = Quaternion.RotateTowards (Head.transform.rotation, Quaternion.LookRotation (PlayerTarget.transform.position - Head.transform.position), (90 + (90 * PlayerVisibility)) * Time.deltaTime);
		} else if (State == AIState.Searching || State == AIState.Alarmed) {
			//Turn to face the players assumed position.
			Head.transform.rotation = Quaternion.RotateTowards (Head.transform.rotation, Quaternion.LookRotation (LastPlayerPosition - Head.transform.position), (90 * Time.deltaTime));
		}  else {
			//Turn to face forwards.
			Head.transform.rotation = Quaternion.RotateTowards (Head.transform.rotation, Quaternion.LookRotation (transform.forward), (90 * Time.deltaTime));
		}

		//If the refresh delay is finished (to reduce resources used):
		if (Time.time > LastRefresh + RefreshRate) {
			//If the maximum follow time has expired, go back to the bots starting position, make CanSeePlayer false, and reset the head.
			//Creepy mode makes the bot do the opposite in this section to the non-creepy bot.
			if (Time.time > LastSawPlayer + ForgetTime ^ CreepyMode) {
				if (!Disabled)
					AgentComponent.SetDestination (RestLocation);
				if (!CreepyMode)
					Head.rotation = new Quaternion ();
			} else {
				//If the bot hasn't forgotten yet, go to the players follow point (directly beneath it). [CHANGE TO 'LAST SIGHTING POINT']
				LastRefresh = Time.time;
				if (!Disabled)
					AgentComponent.SetDestination (LastPlayerPosition);
				//CanSeePlayer = true;
			}
		}
		*/

	}

	public bool UpdateDestination (Vector3 Destination) {
		if (!Disabled)
			AgentComponent.SetDestination (Destination);
		return AgentComponent.pathStatus == NavMeshPathStatus.PathComplete;
	}

	public void DisableAgentMovement () {
		AgentComponent.enabled = false;
		Disabled = true;
	}

	public void ReEnableAgentMovement () {
		AgentComponent.enabled = true;
		Disabled = false;
	}

	public override void Die() {
		if (CreepyMode)
			EnemyCounter.CuriousEnemiesKilled += 1;
		EnemyCounter.FollowingEnemiesKilled += 1;
		base.Die ();
	}

	void OnCollisionEnter (Collision Col) {
		if (Col.transform.GetComponent<Movement> ()) {
			EnemyCounter.HitsTaken += 1;
			EnemyCounter.UpdateScoreboard ();
			if (PlaySounds)
				GetComponent<AudioSource> ().Play ();
		}
	}
}
