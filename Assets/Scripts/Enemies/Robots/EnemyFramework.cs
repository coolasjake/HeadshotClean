using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Holds basic functions for AI enemies including the state machine, state machine functions, player detection and health/destruction,
/// but does not enforce the use of any of them.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class EnemyFramework : MonoBehaviour
{
    public EnemyDetection detection = new EnemyDetection();
    public StateMachine stateMachine = new StateMachine();
    public EnemyMovement movement = new EnemyMovement();
    public AudioManager SFXPlayer;



    // Start is called before the first frame update
    void Start()
    {
        movement.Initialize(transform, GetComponent<Rigidbody>());
        stateMachine.startChargingDelay += (Random.value - 0.5f);

        GameObject DI = Instantiate(Resources.Load<GameObject>("Prefabs/Enemies/EFDetectionIndicator"), FindObjectOfType<Canvas>().transform);
        DI.GetComponent<EFDetectionIndicator>().Target = transform;
    }

    // Update is called once per frame
    void Update()
    {
        EnemyUpdate();
    }

    private void EnemyUpdate()
    {
        if (PauseManager.Paused)
            return;

        bool canLooseInterest = (stateMachine.state == AIState.Working || stateMachine.state == AIState.Searching);
        detection.DetectPlayer(canLooseInterest);

        StateMachineUpdate();

        MovementUpdate();
    }

    #region Detection
    [System.Serializable]
    public class EnemyDetection
    {
        #region Detection Vars
        public LayerMask raycastLookingMask;

        [Tooltip("Make sure the 'forward' vector of the head aligns with its eye-line, as it defines the default detection cone.")]
        public Transform head;
        protected float LastSawPlayer = -1000;
        /// <summary> The players last position for the bot to look at (not move to, as it will often be high in the air). </summary>
        [HideInInspector]
        public Vector3 LastPlayerPosition;
        /// <summary> The players last position for the bot to move to. </summary>
        [HideInInspector]
        public Vector3 LastPlayerGroundedPosition;
        protected float StartedSeeingPlayer = 0;
        /// <summary> Used to stop bots from searching after not being able to reach the player for a period of time </summary>
        [HideInInspector]
        public float StartedSearching;
        protected bool HaveLOSToPlayer = false;
        /// <summary> Used by the RotateHead () function to know if the AI should 'look around' or not. Mostly set by child AI scripts. </summary>
        [HideInInspector]
        //public bool LookAround = false;
        protected float LookAroundStarted = 0;
        private Vector3 LookAroundDirection = new Vector3();
        /// <summary> Rate at which player will be detected (from 0 - 1). 0 = Cannot see player, 1 = Player is 'obvious'. </summary>
        [HideInInspector]
        public float PlayerVisibility = 0;

        /// <summary> How quickly the AI will lose 'suspision' (DetectionProgress), relative to the normal gain (default = 1/s). </summary>
        public float LooseIntrestMagnitude = 0.1f;
        /// <summary> The maximum angle in degrees on either side the AIs facing (so 45deg is a 90deg cone in total), from which it can detect the player. </summary>
        public float DetectionAngle = 45;
        /// <summary> The angle (in any direction) where the player is 'obvious', resulting in very rapid detection (usually milliseconds, but varies based on distance). </summary>
        public float ObviousAngle = 10;
        /// <summary> The distance from which the AI will begin to detect the player, even if they are directly behind them. (Stops them from looking stupid when you walk behind them) </summary>
        public float AutoDetectionDistance = 5;
        /// <summary> Until the player has been out of sight for this ammount of time, the AI will still be able to access their location, creating the illusion that it can guess their most likely position. </summary>
        public static float CanGuessPositionTime = 0.3f;
        /// <summary> The enemy will not detect the player at all from this distance, unless they are within the obvious angle. Used when factoring distance into visibility. </summary>
        public static float MaxDetectionDistance = 300;
        /// <summary> The angle (in any direction) where the AI will begin to detect the player (and perform actions like turning its head) if it is already suspicious (DetectionProgress > 0.5f). </summary>
        public static float AlertDetectionAngle = 120;
        /// <summary> How close the enemy is to detecting the player - increased by being in the enemys view cone, speed based on distance. </summary>
        [HideInInspector]
        public float DetectionProgress = 0;
        /// <summary> How easy it is for this bot to detect the player. The value will roughly translate to time, but detection is dependant on angles, LOS and distance. </summary>
        public float DetectionDifficulty = 2f;
        #endregion

        #region Detection Functions
        public bool CheckPlayerInLOS()
        {
            //If a raycast from head to center, or head to head hits the player return true.
            RaycastHit Hit;
            if (Physics.Raycast(head.position, (Movement.ThePlayer.transform.position - head.position), out Hit, 600, raycastLookingMask))
            {
                if (Hit.transform.CompareTag("Player"))
                    return true;
            }
            if (Physics.Raycast(head.position, (Movement.ThePlayer.MainCamera.transform.position - head.position), out Hit, 600, raycastLookingMask))
            {
                if (Hit.transform.CompareTag("Player"))
                    return true;
            }
            return false;
        }

        public float CalculatePlayerVisibility()
        {
            //Only called if the player is already in LOS

            float playerVisibility = 0f;
            float AngleToPlayer = Vector3.Angle(head.forward, Movement.ThePlayer.transform.position - head.position);
            if (AngleToPlayer < ObviousAngle)
            {
                //If the player is within the 'Obvious' angle, set player visibility relative to distance plus 0.5f
                float playerDist = Vector3.Distance(Movement.ThePlayer.transform.position, head.position);
                playerVisibility = 0.5f + (1 - (playerDist / MaxDetectionDistance)) * 0.5f;
            }
            else if (AngleToPlayer < DetectionAngle || (DetectionProgress > DetectionDifficulty * 0.25f && AngleToPlayer < AlertDetectionAngle))
            {
                //If the player is within the detection angle, set player visibility relative to distance, plus a bonus based on how close the angle is to the Obvious angle
                float playerDist = Vector3.Distance(Movement.ThePlayer.transform.position, head.position);
                if (playerDist < MaxDetectionDistance)
                {
                    playerVisibility = (1 - (playerDist / MaxDetectionDistance)) * 0.5f;
                    playerVisibility += ((AngleToPlayer - ObviousAngle) / (DetectionAngle - ObviousAngle)) * 0.5f;
                }
            }
            else if (Vector3.Distance(Movement.ThePlayer.transform.position, head.position) < AutoDetectionDistance)
            {
                playerVisibility = 0.2f;
            }

            return playerVisibility;
        }

        public void DetectPlayer()
        {
            DetectPlayer(true);
        }

        public void DetectPlayer(bool canLooseInterest)
        {
            bool DetectingPlayer = false;
            PlayerVisibility = 0; //PlayerVisibility of 1 equals instant detection, and values 0 -> 1 are added to the detection progress
            bool playerInLOS = CheckPlayerInLOS();

            if (playerInLOS)
            {
                PlayerVisibility = CalculatePlayerVisibility();
                if (PlayerVisibility > 0)
                    DetectingPlayer = true;
            }

            if (PlayerVisibility == 1)
            {
                //If player is fully visible, set the time that the player was last seen to the current time
                LastSawPlayer = Time.time;
            }

            if (DetectingPlayer || Time.time < LastSawPlayer + CanGuessPositionTime)
            {
                RememberPlayerPos();
            }

            if (DetectingPlayer && DetectionProgress < DetectionDifficulty)
            {
                DetectionProgress += PlayerVisibility * Time.deltaTime;
            }
            else if (!DetectingPlayer && PlayerVisibility == 0 && DetectionProgress > 0 && canLooseInterest)
            {
                DetectionProgress -= Time.deltaTime * LooseIntrestMagnitude;
            }
        }

        public void Alert()
        {
            RememberPlayerPos();
            DetectionProgress = DetectionDifficulty * 1.1f;
            StartedSearching = Time.time;
        }

        private void RememberPlayerPos()
        {
            LastPlayerPosition = Movement.CameraPos;
            LastPlayerGroundedPosition = Movement.ThePlayer._AIFollowPoint;
        }
        #endregion
    }
    #endregion

    #region StateMachine
    [System.Serializable]
    public class StateMachine
    {
        public AIState state = AIState.Working;

        /// <summary> Time after seeing the player until this enemy will shoot. </summary>
        public float startChargingDelay = 3;
        /// <summary> Time that this enemy will charge for (red light -> firing). </summary>
        public float chargeTime = 3;
        /// <summary> Minimum time inbetween firing. </summary>
        public float fireCooldown = 10;
        /// <summary> The duration of the laser. </summary>
        public float fireDuration = 2;
        /// <summary> How long the enemy waits when alarmed before notifying other enemies. </summary>
        public float alarmDuration = 2;
        /// <summary> The minimum duration of the search. </summary>
        public float minSearchDuration = 5;
        /// <summary> The maximum duration of the search. </summary>
        public float maxSearchDuration = 30;
        /// <summary> Ammount of damage this enemy does per second of laser contact. </summary>
        public float DPS = 50;
        /// <summary> Range from which a Bot-to-Bot alarm will alert other AI (Radius). </summary>
        public float alarmRange = 10;

        //private int RaycastMask;
        /// <summary> 'Counter' which stops the enemy from firing too often, but is not cheesed by temporary loss of LOS. </summary>
        [HideInInspector]
        public float firingDelayTimer;
        /// <summary> Time that this enemy started charging. </summary>
        [HideInInspector]
        public float startedCharging;
        /// <summary> Time that this enemy started firing. </summary>
        [HideInInspector]
        public float startedFiring;
        /// <summary> Time that this enemy stopped firing, which is used for the minumum delay between shots. </summary>
        [HideInInspector]
        public float stoppedFiring;
        /// <summary> Time that this enemy started giving the alarm. </summary>
        [HideInInspector]
        public float startedAlarm;

        public StateMachine()
        {
            state = AIState.Working;
            firingDelayTimer = startChargingDelay;
            stoppedFiring = -fireCooldown;
        }
    }

    protected virtual void StateMachineUpdate()
    {
        if (stateMachine.state == AIState.Working)
            Working();

        if (stateMachine.state == AIState.Alarmed)
            Alarmed();

        if (stateMachine.state == AIState.Staring)
            Staring();

        if (stateMachine.state == AIState.Searching)
            Searching();

        if (stateMachine.state == AIState.Charging)
            Charging();

        if (stateMachine.state == AIState.Firing)
            Firing();
    }

    private void TrackPlayer()
    {
        if (Time.time > movement.LastRefresh + movement.RefreshRate)
        {
            movement.ChangeMoveTarget(detection.LastPlayerGroundedPosition);
            movement.LastRefresh = Time.time;
        }
    }

    #region StateUpdates
    /// <summary> Checks if the player is visible when in working mode (slightly harder to be noticed than in other states). </summary>
    protected virtual void Working()
    {
        //If the player is 'obvious' or has been in sight for a long time, start giving the alarm.
        if (detection.PlayerVisibility >= 1 || detection.DetectionProgress >= detection.DetectionDifficulty)
        {
            if (Time.time > Network.LastAlarm + Network.AlarmFrequency && Network.AlarmedBots < 3)
            {
                StartAlarmed();
            }
            else
            {
                StartStaring();
            }
            Network.AlarmedBots += 1;
        }
        else if (detection.DetectionProgress > detection.DetectionDifficulty * 0.5f && detection.PlayerVisibility != 0)
        {
            //If the player has been at the edge of the bots vision for a while, go to 'searching' mode.
            StartSearching();
            Network.AlarmedBots += 1;
        }
    }

    /// <summary> Warns nearby bots that the player is near, at the cost of a delayed reaction. Occurs when player is obvious and an alarm hasn't been raised for a while. </summary>
    protected virtual void Alarmed()
    {
        if (Time.time > stateMachine.startedAlarm + stateMachine.alarmDuration)
        {
            //Find nearby enemies, put them in searching mode and give them the players location.
            //Needs fixing HERE
            foreach (LookingEnemy Bot in FindObjectsOfType<LookingEnemy>())
            {
                if (Vector3.Distance(Bot.transform.position, transform.position) < stateMachine.alarmRange)
                    Bot.SoundAlarm(detection.LastPlayerPosition, detection.LastPlayerGroundedPosition);
            }
            StartStaring();
        }

        TrackPlayer();
    }

    /// <summary> The AI 'knows' the player is nearby, but cannot see them, and will look around while moving to the last known location (if possible). Attack delay is not counted here, but not reset either. </summary>
    protected virtual void Searching()
    {

        if (detection.PlayerVisibility >= 1 || detection.DetectionProgress >= detection.DetectionDifficulty)
        {
            if (Time.time > Network.LastAlarm + Network.AlarmFrequency && Network.AlarmedBots < 3)
            {
                StartAlarmed();
            }
            else
            {
                StartStaring();
            }
        }
        else if ((Vector3.Distance(transform.position, detection.LastPlayerGroundedPosition) < 5 && Time.time > detection.StartedSearching + stateMachine.minSearchDuration)
            || Time.time > detection.StartedSearching + stateMachine.maxSearchDuration)
        {
            StartWorking();
        }
    }

    /// <summary> The state where the AI can see the player, and is either moving towards them or waiting for the attack delay. </summary>
    protected virtual void Staring()
    {
        if (detection.PlayerVisibility == 0)
        {
            StartSearching();
            detection.DetectionProgress = detection.DetectionDifficulty * 0.75f;
        }
        else
        {
            stateMachine.firingDelayTimer -= Time.deltaTime;
            if (stateMachine.firingDelayTimer <= 0 && Time.time > stateMachine.stoppedFiring + stateMachine.fireCooldown)
            {
                StartCharging();
            }
            else
            {
                TrackPlayer();
            }
        }
    }

    /// <summary> The AI is 'powering up' its attack; this freezes it (including head movement), and causes lights and sounds to appear. This is NOT cancelled by losing LOS. Note: designed for the Laser Enemies, may not work well for others. </summary>
    protected virtual void Charging()
    {
        if (Time.time >= stateMachine.startedCharging + stateMachine.chargeTime)
        {
            StartFiring();
        }
    }

    /// <summary> The AI fires the charged attack, then goes into Staring mode after resetting cooldowns.
    /// For laser enemies this means 'drawing a line' between the players position at the start and end of charging, and then continuing for 300% of 
    /// the distance, or 100% of the time (rotation speed increases linearly, and the players saved position is reached after half the drawing time).
    /// </summary>
    protected virtual void Firing()
    {
        if (Time.time >= stateMachine.startedFiring + stateMachine.fireDuration)
        {
            StopFiring();
        }
    }
    #endregion

    #region StateChanges
    protected virtual void StartWorking()
    {
        stateMachine.state = AIState.Working;
        movement.ChangeMoveTarget(movement.RestLocation);
        detection.DetectionProgress = detection.DetectionDifficulty * 0.75f;
        stateMachine.firingDelayTimer = stateMachine.startChargingDelay;
        Network.AlarmedBots -= 1;
    }

    protected virtual void StartAlarmed()
    {
        stateMachine.state = AIState.Alarmed;
        stateMachine.startedAlarm = Time.time;
    }

    protected virtual void StartSearching()
    {
        stateMachine.state = AIState.Searching;
        detection.StartedSearching = Time.time;
    }

    protected virtual void StartStaring()
    {
        stateMachine.state = AIState.Staring;
        stateMachine.firingDelayTimer = stateMachine.startChargingDelay;
    }

    protected virtual void StartCharging()
    {
        stateMachine.state = AIState.Charging;
        stateMachine.startedCharging = Time.time;
    }

    protected virtual void StartFiring()
    {
        stateMachine.state = AIState.Firing;
        stateMachine.startedFiring = Time.time;
    }

    protected virtual void StopFiring()
    {
        //Stop Firing
        stateMachine.state = AIState.Staring;
        stateMachine.stoppedFiring = Time.time;
    }
    #endregion
    #endregion

    #region EnemyMovement
    [System.Serializable]
    public class EnemyMovement
    {
        [HideInInspector]
        public Rigidbody RB;
        [HideInInspector]
        public Transform enemyTransform;
        [HideInInspector]
        public float LastRefresh = 0;
        [HideInInspector]
        public Vector3 RestLocation;
        public static bool PlaySounds = false;
        /// <summary>  The refresh rate for pathfinding. </summary>
        public float turnSpeed = 30f;
        public float moveSpeed = 2f;
        public float acceleration = 2f;
        public float brakingMultiplier = 1f;
        public float RefreshRate = 1f;
        public float navPointSize = 1;
        public float turnAccuracy = 1f;
        [HideInInspector]
        public Vector3 moveTarget;

        [HideInInspector]
        public NavMeshPath _path;
        [HideInInspector]
        public int _nextPathPoint = 0;

        public void Initialize(Transform transform, Rigidbody rigidbody)
        {
            enemyTransform = transform;
            RB = rigidbody;

            LastRefresh = Random.value * RefreshRate;
            RestLocation = enemyTransform.position;
            moveTarget = enemyTransform.position;

            _path = new NavMeshPath();
        }

        public bool ChangeMoveTarget(Vector3 Destination)
        {
            moveTarget = Destination;
            return GetPath(Destination);
        }

        public bool GetPath(Vector3 targetPos)
        {
            _nextPathPoint = 0;
            return NavMesh.CalculatePath(enemyTransform.position, targetPos, NavMesh.AllAreas, _path);
        }

        public Vector3 NextPoint
        {
            get
            {
                if (_path == null || _nextPathPoint >= _path.corners.Length)
                    return enemyTransform.position;

                return _path.corners[_nextPathPoint];
            }
        }

        public bool WithinRange(Vector3 start, Vector3 end, float range)
        {
            return (Vector3.SqrMagnitude(start - end) < range * range);
        }

        public bool ReachedPoint(Vector3 start, Vector3 end)
        {
            return WithinRange(start.FixedY(0), end.FixedY(0), navPointSize);
        }
    }

    protected virtual void MovementUpdate()
    {
        if (movement.ReachedPoint(transform.position, movement.NextPoint))
            movement._nextPathPoint += 1;

        float angleToPoint = SignedHorAngleToTarget(movement.NextPoint);
        TurnByAngle(angleToPoint);
        if (Utility.UnsignedDifference(angleToPoint, 0f) < movement.turnAccuracy)
            MoveTowardsTarget(movement.NextPoint);
    }

    protected virtual float SignedHorAngleToTarget(Vector3 targetPos)
    {
        Vector3 targetDir = targetPos.FixedY(0) - transform.position.FixedY(0);
        return Vector3.SignedAngle(transform.forward, targetDir, transform.up);
    }

    protected virtual void TurnByAngle(float signedAngle)
    {
        float maxTurnThisFrame = movement.turnSpeed * Time.deltaTime;
        float amountToTurn = Mathf.Clamp(signedAngle, -maxTurnThisFrame, maxTurnThisFrame);
        transform.Rotate(0, amountToTurn, 0);
    }

    protected virtual void MoveTowardsTarget(Vector3 targetPos)
    {
        Vector3 targetDir = targetPos - transform.position;
        Vector3 targetVel = targetDir.FixedY(0).normalized * movement.moveSpeed;
        AccelerateToVelocity(targetVel);
    }

    protected virtual void AccelerateToVelocity(Vector3 targetVel)
    {
        Vector3 difference = targetVel.FixedY(0) - movement.RB.velocity.FixedY(0);
        float force = movement.acceleration * movement.RB.mass * Time.deltaTime;
        if (targetVel.FixedY(0).sqrMagnitude < movement.RB.velocity.FixedY(0).sqrMagnitude)
            force = force * movement.brakingMultiplier;
        force = Mathf.Min(force, difference.magnitude);
        movement.RB.AddForce(difference.normalized * force);
    }

    protected void UpdateNextPoint()
    {
        if (Vector3.SqrMagnitude((movement.NextPoint - transform.position).FixedY(0)) < movement.navPointSize * movement.navPointSize)
        {
            movement._nextPathPoint += 1;
        }
    }
    #endregion
}