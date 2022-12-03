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
    [Header("Base Enemy Framework:")]
    public EnemyDetection detection = new EnemyDetection();
    public StateMachine combatAndStates = new StateMachine();
    public EnemyMovement movement = new EnemyMovement();
    public AudioManager SFXPlayer;



    // Start is called before the first frame update
    void Start()
    {
        movement.Initialize(transform, GetComponent<Rigidbody>());
        combatAndStates.startChargingDelay += (Random.value - 0.5f);

        CreateDetectionIndicator();

        EFStart();
    }

    protected virtual void EFStart()
    {

    }

    protected virtual void CreateDetectionIndicator()
    {
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

        bool canLooseInterest = (combatAndStates.state == AIState.Working || combatAndStates.state == AIState.Searching);
        detection.DetectPlayer(canLooseInterest);

        StateMachineUpdate();

        MovementUpdate();
    }

    #region Detection
    [System.Serializable]
    public class EnemyDetection
    {
        #region Detection Vars
        /// <summary> The mask used for detection raycasts. </summary>
        public LayerMask raycastLookingMask;

        [Tooltip("Make sure the 'forward' vector of the head aligns with its eye-line, as it defines the default detection cone.")]
        /// <summary> The transform of the robots 'head', which detection raycasts and angles are checked relative to. </summary>
        public Transform head;


        /// <summary> The maximum angle in degrees on either side the AIs facing (so 45deg is a 90deg cone in total), from which it can detect the player. </summary>
        public float detectionAngle = 45;
        /// <summary> The angle (in any direction) where the player is 'obvious', resulting in very rapid detection (usually milliseconds, but varies based on distance). </summary>
        public float obviousAngle = 10;
        /// <summary> The distance from which the AI will begin to detect the player, even if they are directly behind them. (Stops them from looking stupid when you walk behind them) </summary>
        public float autoDetectionDistance = 5;
        /// <summary> Until the player has been out of sight for this ammount of time, the AI will still be able to access their location, creating the illusion that it can guess their most likely position. </summary>
        public float canGuessPositionTime = 0.3f;
        /// <summary> The enemy will not detect the player at all from this distance, unless they are within the obvious angle. Used when factoring distance into visibility. </summary>
        public float maxDetectionDistance = 300;
        /// <summary> The angle (in any direction) where the AI will begin to detect the player (and perform actions like turning its head) if it is already suspicious (DetectionProgress > 1/4). </summary>
        public static float alertDetectionAngle = 120;
        /// <summary> How easy it is for this bot to detect the player. The value will roughly translate to time, but detection is dependant on angles, LOS and distance. </summary>
        public float detectionDifficulty = 2f;
        /// <summary> How quickly the AI will lose 'suspision' (DetectionProgress), relative to the normal gain (default = 1/s). </summary>
        public float looseIntrestMagnitude = 0.1f;


        /// <summary> The last time the player was in LOS, so that the duration of searches can be checked. </summary>
        protected float _lastSawPlayer = -1000;
        /// <summary> The players last position for the bot to look at (not move to, as it will often be high in the air). </summary>
        [HideInInspector]
        public Vector3 _lastPlayerPosition;
        /// <summary> The last known grounded position of the player (AIFollowPoint calculated in the Movement script by raycasting downwards). </summary>
        [HideInInspector]
        public Vector3 _lastPlayerGroundedPosition;
        /// <summary> Used to stop bots from searching after not being able to reach the player for a period of time </summary>
        [HideInInspector]
        public float _startedSearching;

        /// <summary> Rate at which player will be detected (from 0 - 1). 0 = Cannot see player, 1 = Player is 'obvious'. </summary>
        [HideInInspector]
        public float _playerVisibility = 0;
        /// <summary> How close the enemy is to detecting the player - increased by being in the enemys view cone, speed based on distance. </summary>
        [HideInInspector]
        public float _detectionProgress = 0;
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
            if (AngleToPlayer < obviousAngle)
            {
                //If the player is within the 'Obvious' angle, set player visibility relative to distance plus 0.5f
                float playerDist = Vector3.Distance(Movement.ThePlayer.transform.position, head.position);
                playerVisibility = 0.5f + (1 - (playerDist / maxDetectionDistance)) * 0.5f;
            }
            else if (AngleToPlayer < detectionAngle || (_detectionProgress > detectionDifficulty * 0.25f && AngleToPlayer < alertDetectionAngle))
            {
                //If the player is within the detection angle, set player visibility relative to distance, plus a bonus based on how close the angle is to the Obvious angle
                float playerDist = Vector3.Distance(Movement.ThePlayer.transform.position, head.position);
                if (playerDist < maxDetectionDistance)
                {
                    playerVisibility = (1 - (playerDist / maxDetectionDistance)) * 0.5f;
                    playerVisibility += ((AngleToPlayer - obviousAngle) / (detectionAngle - obviousAngle)) * 0.5f;
                }
            }
            else if (Vector3.Distance(Movement.ThePlayer.transform.position, head.position) < autoDetectionDistance)
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
            _playerVisibility = 0; //PlayerVisibility of 1 equals instant detection, and values 0 -> 1 are added to the detection progress
            bool playerInLOS = CheckPlayerInLOS();

            if (playerInLOS)
            {
                _playerVisibility = CalculatePlayerVisibility();
                if (_playerVisibility > 0)
                    DetectingPlayer = true;
            }

            if (_playerVisibility == 1)
            {
                //If player is fully visible, set the time that the player was last seen to the current time
                _lastSawPlayer = Time.time;
            }

            if (DetectingPlayer || Time.time < _lastSawPlayer + canGuessPositionTime)
            {
                RememberPlayerPos();
            }

            if (DetectingPlayer && _detectionProgress < detectionDifficulty)
            {
                _detectionProgress += _playerVisibility * Time.deltaTime;
            }
            else if (!DetectingPlayer && _playerVisibility == 0 && _detectionProgress > 0 && canLooseInterest)
            {
                _detectionProgress -= Time.deltaTime * looseIntrestMagnitude;
            }
        }

        public void Alert()
        {
            RememberPlayerPos();
            _detectionProgress = detectionDifficulty * 1.1f;
            _startedSearching = Time.time;
        }

        private void RememberPlayerPos()
        {
            _lastPlayerPosition = Movement.CameraPos;
            _lastPlayerGroundedPosition = Movement.ThePlayer._AIFollowPoint;
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
        
        /// <summary> 'Counter' which stops the enemy from firing too often, but is not cheesed by temporary loss of LOS. </summary>
        [HideInInspector]
        public float _firingDelayTimer;
        /// <summary> Time that this enemy started charging. </summary>
        [HideInInspector]
        public float _startedCharging;
        /// <summary> Time that this enemy started firing. </summary>
        [HideInInspector]
        public float _startedFiring;
        /// <summary> Time that this enemy stopped firing, which is used for the minumum delay between shots. </summary>
        [HideInInspector]
        public float _stoppedFiring;
        /// <summary> Time that this enemy started giving the alarm. </summary>
        [HideInInspector]
        public float _startedAlarm;

        public StateMachine()
        {
            state = AIState.Working;
            _firingDelayTimer = startChargingDelay;
            _stoppedFiring = -fireCooldown;
        }
    }

    protected virtual void StateMachineUpdate()
    {
        if (combatAndStates.state == AIState.Working)
            Working();

        if (combatAndStates.state == AIState.Alarmed)
            Alarmed();

        if (combatAndStates.state == AIState.Staring)
            Staring();

        if (combatAndStates.state == AIState.Searching)
            Searching();

        if (combatAndStates.state == AIState.Charging)
            Charging();

        if (combatAndStates.state == AIState.Firing)
            Firing();
    }

    private void TrackPlayer()
    {
        if (Time.time > movement._lastRefresh + movement.refreshRate)
        {
            movement.ChangeMoveTarget(detection._lastPlayerGroundedPosition);
            movement._lastRefresh = Time.time;
        }
    }

    #region StateUpdates
    /// <summary> Checks if the player is visible when in working mode (slightly harder to be noticed than in other states). </summary>
    protected virtual void Working()
    {
        //If the player is 'obvious' or has been in sight for a long time, start giving the alarm.
        if (detection._playerVisibility >= 1 || detection._detectionProgress >= detection.detectionDifficulty)
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
        else if (detection._detectionProgress > detection.detectionDifficulty * 0.5f && detection._playerVisibility != 0)
        {
            //If the player has been at the edge of the bots vision for a while, go to 'searching' mode.
            StartSearching();
            Network.AlarmedBots += 1;
        }
    }

    /// <summary> Warns nearby bots that the player is near, at the cost of a delayed reaction. Occurs when player is obvious and an alarm hasn't been raised for a while. </summary>
    protected virtual void Alarmed()
    {
        if (Time.time > combatAndStates._startedAlarm + combatAndStates.alarmDuration)
        {
            //Find nearby enemies, put them in searching mode and give them the players location.
            //Needs fixing HERE
            foreach (LookingEnemy Bot in FindObjectsOfType<LookingEnemy>())
            {
                if (Vector3.Distance(Bot.transform.position, transform.position) < combatAndStates.alarmRange)
                    Bot.SoundAlarm(detection._lastPlayerPosition, detection._lastPlayerGroundedPosition);
            }
            StartStaring();
        }

        TrackPlayer();
    }

    /// <summary> The AI 'knows' the player is nearby, but cannot see them, and will look around while moving to the last known location (if possible). Attack delay is not counted here, but not reset either. </summary>
    protected virtual void Searching()
    {

        if (detection._playerVisibility >= 1 || detection._detectionProgress >= detection.detectionDifficulty)
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
        else if ((Vector3.Distance(transform.position, detection._lastPlayerGroundedPosition) < 5 && Time.time > detection._startedSearching + combatAndStates.minSearchDuration)
            || Time.time > detection._startedSearching + combatAndStates.maxSearchDuration)
        {
            StartWorking();
        }
    }

    /// <summary> The state where the AI can see the player, and is either moving towards them or waiting for the attack delay. </summary>
    protected virtual void Staring()
    {
        if (detection._playerVisibility == 0)
        {
            StartSearching();
            detection._detectionProgress = detection.detectionDifficulty * 0.75f;
        }
        else
        {
            combatAndStates._firingDelayTimer -= Time.deltaTime;
            if (combatAndStates._firingDelayTimer <= 0 && Time.time > combatAndStates._stoppedFiring + combatAndStates.fireCooldown)
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
        if (Time.time >= combatAndStates._startedCharging + combatAndStates.chargeTime)
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
        if (Time.time >= combatAndStates._startedFiring + combatAndStates.fireDuration)
        {
            StopFiring();
        }
    }
    #endregion

    #region StateChanges
    protected virtual void StartWorking()
    {
        combatAndStates.state = AIState.Working;
        movement.ChangeMoveTarget(movement.restLocation);
        detection._detectionProgress = detection.detectionDifficulty * 0.75f;
        combatAndStates._firingDelayTimer = combatAndStates.startChargingDelay;
        Network.AlarmedBots -= 1;
    }

    protected virtual void StartAlarmed()
    {
        combatAndStates.state = AIState.Alarmed;
        combatAndStates._startedAlarm = Time.time;
    }

    protected virtual void StartSearching()
    {
        combatAndStates.state = AIState.Searching;
        detection._startedSearching = Time.time;
    }

    protected virtual void StartStaring()
    {
        combatAndStates.state = AIState.Staring;
        combatAndStates._firingDelayTimer = combatAndStates.startChargingDelay;
    }

    protected virtual void StartCharging()
    {
        combatAndStates.state = AIState.Charging;
        combatAndStates._startedCharging = Time.time;
    }

    protected virtual void StartFiring()
    {
        combatAndStates.state = AIState.Firing;
        combatAndStates._startedFiring = Time.time;
    }

    protected virtual void StopFiring()
    {
        //Stop Firing
        combatAndStates.state = AIState.Staring;
        combatAndStates._stoppedFiring = Time.time;
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
        /// <summary> The default position of the enemy, which they will return to if the player cannot be found (usually the starting position). </summary>
        [HideInInspector]
        public Vector3 restLocation;
        
        public bool showPathGizmo = true;

        [Space]
        /// <summary> Turn speed in degrees per second of the enemies body. </summary>
        public float turnSpeed = 30f;
        /// <summary> Move speed of the enemy. </summary>
        public float moveSpeed = 5f;
        /// <summary> Acceleration rate of this enemy. </summary>
        public float acceleration = 100f;
        /// <summary> Multiplier to the acceleration rate when reducing speed instead of increasing it. </summary>
        [Min(0)]
        public float brakingMultiplier = 1f;
        /// <summary> Multiplier for the acceleration added when turning. Formula is [force + force * (1 - Cosθ) * turningEffectiveness], meaning 1 = on rails, 0 = on ice. </summary>
        [Range(0, 1)]
        public float turningEffectiveness = 0.5f;
        /// <summary> Max rate that expensive calculations (such as pathfinding) can be performed. </summary>
        public float refreshRate = 1f;
        /// <summary> The distance from a point in the nav-path at which the enemy moves to the next point. </summary>
        public float navPointSize = 1;
        /// <summary> The minimum distance in degrees from the desired angle before the enemy can move
        /// (Note: This behaviour can be overridden). </summary>
        public float turnAccuracy = 1f;

        /// <summary> The point that the enemy is currently trying to move to
        /// (Note: not always the same as the last point in the calculated path - target may be inaccessible). </summary>
        [HideInInspector]
        public Vector3 _moveTarget;
        /// <summary> The last time expensive calculations (such as pathfinding) were performed.
        /// Enemies are given random starting times for refreshes to avoid frame drops. </summary>
        [HideInInspector]
        public float _lastRefresh = 0;
        /// <summary> The nav-mesh path calculated between this enemy and the moveTarget. </summary>
        [HideInInspector]
        public NavMeshPath _path;
        /// <summary> The index of the point in the path which is currently being targeted (almost always 1 when the refresh rate is low). </summary>
        [HideInInspector]
        public int _nextPathPoint = 0;

        public void Initialize(Transform transform, Rigidbody rigidbody)
        {
            enemyTransform = transform;
            RB = rigidbody;

            _lastRefresh = Random.value * refreshRate;
            restLocation = enemyTransform.position;
            _moveTarget = enemyTransform.position;

            _path = new NavMeshPath();
        }

        public bool ChangeMoveTarget(Vector3 Destination)
        {
            _moveTarget = Destination;
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

        public bool ReachedPoint(Vector3 current, Vector3 target)
        {
            return Utility.WithinRange(current.FixedY(0), target.FixedY(0), navPointSize);
        }

        public float SignedHorAngleToTarget(Vector3 targetPos)
        {
            Vector3 targetDir = targetPos.FixedY(0) - enemyTransform.position.FixedY(0);
            return Vector3.SignedAngle(enemyTransform.forward, targetDir, enemyTransform.up);
        }
    }

    protected virtual void MovementUpdate()
    {
        UpdateNextPoint();

        float angleToPoint = movement.SignedHorAngleToTarget(movement.NextPoint);
        TurnByAngle(angleToPoint);
        if (Utility.UnsignedDifference(angleToPoint, 0f) < movement.turnAccuracy)
            MoveTowardsTarget(movement.NextPoint);
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
        else if (movement.turningEffectiveness > 0)
            force += force * (1 - Mathf.Cos(Vector3.Angle(targetVel.FixedY(0), movement.RB.velocity.FixedY(0)))) * movement.turningEffectiveness;
        force = Mathf.Min(force, difference.magnitude * movement.RB.mass / Time.fixedDeltaTime);
        movement.RB.AddForce(difference.normalized * force);
    }

    protected void UpdateNextPoint()
    {
        if (movement.ReachedPoint(movement.NextPoint, transform.position))
        {
            movement._nextPathPoint += 1;
        }
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (movement.showPathGizmo)
        {
            if (movement._path == null)
                return;
            if (movement._path.corners != null && movement._path.corners.Length > 1)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawSphere(movement._path.corners[0], movement.navPointSize);
                Gizmos.color = Color.white;
                for (int i = 1; i < movement._path.corners.Length; ++i)
                {
                    Gizmos.DrawLine(movement._path.corners[i - 1], movement._path.corners[i]);
                    Gizmos.DrawSphere(movement._path.corners[i], movement.navPointSize);
                }
            }
        }
    }
    #endregion
}