using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LavaThrower : ShootingEnemy
{
    [Header("Lava Thrower")]
    [SerializeField]
    private Transform turretHorizontalTransform;
    [SerializeField]
    private Transform turretVerticalTransform;
    [SerializeField]
    private Transform firePoint;
    [SerializeField]
    private GameObject lavaProjectile;
    
    [SerializeField]
    private List<Transform> testPatrolPoints;
    [SerializeField]
    private Transform testStart;
    [SerializeField]
    private Transform testEnd;
    private int nextPatrolPoint;

    [SerializeField]
    private float turretSpeed = 30f;
    [SerializeField]
    private float lavaVelocity = 10f;

    [SerializeField]
    private float turnSpeed = 30f;
    [SerializeField]
    private float canMoveAngle = 5f;
    [SerializeField]
    private float moveSpeed = 2f;
    [SerializeField]
    private float moveForce = 2f;

    public bool showPathGizmo = true;

    public float debugAngle = 0;

    //ToDo:
    /* - Move() function, moves along path to moveTarget, using NextPoint etc
     * - Aim(), rotates head/turret to aim at player (or generic target)
     * - LaunchLava() function, launches lava projectile prefab
     * - Damage states, effect how actions (e.g. move) happen
     * - Explode(), creates lava around self and dies
     * - Die(), does not create lava
     */

    // Start is called before the first frame update
    void Start()
    {
        ShootingEnemyInitialize();
    }

    // Update is called once per frame
    void Update()
    {
        //StateMachine();
        Working();
        Move();
    }

    /// <summary> Called every update. Handles generating paths, rotating to face points and applying movement forces. </summary>
    private void Move()
    {
        //moveTarget is set by the state machine functions
        if (ReachedPoint(transform.position, moveTarget, 2f) && RB.velocity.magnitude > moveSpeed * 0.1f)
        {
            AccelerateToVelocity(Vector3.zero);
            return;
        }

        if (ReachedPoint(transform.position.FixedY(0), NextPoint.FixedY(0), navPointSize))
            _nextPathPoint += 1;

        Vector3 targetDir = NextPoint - transform.position;
        float angleToTarget = Vector3.SignedAngle(transform.forward, targetDir, transform.up);
        debugAngle = angleToTarget;
        if (angleToTarget > canMoveAngle)
        {//Rotate
            print("Rotating");
            float maxTurnThisFrame = turnSpeed * Time.deltaTime;
            float amountToTurn = Mathf.Clamp(angleToTarget, -maxTurnThisFrame, maxTurnThisFrame);
            transform.Rotate(0, amountToTurn, 0);
        }
        else
        {//Move
            Vector3 targetVel = targetDir.normalized.FixedY(0) * moveSpeed;
            AccelerateToVelocity(targetVel);
        }
    }

    private void AccelerateToVelocity(Vector3 targetVel)
    {
        Vector3 difference = targetVel.FixedY(0) - RB.velocity;
        RB.AddForce(difference.normalized * moveForce);
    }

    /// <summary> Called by Staring and Charging states. Rotates the head/turret so that a fired projectile would land near the target. </summary>
    protected override void RotateHead()
    {
        Vector3 targetDir = transform.forward;
        if (PlayerVisibility > 0.5f)
            targetDir = transform.position - Movement.ThePlayer.transform.position;

        float horizontalAngle = Vector3.SignedAngle(transform.forward, targetDir, Vector3.up);

        float currentHAngle = turretHorizontalTransform.localRotation.eulerAngles.y;
        horizontalAngle = Mathf.MoveTowardsAngle(currentHAngle, horizontalAngle, turretSpeed * Time.deltaTime);
        Quaternion newHRot = Quaternion.Euler(0, horizontalAngle, 0);
        turretHorizontalTransform.localRotation = newHRot;

        float verticalAngle = Vector3.SignedAngle(transform.forward, targetDir, Vector3.right);

        float currentVAngle = turretHorizontalTransform.localRotation.eulerAngles.x;
        verticalAngle = Mathf.MoveTowardsAngle(currentVAngle, verticalAngle, turretSpeed);
        Quaternion newVRot = Quaternion.Euler(verticalAngle, 0, 0);
        turretVerticalTransform.localRotation = newVRot;
    }
    
    /// <summary> Called by Firing state. Instantiates a lava projectile with velocity defined by turret direction. </summary>
    private void LaunchLava()
    {
        GameObject GO = Instantiate(lavaProjectile, firePoint.position, turretVerticalTransform.rotation);
        GO.GetComponent<Rigidbody>().velocity = turretVerticalTransform.forward * lavaVelocity;
    }

    /// <summary> Drop lava projectiles around body and then Die. </summary>
    private void Explode()
    {
        Die();
    }

    public override void Die()
    {

    }

    protected override void Working()
    {
        //If end of path is not near target and refresh has cooled down
        //  GetPath(target.position);
        if (ReachedPoint(transform.position, testPatrolPoints[nextPatrolPoint].position, navPointSize))
            nextPatrolPoint = (nextPatrolPoint + 1) % testPatrolPoints.Count;

        if (Time.time > LastRefresh + RefreshRate)
        {
            UpdateDestination(testPatrolPoints[nextPatrolPoint].position);
            print("Path after update: " + _path.corners.Length);
            LastRefresh = Time.time;
        }

        base.Working();
    }

    private void OnDrawGizmosSelected()
    {
    }
    
    private void OnDrawGizmos()
    {
        if (showPathGizmo)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
            Gizmos.color = Color.red;
            Vector3 targetDir = NextPoint - transform.position;
            Gizmos.DrawLine(transform.position, transform.position + targetDir);
            Gizmos.DrawSphere(NextPoint, navPointSize);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(testPatrolPoints[nextPatrolPoint].position, navPointSize);
            Gizmos.DrawLine(transform.position, testPatrolPoints[nextPatrolPoint].position);
            if (_path == null)
                return;
            if (_path.corners != null && _path.corners.Length > 1)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawSphere(_path.corners[0], navPointSize);
                Gizmos.color = Color.white;
                for (int i = 1; i < _path.corners.Length; ++i)
                {
                    Gizmos.DrawLine(_path.corners[i - 1], _path.corners[i]);
                    Gizmos.DrawSphere(_path.corners[i], navPointSize);
                }
            }
        }
    }
}
