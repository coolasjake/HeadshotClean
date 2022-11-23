using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningDroid : EnemyFramework
{
    public Transform firingPoint;
    public LineRenderer laserLine;
    public Light firingLight;
    public LayerMask raycastShootingMask;

    private bool LookAround = false;
    private float LookAroundStarted = -1000;
    private Vector3 LookAroundDirection = Vector3.zero;

    public float LaserDPS = 1f;
    private bool Freeze = false;
    
    private Vector3 startTargetLocation;
    private Vector3 endTargetLocation;
    private Vector3 LastDecalPoint = new Vector3();

    protected override void StateMachineUpdate()
    {
        base.StateMachineUpdate();

        DoHeadRotation();
    }

    #region Head Rotation
    private void DoHeadRotation()
    {
        if (detection._playerVisibility > 0.5f)
        {
            //Turn to face the player (lerp relative to PV).
            FacePlayer();
        }
        else if (stateMachine.state == AIState.Searching || stateMachine.state == AIState.Alarmed)
        {
            if (LookAround)
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
        if (Time.time > LookAroundStarted + 0.8f)
        {
            LookAroundStarted = Time.time;
            LookAroundDirection = body.forward;
            LookAroundDirection += body.right * (Random.value - 0.5f) * 5;
            LookAroundDirection += body.up * (Random.value - 0.5f) * 3;
        }
        detection.head.rotation = Quaternion.RotateTowards(detection.head.rotation, Quaternion.LookRotation(LookAroundDirection), (90 * Time.deltaTime));
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
        if (LookAround == false)
        {
            LookAroundStarted = Time.time - 0.8f;
            LookAround = true;
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
            LookAround = false;
    }

    protected override void Searching()
    {
        base.Searching();

        if (detection._playerVisibility == 0)
            StartLookingAround();
        else
            LookAround = false;

        if ((Vector3.Distance(transform.position, detection._lastPlayerGroundedPosition) < 5))
        {
            StartLookingAround();
        }
    }

    protected override void StartCharging()
    {
        base.StartCharging();

        firingLight.enabled = true;
        Freeze = true;
        startTargetLocation = detection._lastPlayerPosition;
        //SFXPlayer.PlaySound("Charge", 1, 15, 30, 1, 1, false);
    }

    protected override void StartFiring()
    {
        base.StartFiring();
        
        laserLine.enabled = true;
        //FP.ParticleEffect.Play ();
        endTargetLocation = detection._lastPlayerPosition;
        //SFXPlayer.PlaySound("Fire", 1, 15, 30, 1, 1, false);
    }

    protected override void StopFiring()
    {
        base.StopFiring();

        firingLight.enabled = false;
        laserLine.enabled = false;
        Freeze = false;
    }

    protected override void Firing()
    {
        base.Firing();

        Fire();
    }
    #endregion

    private void Fire()
    {
        float t = (Time.time - stateMachine._startedFiring) / stateMachine.fireDuration;
        PointLaserAtTarget(Vector3.Lerp(startTargetLocation, endTargetLocation, t));

        RaycastHit Hit;
        if (Physics.Raycast(firingPoint.position - firingPoint.forward, firingPoint.forward, out Hit, 600, raycastShootingMask))
        {
            Vector3 LinePoint = laserLine.transform.InverseTransformPoint(Hit.point);
            laserLine.SetPosition(1, LinePoint);

            Shootable SH = Hit.collider.GetComponentInParent<Shootable>();
            if (SH)
                SH.Hit(LaserDPS * Time.deltaTime);
            else if (Time.timeScale > 0)
                SpawnBurnDecal(Hit);
        }
    }

    private void PointLaserAtTarget(Vector3 target)
    {
        float angleToPoint = movement.SignedHorAngleToTarget(target);
        TurnByAngle(angleToPoint);

        //TODO: Angle box/arms here
    }

    private void SpawnBurnDecal(RaycastHit Hit)
    {
        Quaternion DecalRotation = Quaternion.LookRotation(Hit.normal);
        GameObject LaserBurn;
        LastDecalPoint = Hit.point + (Hit.normal * 0.001f);
        LaserBurn = Instantiate(Resources.Load<GameObject>("Prefabs/LaserBurn"), LastDecalPoint, DecalRotation);
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
