using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningDroid : EnemyFramework
{
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

    private bool _lookAround = false;
    private float _lookAroundStarted = -1000;
    private Vector3 _lookAroundDirection = Vector3.zero;

    public float _laserDPS = 1f;
    private bool _freeze = false;
    
    private Vector3 _startTargetLocation;
    private Vector3 _endTargetLocation;
    private Vector3 _lastDecalPoint = new Vector3();

    protected override void StateMachineUpdate()
    {
        base.StateMachineUpdate();

        DoHeadRotation();
    }

    protected override void MovementUpdate()
    {
        if (combatAndStates.state == AIState.Firing)
            return;

        if (combatAndStates.state == AIState.Charging)
            AimBoxAtTarget(_startTargetLocation);
        else
        {
            UpdateNextPoint();

            float angleToPoint = movement.SignedHorAngleToTarget(movement.NextPoint);
            TurnByAngle(angleToPoint);
            if (Utility.UnsignedDifference(angleToPoint, 0f) < movement.turnAccuracy)
                MoveTowardsTarget(movement.NextPoint);
        }
    }

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
            _lookAroundDirection += body.right * (Random.value - 0.5f) * 5;
            _lookAroundDirection += body.up * (Random.value - 0.5f) * 3;
        }
        detection.head.rotation = Quaternion.RotateTowards(detection.head.rotation, Quaternion.LookRotation(_lookAroundDirection), (90 * Time.deltaTime));
    }

    private void FacePlayer()
    {
        Quaternion targetRot = Quaternion.LookRotation(Movement.ThePlayer.MainCamera.transform.position - detection.head.position);
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

            Shootable SH = Hit.collider.GetComponentInParent<Shootable>();
            if (SH)
                SH.Hit(_laserDPS * Time.deltaTime);
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
        Quaternion DecalRotation = Quaternion.LookRotation(Hit.normal);
        GameObject LaserBurn;
        _lastDecalPoint = Hit.point + (Hit.normal * 0.001f);
        LaserBurn = Instantiate(Resources.Load<GameObject>("Prefabs/LaserBurn"), _lastDecalPoint, DecalRotation);
    }
}

/* TODO:
 * - finish PointLaserAtTarget
 * - add public list of targets
 * - add working sub-states for:
 *      - moving to target (e.g. rocks)
 *      - charging to shoot at target
 *      - firing at target (endTargetLocation can be random offset?)
 */
