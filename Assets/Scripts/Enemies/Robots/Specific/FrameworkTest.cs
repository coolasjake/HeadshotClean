using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameworkTest : EnemyFramework
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
        if (movement.ReachedPoint(transform.position, movement.NextPoint))
            movement._nextPathPoint += 1;

        //float angleToPoint = SignedHorAngleToTarget(movement.NextPoint);
        angleToPoint = SignedHorAngleToTarget(movement.NextPoint);
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

        if (Time.time > movement.LastRefresh + movement.RefreshRate)
        {
            movement.ChangeMoveTarget(testPatrolPoints[nextPatrolPoint].position);
            movement.LastRefresh = Time.time;
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
        Vector3 horTargetDir = movement.moveTarget - transform.position;
        float targetTurretAngle = Vector3.SignedAngle(transform.forward, horTargetDir, Vector3.up);

        Vector3 gunTargetDir = movement.moveTarget - gunTransform.position;
        float targetGunAngle = Vector3.SignedAngle(turretTransform.forward, gunTargetDir, turretTransform.right);

        MoveTurretToAngles(targetTurretAngle, targetGunAngle);
    }

    private void LookAtLastPlayerPos()
    {
        Vector3 horTargetDir = detection.LastPlayerPosition - transform.position;
        float targetTurretAngle = Vector3.SignedAngle(transform.forward, horTargetDir, Vector3.up);

        Vector3 gunTargetDir = detection.LastPlayerPosition - gunTransform.position;
        float targetGunAngle = Vector3.SignedAngle(turretTransform.forward, gunTargetDir, turretTransform.right);

        MoveTurretToAngles(targetTurretAngle, targetGunAngle);
    }

    private void AimTurret()
    {
        Vector3 targetDir = detection.LastPlayerPosition - transform.position;
        float targetTurretAngle = Vector3.SignedAngle(transform.forward, targetDir, Vector3.up);

        float relativeHeight = detection.LastPlayerPosition.y - firePoint.position.y;
        if (detection.LastPlayerPosition.y < firePoint.position.y)
            relativeHeight = firePoint.position.y - detection.LastPlayerPosition.y;
        float gravity = Physics.gravity.magnitude;
        float startingVel = lavaVelocity;
        turretAngle = Mathf.Asin(Mathf.Sqrt(2 * gravity * relativeHeight) / startingVel);
        turretAngle = turretAngle * (180f / Mathf.PI);

        MoveTurretToAngles(targetTurretAngle, turretAngle);
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
        GO.GetComponent<Rigidbody>().velocity = firePoint.forward * lavaVelocity;
    }

    private void OnDrawGizmos()
    {
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
