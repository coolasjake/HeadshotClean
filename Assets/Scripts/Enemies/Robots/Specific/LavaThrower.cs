﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LavaThrower : EnemyFramework
{
    [Header("Lava Thrower")]
    [SerializeField]
    private Transform turretTransform;
    [SerializeField]
    private Transform gunTransform;
    [SerializeField]
    private Transform firePoint;
    [SerializeField]
    private GameObject lavaProjectile;

    [SerializeField]
    private float turretSpeed = 30f;
    [SerializeField]
    private float turretMaxAngle = 30f;
    [SerializeField]
    private float gunSpeed = 30f;
    [SerializeField]
    private float gunMaxAngle = 30f;
    [SerializeField]
    private float lavaVelocity = 10f;

    [SerializeField]
    private List<Transform> testPatrolPoints;
    private int nextPatrolPoint;

    public bool showPathGizmo = true;

    public float angleToPoint = 0;

    private float turretAngle = 0;

    protected override void MovementUpdate()
    {
        UpdateNextPoint();

        //float angleToPoint = SignedHorAngleToTarget(movement.NextPoint);
        angleToPoint = movement.SignedHorAngleToTarget(movement.NextPoint);
        if (Utility.UnsignedDifference(angleToPoint, 0f) < movement.turnAccuracy)
        {
            MoveTowardsTarget(movement.NextPoint);
        }
        else
        {
            AccelerateToVelocity(Vector3.zero);
            if (movement.RB.velocity.sqrMagnitude < 0.1f)
                TurnByAngle(angleToPoint);
        }
    }

    protected override void Working()
    {
        base.Working();

        if (movement.ReachedPoint(transform.position, testPatrolPoints[nextPatrolPoint].position))
        {
            nextPatrolPoint = (nextPatrolPoint + 1) % testPatrolPoints.Count;
            movement.ChangeMoveTarget(testPatrolPoints[nextPatrolPoint].position);
        }

        if (Time.time > movement._lastRefresh + movement.refreshRate)
        {
            movement.ChangeMoveTarget(testPatrolPoints[nextPatrolPoint].position);
            movement._lastRefresh = Time.time;
        }

        LookAtMoveTarget();
    }

    protected override void Searching()
    {
        base.Searching();

        LookAtLastPlayerPos();
    }

    protected override void Staring()
    {
        base.Staring();

        LookAtLastPlayerPos();
    }

    protected override void Charging()
    {
        base.Charging();

        AimTurret();
    }

    /// <summary> Called by Staring and Charging states. Rotates the head/turret so that a fired projectile would land near the target. </summary>
    private void LookAtMoveTarget()
    {
        Vector3 horTargetDir = movement._moveTarget - transform.position;
        float targetTurretAngle = Vector3.SignedAngle(transform.forward, horTargetDir, Vector3.up);

        Vector3 gunTargetDir = (movement._moveTarget + new Vector3(0, gunTransform.localPosition.y, 0)) - gunTransform.position;
        float targetGunAngle = Vector3.SignedAngle(turretTransform.forward, gunTargetDir, turretTransform.right);

        MoveTurretToAngles(targetTurretAngle, targetGunAngle);
    }

    private void LookAtLastPlayerPos()
    {
        //float targetTurretAngle = Vector3.SignedAngle(transform.forward, dirToTarget.FixedY(transform.forward.y), Vector3.up);
        //float targetGunAngle = Vector3.SignedAngle(turretTransform.forward, projectileVel, turretTransform.right);
        Vector3 horTargetDir = detection._lastPlayerPosition.FixedY(0) - transform.position.FixedY(0);
        float targetTurretAngle = Vector3.SignedAngle(transform.forward, horTargetDir, Vector3.up);

        Vector3 gunTargetDir = detection._lastPlayerPosition - gunTransform.position;
        float targetGunAngle = Vector3.SignedAngle(turretTransform.forward, gunTargetDir, turretTransform.right);

        MoveTurretToAngles(targetTurretAngle, targetGunAngle);
    }

    Vector3 debugProjVel = Vector3.zero;
    private void AimTurret()
    {
        Vector3 dirToTarget = detection._lastPlayerPosition - transform.position;
        Vector3 projectileVel = (detection._lastPlayerPosition - transform.position).normalized * lavaVelocity;
        if (fts.solve_ballistic_arc_lateral(firePoint.position, lavaVelocity, Physics.gravity.magnitude, detection._lastPlayerPosition, Vector3.zero, out projectileVel))
        {
            debugProjVel = projectileVel;

            float targetTurretAngle = Vector3.SignedAngle(transform.forward, dirToTarget.FixedY(transform.forward.y), Vector3.up);
            float targetGunAngle = Vector3.SignedAngle(turretTransform.forward, projectileVel, turretTransform.right);

            MoveTurretToAngles(targetTurretAngle, targetGunAngle);
        }
    }

    private float debugHorAngle = 0;
    private float debugVertAngle = 0;
    private void MoveTurretToAngles(float horizontal, float vertical)
    {
        debugHorAngle = horizontal;
        debugVertAngle = vertical;

        float currentTurretAngle = turretTransform.localRotation.eulerAngles.y;
        horizontal = Mathf.Clamp(horizontal, -turretMaxAngle, turretMaxAngle);
        horizontal = Mathf.MoveTowardsAngle(currentTurretAngle, horizontal, turretSpeed * Time.deltaTime);
        Quaternion newHRot = Quaternion.Euler(0, horizontal, 0);
        turretTransform.localRotation = newHRot;

        float currentGunAngle = gunTransform.localRotation.eulerAngles.x;
        vertical = Mathf.Clamp(vertical, -gunMaxAngle, gunMaxAngle);
        vertical = Mathf.MoveTowardsAngle(currentGunAngle, vertical, gunSpeed * Time.deltaTime);
        Quaternion newVRot = Quaternion.Euler(vertical, 0, 0);
        gunTransform.localRotation = newVRot;
    }

    protected override void StartFiring()
    {
        base.StartFiring();
        LaunchLava();
    }

    /// <summary> Called by Firing state. Instantiates a lava projectile with velocity defined by turret direction. </summary>
    private void LaunchLava()
    {
        GameObject GO = Instantiate(lavaProjectile, firePoint.position, firePoint.rotation);
        Vector3 projectileVel = firePoint.forward * lavaVelocity;
        if (fts.solve_ballistic_arc_lateral(firePoint.position, lavaVelocity, Physics.gravity.magnitude, detection._lastPlayerPosition, Vector3.zero, out projectileVel))
        {
            GO.GetComponent<Rigidbody>().velocity = projectileVel;
        }
    }

    private void OnDrawGizmos()
    {
        //Draw turret and gun desired angles
        Vector3 starting = turretTransform.forward;
        Quaternion rotation = new Quaternion();
        rotation = Quaternion.AngleAxis(debugHorAngle, -Vector3.up);
        Vector3 hor = (rotation * starting);
        rotation = Quaternion.AngleAxis(debugVertAngle, gunTransform.right);
        Vector3 vert = (rotation * starting);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(turretTransform.position, turretTransform.position + hor * 4f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(turretTransform.position, turretTransform.position + vert * 3f);

        //Draw firing point direction and projectile velocity
        Gizmos.color = Color.red;
        Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * 3f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(firePoint.position, firePoint.position + debugProjVel.normalized * 4f);

        if (showPathGizmo)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(testPatrolPoints[nextPatrolPoint].position, movement.navPointSize);
            Gizmos.DrawLine(transform.position, testPatrolPoints[nextPatrolPoint].position);
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
}
