using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningDroid : EnemyFramework
{
    [Header("Mining Droid Settings")]
    public Transform firingPoint;
    public LineRenderer laserLine;
    public Light firingLight;
    public LayerMask raycastShootingMask;

    public Transform shoulders;
    public Transform box;
    [Range(0, 180)]
    public float maxBoxAngle = 75;
    [Range(0, -180)]
    public float minBoxAngle = -75;
    //public float shoulderRotSpeed;
    public float boxRotSpeed = 90;

    public List<Transform> miningTargets = new List<Transform>();
    public float miningMaxDistance = 20f;
    public float miningMinDistance = 5f;
    private int _currentTarget = 0;

    private bool _lookAround = false;
    private float _lookAroundStarted = -1000;
    private Vector3 _lookAroundDirection = Vector3.zero;

    public float _laserDPS = 1f;
    private bool _stabilise = false;
    private bool _freeze = false;
    
    private Vector3 _startTargetLocation;
    private Vector3 _endTargetLocation;
    private Vector3 _lastDecalPoint = new Vector3();

    private WorkStates workState = WorkStates.moving;

    private bool boxDestroyed = false;

    #region EnemyFramework Overrides
    protected override void EFStart()
    {
        firingLight.enabled = false;
        laserLine.enabled = false;
    }

    protected override void StateMachineUpdate()
    {
        base.StateMachineUpdate();

        DoHeadRotation();
    }

    protected override void MovementUpdate()
    {
        if (_freeze)
            _stabilise = true;

        if (_stabilise)
        {
            AccelerateToVelocity(Vector3.zero);
            if (movement.RB.velocity.sqrMagnitude < 0.1f)
                _stabilise = false;
        }


        if (combatAndStates.state == AIState.Firing)
            return;

        if (combatAndStates.state == AIState.Charging)
            AimBoxAtTarget(_startTargetLocation);
        else
        {
            UpdateNextPoint();

            float angleToPoint = movement.SignedHorAngleToTarget(movement.NextPoint);
            TurnByAngle(angleToPoint);
            MoveTowardsTarget(movement.NextPoint);
        }
    }

    /// <summary> Overridden to enable y-axis movement. </summary>
    protected override void MoveTowardsTarget(Vector3 targetPos)
    {
        Vector3 targetDir = targetPos - transform.position;
        Vector3 targetVel = targetDir.normalized * movement.moveSpeed;
        AccelerateToVelocity(targetVel);
    }

    /// <summary> Overridden to enable y-axis forces. </summary>
    protected override void AccelerateToVelocity(Vector3 targetVel)
    {
        Vector3 difference = targetVel - movement.RB.velocity;
        float force = movement.acceleration * movement.RB.mass * Time.deltaTime;
        if (targetVel.sqrMagnitude < movement.RB.velocity.sqrMagnitude)
            force = force * movement.brakingMultiplier;
        else if (movement.turningEffectiveness > 0)
            force += force * (1 - Mathf.Cos(Vector3.Angle(targetVel.FixedY(0), movement.RB.velocity.FixedY(0)))) * movement.turningEffectiveness;
        force = Mathf.Min(force, difference.magnitude * movement.RB.mass / Time.fixedDeltaTime);
        movement.RB.AddForce(difference.normalized * force);
    }
    #endregion

    #region Work Sub-States
    private enum WorkStates
    {
        moving,
        charging,
        firing
    }

    private void WorkStateMachine()
    {
        if (workState == WorkStates.moving)
        {
            WorkMoving();
        }
        if (workState == WorkStates.charging)
        {
            WorkCharging();
        }
        if (workState == WorkStates.firing)
        {
            WorkCharging();
        }
    }

    private void WorkMoving()
    {
        //if refreshRate
        //  if less than miningDistance from next target
        //      
    }

    private void WorkCharging()
    {

    }

    private void WorkFiring()
    {

    }

    #endregion

    #region State Overrides
    protected override void Working()
    {
        base.Working();


    }

    protected override void Alarmed()
    {
        base.Alarmed();

        if (detection._playerVisibility == 0)
            StartLookingAround();
        else
            _lookAround = false;
    }

    protected override void Searching()
    {
        base.Searching();

        if (detection._playerVisibility == 0)
            StartLookingAround();
        else
            _lookAround = false;

        if ((Vector3.Distance(transform.position, detection._lastPlayerGroundedPosition) < 5))
        {
            StartLookingAround();
        }
    }

    protected override void StartCharging()
    {
        if (boxDestroyed)
        {
            StartSearching();
            return;
        }

        base.StartCharging();

        firingLight.enabled = true;
        _freeze = true;
        _startTargetLocation = detection._lastPlayerPosition;
        //SFXPlayer.PlaySound("Charge", 1, 15, 30, 1, 1, false);
    }

    protected override void StartFiring()
    {
        base.StartFiring();
        
        laserLine.enabled = true;
        //FP.ParticleEffect.Play ();
        _endTargetLocation = detection._lastPlayerPosition;
        //SFXPlayer.PlaySound("Fire", 1, 15, 30, 1, 1, false);
    }

    protected override void StopFiring()
    {
        base.StopFiring();

        firingLight.enabled = false;
        laserLine.enabled = false;
        _freeze = false;
    }

    protected override void Firing()
    {
        base.Firing();

        Fire();
    }
    #endregion

    #region Laser
    private void Fire()
    {
        float t = (Time.time - combatAndStates._startedFiring) / (combatAndStates.fireDuration * 0.5f);
        //float t = (Time.time - stateMachine._startedFiring) / stateMachine.fireDuration;
        PointLaserAtTarget(Vector3.LerpUnclamped(_startTargetLocation, _endTargetLocation, t));

        RaycastHit Hit;
        if (Physics.Raycast(firingPoint.position - firingPoint.forward, firingPoint.forward, out Hit, 600, raycastShootingMask))
        {
            Vector3 LinePoint = laserLine.transform.InverseTransformPoint(Hit.point);
            laserLine.SetPosition(1, LinePoint);

            //Check tag HERE
            Shootable SH = Hit.collider.GetComponentInParent<Shootable>();
            if (SH)
                SH.Hit(_laserDPS * Time.deltaTime, name, Hit.collider.name);
            else if (Time.timeScale > 0)
                SpawnBurnDecal(Hit);
        }
    }

    private void PointLaserAtTarget(Vector3 targetPos)
    {
        //print("target = " + targetPos + ", start = " + _startTargetLocation + ", end = " + _endTargetLocation);

        float angleToPoint = movement.SignedHorAngleToTarget(targetPos);
        TurnByAngle(angleToPoint);

        AimBoxAtTarget(targetPos);
    }
    
    private void AimBoxAtTarget(Vector3 targetPos)
    {
        Vector3 targetDir = targetPos - firingPoint.position;
        float targetHeightDiff = targetDir.y;
        float targetLatDist = targetDir.FixedY(0).magnitude;
        Vector3 inLineTargetDir = (transform.forward * targetLatDist) + new Vector3(0, targetDir.y, 0);
        float desiredBoxAngle = Vector3.SignedAngle(transform.forward, inLineTargetDir, firingPoint.right);
        desiredBoxAngle = Mathf.Clamp(desiredBoxAngle, minBoxAngle, maxBoxAngle);

        Quaternion rot = new Quaternion();
        rot.eulerAngles = new Vector3(desiredBoxAngle, 0, 0);
        box.localRotation = rot;

        //TODO: factor in rotation speed?
    }

    private void SpawnBurnDecal(RaycastHit Hit)
    {
        Quaternion DecalRotation = Quaternion.LookRotation(-Hit.normal);
        GameObject LaserBurn;
        _lastDecalPoint = Hit.point + (Hit.normal * 0.001f);
        LaserBurn = Instantiate(Resources.Load<GameObject>("Prefabs/LaserBurn"), _lastDecalPoint, DecalRotation);
    }
    #endregion

    #region Head Rotation
    private void DoHeadRotation()
    {
        if (detection._playerVisibility > 0.5f)
        {
            //Turn to face the player (lerp relative to PV).
            FacePlayer();
        }
        else if (combatAndStates.state == AIState.Searching || combatAndStates.state == AIState.Alarmed)
        {
            if (_lookAround)
            {
                LookForPlayer(transform);
            }
            else
                //Turn to face the players assumed position.
                detection.head.rotation = Quaternion.RotateTowards(detection.head.rotation,
                    Quaternion.LookRotation(detection._lastPlayerPosition - detection.head.position), (90 * Time.deltaTime));
        }
        else
        {
            //Turn to face forwards.
            detection.head.rotation = Quaternion.RotateTowards(detection.head.rotation, Quaternion.LookRotation(transform.forward), (90 * Time.deltaTime));
        }
    }

    private void LookForPlayer(Transform body)
    {
        if (Time.time > _lookAroundStarted + 0.8f)
        {
            _lookAroundStarted = Time.time;
            _lookAroundDirection = body.forward;
            _lookAroundDirection += body.right * (Random.value - 0.5f) * 5f;
            _lookAroundDirection += body.up * (Random.value - 0.5f) * 0.2f;
        }
        detection.head.rotation = Quaternion.RotateTowards(detection.head.rotation, Quaternion.LookRotation(_lookAroundDirection), (90 * Time.deltaTime));
    }

    private void FacePlayer()
    {
        Quaternion targetRot = Quaternion.LookRotation(PlayerMovement.ThePlayer.MainCamera.transform.position - detection.head.position);
        detection.head.rotation = Quaternion.RotateTowards(detection.head.rotation, targetRot, (90 + (90 * detection._playerVisibility)) * Time.deltaTime);
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 targetDir = targetPos - detection.head.position;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);
        detection.head.rotation = Quaternion.RotateTowards(detection.head.rotation, targetRot, (90 * Time.deltaTime));
    }

    public void StartLookingAround()
    {
        if (_lookAround == false)
        {
            _lookAroundStarted = Time.time - 0.8f;
            _lookAround = true;
        }
    }
    #endregion

    #region Damage Functions
    public void DestroyLaserBox()
    {
        box.gameObject.SetActive(false);
        boxDestroyed = true;
    }

    public void DestroyHead()
    {
        detection.head.gameObject.SetActive(false);
        DestroyMiningDroid();
    }

    public void DestroyMiningDroid()
    {
        movement.RB.useGravity = true;
        movement.RB.constraints = RigidbodyConstraints.None;
        EFDestroy();
        Destroy(this);
    }
    #endregion
}

/* TODO:
 * - finish PointLaserAtTarget
 * - add public list of targets
 * - add working sub-states for:
 *      - moving to target (e.g. rocks)
 *      - charging to shoot at target
 *      - firing at target (endTargetLocation can be random offset?)
 */
