using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public abstract class MovingEnemy : LookingEnemy {

	//public Movement Target;
    [SerializeField]
	private bool Disabled = false;

	protected NavMeshAgent AgentComponent;
    protected Rigidbody RB;
	protected float LastRefresh = 0;
	protected Vector3 RestLocation;
	public static bool PlaySounds = false;
	//public static float ForgetTime = 10;
	/// <summary>  The refresh rate for pathfinding. </summary>
	public static float RefreshRate = 1f;
    public float navPointSize;
    protected Vector3 moveTarget;


	// Use this for initialization
	void Start () {
        MEInitialize();
	}

    protected void MEInitialize()
    {
        LEInitialise();
        _path = new NavMeshPath();
        LastRefresh = Random.value * RefreshRate;
        RestLocation = transform.position;
        AgentComponent = GetComponent<NavMeshAgent>();
        moveTarget = transform.position;
        RB = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
		MovingEnemyUpdate ();
	}

	public void MovingEnemyUpdate () {
		if (PauseManager.Paused)
            return;

		DetectPlayer ();

		RotateHead ();

		//Skip all checks and motion (usually so another script can control them exclusively).
		if (Freeze)
			return;
    }

    protected NavMeshPath _path;
    protected int _nextPathPoint = 0;
    protected bool GetPath(Vector3 targetPos)
    {
        _nextPathPoint = 0;
        return NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, _path);
    }

    protected Vector3 NextPoint
    {
        get
        {
            if (_path == null || _nextPathPoint >= _path.corners.Length)
                return transform.position + transform.forward;

            return _path.corners[_nextPathPoint];
        }
    }

    protected void UpdateNextPoint()
    {
        if (Vector3.SqrMagnitude((NextPoint - transform.position).FixedY(0)) < navPointSize * navPointSize)
        {
            _nextPathPoint += 1;
        }
    }

    protected bool WithinRange(Vector3 start, Vector3 end, float range)
    {
        return (Vector3.SqrMagnitude(start - end) < range * range);
    }

    protected bool ReachedPoint(Vector3 start, Vector3 end, float range)
    {
        return WithinRange(start.FixedY(0), end.FixedY(0), range);
    }

    public bool UpdateDestination (Vector3 Destination)
    {
        moveTarget = Destination;
        if (!Disabled)
        {
            AgentComponent.SetDestination(Destination);
            return AgentComponent.pathStatus == NavMeshPathStatus.PathComplete;
        }
        return GetPath(Destination);
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
