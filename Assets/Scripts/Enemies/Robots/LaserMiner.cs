using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LaserMiner : MovingEnemy {

	/// <summary> Time after seeing the player until this enemy will shoot. </summary>
	public float StartChargingDelay = 3;
	/// <summary> Time that this enemy will charge for (red light -> firing). </summary>
	public float ChargeTime = 3;
	/// <summary> Minimum time inbetween firing. </summary>
	public float FireCooldown = 10;
	/// <summary> The duration of the laser. </summary>
	public float FireDuration = 2;
	/// <summary> How long the enemy waits when alarmed before notifying other enemies. </summary>
	public float AlarmDuration = 2;
	/// <summary> The minimum duration of the search. </summary>
	public float MinSearchDuration = 5;
	/// <summary> The maximum duration of the search. </summary>
	public float MaxSearchDuration = 30;
	/// <summary> Ammount of damage this enemy does per second of laser contact. </summary>
	public float DPS = 50;

	//private int RaycastMask;
	/// <summary> 'Counter' which stops the enemy from firing too often, but is not cheesed by temporary loss of LOS. </summary>
	protected float FiringDelayTimer;
	/// <summary> Time that this enemy started charging. </summary>
	protected float StartedCharging;
	/// <summary> Time that this enemy started firing. </summary>
	protected float StartedFiring;
	/// <summary> Time that this enemy stopped firing, which is used for the minumum delay between shots. </summary>
	protected float StoppedFiring;
	/// <summary> Time that this enemy started giving the alarm. </summary>
	protected float StartedAlarm;

	protected Quaternion StartHeadRotation;
	protected Quaternion StartPlayerDirection;
	protected Vector3 LastDecalPoint = new Vector3();
	protected FiringPoint FP;

	void Start () {
		LaserMinerStart ();
	}

	protected void LaserMinerStart () {
		SetLaserEnemyMasks ();

		LEInitialise ();

		State = AIState.Working;
		StartChargingDelay += (Random.value - 0.5f);
		FiringDelayTimer = StartChargingDelay;
		StoppedFiring = -FireCooldown;
		RestLocation = transform.position;
		AgentComponent = GetComponent<NavMeshAgent> ();
		LastRefresh = Random.value * RefreshRate;
		FP = GetComponentInChildren<FiringPoint> ();

		GameObject DI = Instantiate (Resources.Load<GameObject> ("Prefabs/DetectionIndicator"), FindObjectOfType<Canvas> ().transform);
		DI.GetComponent<DetectionIndicator> ().Target = transform;
	}

	protected void SetLaserEnemyMasks () {
		//Laser will be stopped by:
		//Default, Player, Ground, Window.
		RaycastShootingMask = 1 << 0 | 1 << 8 | 1 << 10 | 1 << 11;
		//Sight will be stopped by:
		//Default, Player, Ground, OpaqueGrating.1 << 0 | 1 << 8 | 1 << 9 | 1 << 10
		RaycastLookingMask = 1 << 0 | 1 << 8 | 1 << 9 | 1 << 10 | 1 << 17;
	}

	void Update () {
		StateMachine ();
	}

	//STATE MACHINE FUNCTIONS
	/// <summary> Calls functions to detect and look at the player, and to detect the players location, runs the state machine,
    /// and updates the desired path based on the players current location (with a delay).</summary>
	protected virtual void StateMachine () {

		DetectPlayer ();
		RotateHead ();

		if (PlayerVisibility > 0.5f || Time.time < LastSawPlayer + CanGuessPositionTime) {
			LastPlayerPosition = Movement.ThePlayer.transform.position;
			LastPlayerGroundedPosition = Movement.ThePlayer._AIFollowPoint;
		}

		if (State == AIState.Working)
			Working ();

		if (State == AIState.Alarmed)
			Alarmed ();

		if (State == AIState.Staring)
			Staring ();

		if (State == AIState.Searching)
			Searching ();

		if (State == AIState.Charging)
			Charging ();

		if (State == AIState.Firing)
			Firing ();


		//Need to refresh the destination with the players movements, and it's the same code for all 3.
		if (State == AIState.Alarmed || State == AIState.Searching || State == AIState.Staring) {
			if (Time.time > LastRefresh + RefreshRate) {
				if (!UpdateDestination (LastPlayerGroundedPosition))
					LastPlayerGroundedPosition = new Vector3(LastPlayerGroundedPosition.x, 0, LastPlayerGroundedPosition.z);
				LastRefresh = Time.time;
			}
		}
	}

	/// <summary> Checks if the player is visible when in working mode (slightly harder to be noticed than in other states). </summary>
	protected virtual void Working () {
		//If the player is 'obvious' or has been in sight for a long time, start giving the alarm.
		if (PlayerVisibility >= 1 || DetectionProgress >= 2) {
			//Change to 'go into staring mode with a smaller firing delay than normal'
			if (Time.time > Network.LastAlarm + Network.AlarmFrequency && Network.AlarmedBots < 3) {
				State = AIState.Alarmed;
				StartedAlarm = Time.time;
			} else {
				State = AIState.Staring;
				FiringDelayTimer = StartChargingDelay / 2;
			}
			Network.AlarmedBots += 1;
		} else if (DetectionProgress > 1 && PlayerVisibility != 0) {
			//If the player has been at the edge of the bots vision for a while, go to 'searching' mode.
			State = AIState.Searching;
			StartedSearching = Time.time;
			Network.AlarmedBots += 1;
		}
	}

	/// <summary> Warns nearby bots that the player is near, at the cost of a delayed reaction. Occurs when player is obvious and an alarm hasn't been raised for a while. </summary>
	protected virtual void Alarmed () {
		if (Time.time > StartedAlarm + AlarmDuration) {
			//Find nearby enemies, put them in searching mode and give them the players location.
			foreach (LookingEnemy Bot in FindObjectsOfType<LookingEnemy>()) {
				if (Vector3.Distance (Bot.transform.position, transform.position) < AlarmRange)
					Bot.SoundAlarm (LastPlayerPosition, LastPlayerGroundedPosition);
			}
			State = AIState.Staring;
			FiringDelayTimer = StartChargingDelay;
		} else {
			//Update the 'giving alarm' indicator (light?).

			if (PlayerVisibility == 0)
				StartLookingAround ();
			else
				LookAround = false;
		}
	}

	/// <summary> The AI 'knows' the player is nearby, but cannot see them, and will look around while moving to the last known location (if possible). Attack delay is not counted here, but not reset either. </summary>
	protected virtual void Searching () {
		if (PlayerVisibility == 0)
			StartLookingAround ();
		else
			LookAround = false;

		if (PlayerVisibility >= 1 || DetectionProgress >= 2) {
			if (Time.time > Network.LastAlarm + Network.AlarmFrequency && Network.AlarmedBots < 3) {
				State = AIState.Alarmed;
				StartedAlarm = Time.time;
			} else {
				State = AIState.Staring;
				FiringDelayTimer = StartChargingDelay;
			}
			FP.AlertLight.enabled = true;
			LookAround = false;
		} else if ((Vector3.Distance (transform.position, LastPlayerGroundedPosition) < 5 && Time.time > StartedSearching + MinSearchDuration) || Time.time > StartedSearching + MaxSearchDuration) {
			State = AIState.Working;
			UpdateDestination (RestLocation);
			DetectionProgress = 1.5f;
			FiringDelayTimer = StartChargingDelay;
			LookAround = false;
			Network.AlarmedBots -= 1;
		} else if ((Vector3.Distance (transform.position, LastPlayerGroundedPosition) < 5)) {
			StartLookingAround ();
		}
	}

	/// <summary> The state where the AI can see the player, and is either moving towards them or waiting for the attack delay. </summary>
	protected virtual void Staring () {
		if (PlayerVisibility == 0) {
			State = AIState.Searching;
			FP.AlertLight.enabled = false;
			StartedSearching = Time.time;
			DetectionProgress = 1.5f;
		} else {
			FiringDelayTimer -= Time.deltaTime;
			if (FiringDelayTimer <= 0 && Time.time > StoppedFiring + FireCooldown) {
				State = AIState.Charging;
				StartedCharging = Time.time;
				FP.DangerLight.enabled = true;
				Freeze = true;
				DisableAgentMovement ();
				SFXPlayer.PlaySound ("Charge", 1, 15, 30, 1, 1, false);
			}
		}
	}

	/// <summary> The AI is 'powering up' its attack; this freezes it (including head movement), and causes lights and sounds to appear. This is NOT cancelled by losing LOS. Note: designed for the Laser Enemies, may not work well for others. </summary>
	protected virtual void Charging () {
		//If still charging:
		if (Time.time < StartedCharging + ChargeTime) {
			//Debug.Log ("Increasing Intensity");
			FP.DangerLight.intensity = 0.5f + (Time.time - StartedCharging) / ChargeTime;
		} else {
			//Debug.Log ("Started Firing");
			State = AIState.Firing;
			StartedFiring = Time.time;
			FP.DangerLight.intensity = 1.5f;
			FP.EffectLine.enabled = true;
			//FP.ParticleEffect.Play ();
			StartHeadRotation = Head.transform.rotation;
			StartPlayerDirection = Quaternion.LookRotation (Movement.ThePlayer.transform.position - Head.transform.position);
			if (PlayerVisibility == 0)
				StartPlayerDirection = Quaternion.LookRotation (Head.transform.right * (Random.value - 0.5f) - Head.transform.up * 0.1f);
			SFXPlayer.PlaySound ("Fire", 1, 15, 30, 1, 1, false);
		}
	}

	/// <summary> The AI fires the charged attack, then goes into Staring mode after resetting cooldowns.
	/// For laser enemies this means 'drawing a line' between the players position at the start and end of charging, and then continuing for 300% of 
	/// the distance, or 100% of the time (rotation speed increases linearly, and the players saved position is reached after half the drawing time).
	/// </summary>
	protected virtual void Firing () {
		if (Time.time < StartedFiring + FireDuration) {
			//Rotate by: Time since this started firing, divided by half of the fire duration.
			float RotationFactor = (Time.time - StartedFiring) / (FireDuration * 0.5f);
			Head.transform.rotation = Quaternion.LerpUnclamped (StartHeadRotation, StartPlayerDirection, RotationFactor);

			RaycastHit Hit;
			if (Physics.Raycast (FP.transform.position - FP.transform.up, -FP.transform.up, out Hit, 600, RaycastShootingMask)) {
				Vector3 LinePoint = FP.EffectLine.transform.InverseTransformPoint (Hit.point);
				FP.EffectLine.SetPosition (1, LinePoint);

				Shootable SH = Hit.collider.GetComponentInParent<Shootable> ();
				if (SH)
					SH.Hit(DPS * Time.deltaTime);
				else if (Time.timeScale > 0)
					SpawnBurnDecal (Hit);
			}
		} else {
			//Stop Firing
			State = AIState.Staring;
			StoppedFiring = Time.time;
			FP.DangerLight.enabled = false;
			FP.EffectLine.enabled = false;
			Freeze = false;
			ReEnableAgentMovement ();
		}
	}

	protected void SpawnBurnDecal (RaycastHit Hit) {
		Quaternion DecalRotation = Quaternion.LookRotation (Hit.normal);
		GameObject LaserBurn;
		LastDecalPoint = Hit.point + (Hit.normal * 0.001f);
		LaserBurn = Instantiate (Resources.Load<GameObject> ("Prefabs/LaserBurn"), LastDecalPoint, DecalRotation);
	}

}
