using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LookingEnemy : BaseEnemy {

    [Header("Looking Enemy")]
	public AIState State = AIState.Working;

	//protected float LastRefresh = 0;
	public Transform Head;
	protected float LastSawPlayer = 0;
	/// <summary> The players last position for the bot to look at (not move to, as it will often be high in the air). </summary>
	protected Vector3 LastPlayerPosition;
	/// <summary> The players last position for the bot to move to. </summary>
	protected Vector3 LastPlayerGroundedPosition;
	protected float StartedSeeingPlayer = 0;
	/// <summary> Used to stop bots from searching after not being able to reach the player for a period of time </summary>
	protected float StartedSearching;
	protected bool HaveLOSToPlayer = false;
	/// <summary> Used by the RotateHead () function to know if the AI should 'look around' or not. Mostly set by child AI scripts. </summary>
	protected bool LookAround = false;
	protected float LookAroundStarted = 0;
	private Vector3 LookAroundDirection = new Vector3();
	/// <summary> Rate at which player will be detected (from 0 - 1). 0 = Cannot see player, 1 = Player is 'obvious'. </summary>
	protected float PlayerVisibility = 0;

	/// <summary> CURRENTLY NOT IN USE! </summary>
	private static float ForgetTime = 10;
	/// <summary> How quickly the AI will lose 'suspision' (DetectionProgress), relative to the normal gain (default = 1/s). </summary>
	public static float LooseIntrestMagnitude = 0.1f;
	/// <summary> The maximum angle in degrees on either side the AIs facing (so 45deg is a 90deg cone in total), from which it can detect the player. </summary>
	public static float DetectionAngle = 45;
	/// <summary> The angle (in any direction) where the AI will begin to detect the player (and perform actions like turning its head) if it is already suspicious (DetectionProgress > 0.5f). </summary> </summary>
	public static float AlertDetectionAngle = 120;
	/// <summary> The angle (in any direction) where the player is 'obvious', resulting in very rapid detection (usually milliseconds, but varies based on distance). </summary>
	public static float ObviousAngle = 10;
	/// <summary> CURRENTLY NOT IN USE! </summary>
	private static float MinimumDetectionTime = 1;
	/// <summary> The enemy will not detect the player at all from this distance, unless they are within the obvious angle. Used when factoring distance into visibility. </summary>
	public static float MaxDetectionDistance = 300;
	/// <summary> The distance from which the AI will begin to detect the player, even if they are directly behind them. (Stops them from looking stupid when you walk behind them) </summary>
	public static float AutoDetectionDistance = 5;
	/// <summary> Until the player has been out of sight for this ammount of time, the AI will still be able to access their location, creating the illusion that it can guess their most likely position. </summary>
	public static float CanGuessPositionTime = 0.3f;
	[System.NonSerialized]
    /// <summary> How close the enemy is to detecting the player - increased by being in the enemys view cone, speed based on distance. </summary>
	public float DetectionProgress = 0;
    /// <summary> How easy it is for this bot to detect the player. The value will roughly translate to time, but detection is dependant on angles, LOS and distance. </summary>
	public float DetectionDifficulty = 2f;
    /// <summary> Range from which a Bot-to-Bot alarm will alert other AI (Radius). </summary>
    public static float AlarmRange = 10;
	/// <summary> Freezes the AI's head (i.e. when they are charging an attack). </summary>
	protected bool Freeze = false;

	void Start () {
		LEInitialise ();
	}

	protected void LEInitialise () {
		LastSawPlayer = -ForgetTime;
        if (Head == null)
		    Head = GetComponentInChildren<Head> ().transform;
		SFXPlayer = GetComponent<AudioManager> ();
	}

	protected void DetectPlayer () {

		bool DetectingPlayer = false;
		PlayerVisibility = 0;
		HaveLOSToPlayer = false;

		//If a raycast from head to center, or head to head hits the player, set the time the player was last seen to now, and indicate that the player can be seen currently.
		RaycastHit Hit1;
		RaycastHit Hit2;
		Movement P1;
		Movement P2;
		Movement P = null;
		bool NoHit = true; //Starts here, should be a local variable.
		if (Physics.Raycast (Head.transform.position, (Movement.ThePlayer.transform.position - transform.position), out Hit1, 600, raycastLookingMask)) {
			P1 = Hit1.transform.GetComponentInParent<Movement> ();
			if (P1) {
				NoHit = false;
				P = P1;
			}
		}
		if (Physics.Raycast (Head.transform.position, (Movement.ThePlayer.MainCamera.transform.position - Head.transform.position), out Hit2, 600, raycastLookingMask)) {
			P2 = Hit2.transform.GetComponentInParent<Movement> ();
			if (P2) {
				NoHit = false;
				P = P2;
			}
		}
		
		if (!Movement.ThePlayer._Invisible) {
			if (!NoHit) {
				HaveLOSToPlayer = true;
				float AngleToPlayer = Vector3.Angle (Head.transform.forward, Movement.ThePlayer.transform.position - Head.transform.position);
				if (AngleToPlayer < ObviousAngle) {
                    //If the player is within the 'Obvious' cone, set player visibility relative to distance plus 0.5f (PlayerDetection of 1 equals instant detection)
					DetectingPlayer = true;
					PlayerVisibility = 0.5f + (1 - (Vector3.Distance (Movement.ThePlayer.transform.position, transform.position) / MaxDetectionDistance)) / 2;
					//PlayerVisibility = 1;
				} else if (AngleToPlayer < DetectionAngle || (DetectionProgress > DetectionDifficulty * 0.25f && AngleToPlayer < AlertDetectionAngle)) {
                    //If the player is within the detection cone, or the AI is 'suspicious' (Progress > 0.5) and they are within the Alert cone
					if (Vector3.Distance (Movement.ThePlayer.transform.position, transform.position) > MaxDetectionDistance)
						PlayerVisibility = 0;
					else {
						//The player is within the bots 'vision'.
						DetectingPlayer = true;
						PlayerVisibility = (1 - (Vector3.Distance (Movement.ThePlayer.transform.position, transform.position) / MaxDetectionDistance)) / 2;
						PlayerVisibility += ((AngleToPlayer - ObviousAngle) / (DetectionAngle - ObviousAngle)) / 2;
					}
				}
			}
		}

		if (PlayerVisibility == 1) {
			LastSawPlayer = Time.time;
			DetectingPlayer = true;
		} else if (PlayerVisibility == 0 && !NoHit && Vector3.Distance (Movement.ThePlayer.transform.position, transform.position) < AutoDetectionDistance) {
			DetectingPlayer = true;
			PlayerVisibility = 0.2f;
			LastPlayerPosition = Movement.ThePlayer._AIFollowPoint + new Vector3 (0, Head.transform.position.y - 0.5f, 0); //Make the AI look at eye level rather than the ground.
			LastPlayerGroundedPosition = Movement.ThePlayer._AIFollowPoint;
		}

		if (DetectionProgress < DetectionDifficulty && DetectingPlayer)
			DetectionProgress += PlayerVisibility * Time.deltaTime;
		else if (DetectionProgress > 0 && !DetectingPlayer && PlayerVisibility == 0 && (State == AIState.Working || State == AIState.Searching))
			DetectionProgress -= Time.deltaTime * LooseIntrestMagnitude;
	}

	protected virtual void RotateHead () {
		if (Freeze)
			return;
		if (PlayerVisibility > 0.5f ) {
			//Turn to face the player (lerp relative to PV).
			Head.transform.rotation = Quaternion.RotateTowards (Head.transform.rotation, Quaternion.LookRotation (Movement.ThePlayer.transform.position - Head.transform.position), (90 + (90 * PlayerVisibility)) * Time.deltaTime);
		} else if (State == AIState.Searching || State == AIState.Alarmed) {
			if (LookAround) {
				if (Time.time > LookAroundStarted + 0.8f) {
					LookAroundStarted = Time.time;
					LookAroundDirection = transform.forward;
					LookAroundDirection += transform.right * (Random.value - 0.5f) * 5;
					LookAroundDirection += transform.up * (Random.value - 0.5f) * 3;
				}
				Head.transform.rotation = Quaternion.RotateTowards (Head.transform.rotation, Quaternion.LookRotation (LookAroundDirection), (90 * Time.deltaTime));
				//Debug.Log ("Time Since Look Around Started" + (Time.time - LookAroundStarted));
			} else
			//Turn to face the players assumed position.
				Head.transform.rotation = Quaternion.RotateTowards (Head.transform.rotation, Quaternion.LookRotation (LastPlayerPosition - Head.transform.position), (90 * Time.deltaTime));
		}  else {
			//Turn to face forwards.
			Head.transform.rotation = Quaternion.RotateTowards (Head.transform.rotation, Quaternion.LookRotation (transform.forward), (90 * Time.deltaTime));
		}
	}

	public void StartLookingAround () {
		if (LookAround == false) {
			LookAroundStarted = Time.time - 0.8f;
			LookAround = true;
		}
	}

	public void Alert () {
		SeePlayer ();
		DetectionProgress = DetectionDifficulty * 1.1f;
		StartedSearching = Time.time;
	}

	public void DetectBody (DeadBody Body) {
		RaycastHit Hit;
		bool CanSee = false;
		if (Physics.Raycast (transform.position, (Body.transform.position - transform.position), out Hit)) {
			if (Hit.transform.GetComponentInParent<DeadBody> () == Body) {
				if (Vector3.Angle (Head.transform.forward, Body.transform.position - Head.transform.position) < AlertDetectionAngle)
					CanSee = true;
				else if (Vector3.Distance (Body.transform.position, transform.position) < AutoDetectionDistance)
					CanSee = true;
			}
		}
		if (CanSee) {
			DetectionProgress = DetectionDifficulty;
			PlayerVisibility = 0.4f;
			SeePlayer ();
		}
	}

	public override void Die() {
		if (!died) {
			EnemyCounter.BasicEnemiesKilled += 1;
			EnemyCounter.UpdateScoreboard ();
			Network.AlarmedBots -= 1;
			Instantiate (Resources.Load<GameObject> ("Prefabs/Enemies/DeadBody"), transform.position, transform.rotation);
		}
		died = true;
		Destroy (gameObject);
	}

	public void SoundAlarm (Vector3 PlayerPosition, Vector3 PlayerGroundedPosition) {
		State = AIState.Searching;
		SeePlayer ();
		StartedSearching = Time.time;
		Network.AlarmedBots += 1;
	}

	private void SeePlayer () {
		LastPlayerPosition = Movement.ThePlayer.transform.position;
		LastPlayerGroundedPosition = Movement.ThePlayer._AIFollowPoint;
	}
}
