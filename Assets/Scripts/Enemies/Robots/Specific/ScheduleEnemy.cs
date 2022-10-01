using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScheduleEnemy : ShootingEnemy {

	public Schedule Boss;

	private bool Pausing = false;

	private Vector3 PatrolTarget = new Vector3 ();

	// Use this for initialization
	void Start () {
		ShootingEnemyInitialize ();

		Vector3 FirstPoint = Boss.GetFirstPoint (this);
		UpdateDestination (FirstPoint);
		transform.position = FirstPoint;
	}
	
	// Update is called once per frame
	void Update () {
		StateMachine ();
	}

	protected override void Working () {
		base.Working ();

		if (State == AIState.Working) {
			if (!Pausing && (Vector3.Distance (AgentComponent.destination, transform.position) < 1.5f)) {
				Pausing = true;
				DisableAgentMovement ();
				Boss.ReachedPoint ();
			}
		}
	}

	protected override void Searching () {
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
		} else if ((Vector3.Distance (transform.position, LastPlayerGroundedPosition) < 5 && Time.time > StartedSearching + MinSearchDuration) || Time.time > StartedSearching + MaxSearchDuration) {
			State = AIState.Working;
			UpdateDestination (PatrolTarget);
			DetectionProgress = DetectionDifficulty * 0.75f;
			FiringDelayTimer = StartChargingDelay;
			LookAround = false;
			Network.AlarmedBots -= 1;
		} else if ((Vector3.Distance (transform.position, LastPlayerGroundedPosition) < 5)) {
			StartLookingAround ();
		}
	}

	/// <summary> Used by the Schedule to give this enemy the next point in the patrol loop. </summary>
	public void GiveNextPoint (Vector3 Point) {
		ReEnableAgentMovement ();
		AgentComponent.destination = Point;
		Pausing = false;
	}
}
