using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LaserTurret : ShootingEnemy {

	/*
	//public Movement Target;
	public float StartChargingDelay = 5;
	public float ChargeTime = 3;
	public float FireCooldown = 10;
	public float FireDuration = 2;
	public float DPS = 50;

	//private bool CanSeePlayer = false;
	//private float StartedSeeingPlayer;
	private float StartedCharging;
	private float StartedFiring;
	private float StoppedFiring;
	//private AIState State = AIState.Working;
	private Quaternion StartHeadRotation;
	private Quaternion StartPlayerDirection;
	private Vector3 LastDecalPoint = new Vector3();
	private FiringPoint FP;
	//protected Transform Head;



	public float AlarmDuration = 2;
	public float SearchDuration = 5;

	//private int RaycastMask;
	/// <summary>
	/// 'Counter' which stops the enemy from firing too often, but is not cheesed by temporary loss of LOS.
	/// </summary>
	private float FiringDelayTimer;
	/// <summary>
	/// Time that this enemy started giving the alarm.
	/// </summary>
	private float StartedAlarm;

	*/

	void Start () {
		SetLaserEnemyMasks ();

		LEInitialise ();

		State = AIState.Working;
		StartChargingDelay += (Random.value - 0.5f);
		FiringDelayTimer = StartChargingDelay;
		StoppedFiring = -FireCooldown;
		RestLocation = transform.position;
		LastRefresh = Random.value * RefreshRate;
		FP = GetComponentInChildren<FiringPoint> ();

		GameObject DI = Instantiate (Resources.Load<GameObject> ("Prefabs/Enemies/DetectionIndicator"), FindObjectOfType<Canvas> ().transform);
		DI.GetComponent<DetectionIndicator> ().Target = transform;
	}
		
	void Update () {
		StateMachine ();
	}
		
	/// <summary> This is the TURRET version, meaning it doesn't interact with the NavAgent. </summary>
	protected virtual void StateMachine () {

		DetectPlayer ();
		RotateHead ();

		if (PlayerVisibility > 0.5f || Time.time < LastSawPlayer + CanGuessPositionTime) {
			LastPlayerPosition = PlayerMovement.ThePlayer.transform.position;
			LastPlayerGroundedPosition = PlayerMovement.ThePlayer._AIFollowPoint;
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
	}

	/// <summary> This is the TURRET version, meaning it doesn't interact with the NavAgent. </summary>
	protected virtual void Searching () {
		if (PlayerVisibility == 0)
			StartLookingAround ();
		else
			LookAround = false;

		if (PlayerVisibility >= 1 || DetectionProgress >= DetectionDifficulty) {
			if (Time.time > Network.LastAlarm + Network.AlarmFrequency && Network.AlarmedBots < 3) {
				State = AIState.Alarmed;
				StartedAlarm = Time.time;
			} else {
				State = AIState.Staring;
				FiringDelayTimer = StartChargingDelay;
			}
			FP.AlertLight.enabled = true;
			LookAround = false;
		} else if (Time.time > StartedSearching + MinSearchDuration) {
			State = AIState.Working;
			FiringDelayTimer = StartChargingDelay;
			LookAround = false;
			Network.AlarmedBots -= 1;
		} else
			StartLookingAround ();
	}

	/// <summary> This is the TURRET version, meaning it doesn't interact with the NavAgent. </summary>
	protected virtual void Staring () {
		if (PlayerVisibility == 0) {
			State = AIState.Searching;
			FP.AlertLight.enabled = false;
			StartedSearching = Time.time;
			DetectionProgress = DetectionDifficulty * 0.75f;
		} else {
			FiringDelayTimer -= Time.deltaTime;
			if (FiringDelayTimer <= 0 && Time.time > StoppedFiring + FireCooldown) {
				State = AIState.Charging;
				StartedCharging = Time.time;
				FP.DangerLight.enabled = true;
				Freeze = true;
				SFXPlayer.PlaySound ("Charge", 1, 5, 30, 1, 1, false);
			}
		}
	}


	/// <summary> This is the TURRET version, meaning it doesn't interact with the NavAgent. </summary>
	protected virtual void Firing () {
		if (Time.time < StartedFiring + FireDuration) {
			//Rotate by: Time since this started firing, divided by half of the fire duration.
			float RotationFactor = (Time.time - StartedFiring) / (FireDuration * 0.5f);
			Head.transform.rotation = Quaternion.LerpUnclamped (StartHeadRotation, StartPlayerDirection, RotationFactor);

			RaycastHit Hit;
			if (Physics.Raycast (FP.transform.position - FP.transform.up, -FP.transform.up, out Hit, 600, raycastShootingMask)) {
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
		}
	}
















	/*

	void Update () {

		DetectPlayer ();
		RotateHead ();

		if (PlayerVisibility > 0.5f || Time.time < LastSawPlayer + CanGuessPositionTime) {
			LastPlayerPosition = Movement.ThePlayer.transform.position;
			LastPlayerGroundedPosition = Movement.ThePlayer.AIFollowPoint;
		}

		


		if (State == AIState.Searching) {
		}

		if (State == AIState.Staring) {
		}

		if (State == AIState.Charging) {
		}

		if (State == AIState.Firing) {
			if (Time.time < StartedFiring + FireDuration) {
				float RotationFactor = (Time.time - StartedFiring) / (FireDuration * 0.5f);
				Head.transform.rotation = Quaternion.LerpUnclamped (StartHeadRotation, StartPlayerDirection, RotationFactor);

				RaycastHit Hit;
				if (Physics.Raycast (FP.transform.position - FP.transform.up, -FP.transform.up, out Hit, 600, RaycastShootingMask)) {
					//Line.transform.localPosition = new Vector3 (0, -2, 0);
					Vector3 LinePoint = FP.EffectLine.transform.InverseTransformPoint (Hit.point);
					FP.EffectLine.SetPosition (1, LinePoint);//new Vector3(0, -Vector3.Distance(FP.transform.position, Hit.point), 0));
					//FP.ParticleEffect.transform.localScale = new Vector3(0, Vector3.Distance(FP.transform.position, Hit.point), 0);

					Shooteable SH = Hit.collider.GetComponentInParent<Shooteable> ();
					if (SH)
						SH.Hit (DPS * Time.deltaTime);
					else if (Time.timeScale > 0)
						SpawnBurnDecal (Hit);
				}
			} else {
				//Debug.Log ("Stopped Firing");
				State = AIState.Staring;
				StoppedFiring = Time.time;
				Freeze = false;
				FP.DangerLight.enabled = false;
				FP.EffectLine.enabled = false;
			}
		}
	}

		/*
		
		DetectPlayer ();
		RotateHead ();

		if (PlayerVisibility == 1 || Time.time < LastSawPlayer + CanGuessPositionTime) {
			LastPlayerPosition = PlayerTarget.transform.position;
			//LastPlayerGroundedPosition = PlayerTarget.FollowPoint;
		}

		if (State == AIState.Working) {
			//If the player is 'obvious' or has been in sight for a long time, start giving the alarm.
			if (PlayerVisibility >= 1 || DetectionProgress >= 2) {
				State = AIState.Alarmed;
				StartedAlarm = Time.time;
			} else if (DetectionProgress > 1 && PlayerVisibility != 0) {
				//If the player has been at the edge of the bots vision for a while, go to 'searching' mode.
				State = AIState.Searching;
				StartedSearching = Time.time;
				//UpdateDestination (LastPlayerGroundedPosition);
			}
		}

		if (State == AIState.Alarmed) {
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
			}
		}

		if (State == AIState.Searching) {
			TryToLookAtPlayer = true;
			//If the bot is a negligible distance from the last player position, go back to work
			if (PlayerVisibility >= 1 || DetectionProgress >= 2) {
				State = AIState.Staring;
				FiringDelayTimer = StartChargingDelay;
			}

			if (Time.time > StartedSearching + SearchDuration) {
				State = AIState.Working;
				FiringDelayTimer = StartChargingDelay;
				TryToLookAtPlayer = false;
			}
		}

		if (State == AIState.Staring) {
			if (PlayerVisibility == 0) {
				State = AIState.Searching;
				StartedSearching = Time.time;
			} else {
				FiringDelayTimer -= Time.deltaTime;
				if (FiringDelayTimer <= 0 && Time.time > StoppedFiring + FireCooldown) {
					State = AIState.Charging;
					StartedCharging = Time.time;
					FP.EffectLight.enabled = true;
					//Freeze = true;
					//DisableAgentMovement ();
				}
			}
		}

		if (State == AIState.Charging) {
			//If still charging:
			if (Time.time < StartedCharging + ChargeTime) {
				//Debug.Log ("Increasing Intensity");
				FP.EffectLight.intensity = 0.5f + (Time.time - StartedCharging) / ChargeTime;
			} else {
				//Debug.Log ("Started Firing");
				State = AIState.Firing;
				StartedFiring = Time.time;
				FP.EffectLight.intensity = 1.5f;
				FP.EffectLine.enabled = true;
				//FP.ParticleEffect.Play ();
				StartHeadRotation = Head.transform.rotation;
				StartPlayerDirection = Quaternion.LookRotation (PlayerTarget.transform.position - Head.transform.position);
				GetComponent<AudioSource> ().Play ();
			}
		}

		if (State == AIState.Firing) {
			if (Time.time < StartedFiring + FireDuration) {
				float RotationFactor = (Time.time - StartedFiring) / (FireDuration * 0.5f);
				Head.transform.rotation = Quaternion.LerpUnclamped (StartHeadRotation, StartPlayerDirection, RotationFactor);

				RaycastHit Hit;
				if (Physics.Raycast (FP.transform.position - FP.transform.up, -FP.transform.up, out Hit, 600, RaycastMask)) {
					//Line.transform.localPosition = new Vector3 (0, -2, 0);
					Vector3 LinePoint = FP.EffectLine.transform.InverseTransformPoint (Hit.point);
					FP.EffectLine.SetPosition (1, LinePoint);//new Vector3(0, -Vector3.Distance(FP.transform.position, Hit.point), 0));
					//FP.ParticleEffect.transform.localScale = new Vector3(0, Vector3.Distance(FP.transform.position, Hit.point), 0);

					Shooteable SH = Hit.collider.GetComponentInParent<Shooteable> ();
					if (SH)
						SH.Hit(DPS * Time.deltaTime);
					else if (Time.timeScale > 0)
						SpawnBurnDecal (Hit);
				}
			} else {
				//Debug.Log ("Stopped Firing");
				State = AIState.Staring;
				StoppedFiring = Time.time;
				FP.EffectLight.enabled = false;
				FP.EffectLine.enabled = false;
				//Freeze = false;
				//ReEnableAgentMovement ();
			}
		}


	}
	*/

	private void SpawnBurnDecal (RaycastHit Hit) {
		Quaternion DecalRotation = Quaternion.LookRotation (Hit.normal);
		GameObject LaserBurn;
		LastDecalPoint = Hit.point + (Hit.normal * 0.001f);
		LaserBurn = Instantiate (Resources.Load<GameObject> ("Prefabs/LaserBurn"), LastDecalPoint, DecalRotation);
	}

}
