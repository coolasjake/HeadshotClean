using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemy : LaserMiner {

	public bool PauseBetweenPoints = false;
	public float PauseDuration = 3;
	public List<Vector3> PatrolPoints = new List<Vector3> ();
	public List<bool> PausePoints = new List<bool> ();

	private int CurrentPatrolIndex = 0;
	private bool Pausing = false;
	private float PauseUntil = 0;

	// Use this for initialization
	void Start () {
		LaserMinerStart ();
		if (PatrolPoints.Count == 0)
			PatrolPoints.Add (transform.position);
		UpdateDestination (PatrolPoints [0]);
	}
	
	// Update is called once per frame
	void Update () {
		StateMachine ();
	}

	protected override void Working () {
		base.Working ();

		if (State == AIState.Working) {
			if (Pausing) {
				if (Time.time > PauseUntil) {
					Pausing = false;
					ReEnableAgentMovement ();
					AgentComponent.destination = PatrolPoints [CurrentPatrolIndex];

					CurrentPatrolIndex += 1;
					if (CurrentPatrolIndex >= PatrolPoints.Count)
						CurrentPatrolIndex = 0;
				}
			} else if (Vector3.Distance (AgentComponent.destination, transform.position) < 1.5f) {

				if (PauseBetweenPoints && PausePoints [PreviousIndexLooped(CurrentPatrolIndex)]) {
					Pausing = true;
					PauseUntil = Time.time + PauseDuration;
					DisableAgentMovement ();
				} else {
					AgentComponent.destination = PatrolPoints [CurrentPatrolIndex];

					CurrentPatrolIndex += 1;
					if (CurrentPatrolIndex >= PatrolPoints.Count)
						CurrentPatrolIndex = 0;
				}
			}
		}
	}

	private int PreviousIndexLooped (int Index) {
		if (Index <= 0)
			return PatrolPoints.Count - 1;
		return Index - 1;
	}
}
