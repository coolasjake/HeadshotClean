using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemy : ShootingEnemy {

	public bool PauseBetweenPoints = false;
	public float PauseDuration = 3;
	public List<Vector3> PatrolPoints = new List<Vector3> ();
	public List<bool> PausePoints = new List<bool> ();

	private int CurrentPatrolIndex = 0;
	private bool Pausing = false;
	private float PauseUntil = 0;

	// Use this for initialization
	void Start () {
		ShootingEnemyInitialize ();
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

    private void OnDrawGizmosSelected()
    {
        if (PatrolPoints == null || PatrolPoints.Count == 0)
            return;

        Gizmos.color = Color.green;
        for (int i = 0; i < PatrolPoints.Count - 1; ++i)
        {
            Gizmos.DrawLine(PatrolPoints[i], PatrolPoints[i + 1]);
            Gizmos.DrawWireSphere(PatrolPoints[i], 0.2f);
        }
        Gizmos.DrawLine(PatrolPoints[PatrolPoints.Count - 1], PatrolPoints[0]);
        Gizmos.DrawWireSphere(PatrolPoints[PatrolPoints.Count - 1], 0.2f);
    }
}
