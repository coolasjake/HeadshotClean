using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class Gravity : PlayerAbility
{
    #region Auto References
    private Rigidbody RB;
    private Movement PM;
    private GravityRep UIGyroscope;
    private GravityRep UINormalGravity;
    /// <summary> Reference to the object that shows where gravity has been shifted to. </summary>
	private GravityCircle circle;
    #endregion

    public GameObject gravityCirclePrefab;
    /// <summary> Reference to an AudioSource which plays sounds for gravity abilities. </summary>
	public AudioSource GravitySFXPlayer;

    private float defaultGravityMagnitude = 9.8f;

    private Vector3 _customGravity;
    private Vector3 _customGravityDirection;
    private float _customGravityMagnitude;
    private Vector3 CustomGravity
    {
        get { return _customGravity; }
        set
        {
            _customGravity = value;
            _customGravityDirection = _customGravity.normalized;
            _customGravityMagnitude = _customGravity.magnitude;
            UIGyroscope.Down = _customGravity;
        }
    }
    private Vector3 CustomGravityDir
    {
        get { return _customGravityDirection; }
    }
    private float CustomGravityMag
    {
        get { return _customGravityMagnitude; }
    }

    public enum GravityType
    {
        Normal,     //Gravity is set by the RigidBody, and the resource can regenerate.
        Unaligned,  //Gravity was set by the camera direction, and will change on a collision.
        Aligned     //Gravity is aligned to a surface (or to normal, but with a different magnitude).
    }
    public GravityType gravityType = GravityType.Normal;

    /// <summary> Last point the player shifted gravity from. </summary>
    private Vector3 shiftPoint;
    /// <summary> . </summary>
    private RaycastHit targetWall;



    #region Inspector Settings
    [Header("Gravity Ability Settings")]
    /// <summary> Aiming at a wall closer than this distance will align gravity to the normal of the hit, further will align gravity to the aim direction. </summary>
    [Tooltip("Aiming at a wall closer than this distance will align gravity to the normal of the hit, further will align gravity to the aim direction.")]
    public float flyDistance = 2f;
    /// <summary> Time between gravity changes (in the same direction) where it will double the gravity instead of resetting. </summary>
    [Tooltip("Time between gravity changes (in the same direction) where it will double the gravity instead of resetting.")]
    public float superGravityWindow = 0.2f;
    /// <summary> Maximum number of times the player can double their gravity. </summary>
    [Tooltip("Maximum number of times the player can double their gravity.")]
    public int maxGravityMultipliers = 3;
    /// <summary> The minimum velocity to play impact sounds at (or destroy robots on impact). </summary>
    [Tooltip("The minimum velocity to play impact sounds at (or destroy robots on impact).")]
    public float impactVelocity = 10;
    #endregion

    #region ExperimentalSettings
    [Header("Experimental Settings")]
    /// <summary> How long it takes to reset gravity to world default when holding the gravity down button [C]. </summary>
    public float holdTimeToReset = 1f;
    /// <summary> When true sensitivity is reduced after gravity has changed until the player lands. </summary>
    public bool reduceSenseWhileFlying = false;
    /// <summary> Amount to reduce sensitivity by when flying if setting is on. </summary>
    [Range(0f, 1)]
    public float flyingSenseMultiplier = 0.5f;
    /// <summary> When true sensitivity is reduced after gravity has changed until the player lands. </summary>
    public bool reduceControlWhileFlying = false;
    /// <summary> Amount to reduce sensitivity by when flying if setting is on. </summary>
    [Range(0f, 1)]
    public float flyingAirControlMultiplier = 0.5f;

    public bool doCollisionAutoCamera = true;
    public bool doFlyingAutoCamera = false;
    public AnimationCurve autoCameraSpeedByAngle = new AnimationCurve(new Keyframe[1] { new Keyframe(0, 90) });
    public AnimationCurve autoCameraSpeedForFlying = new AnimationCurve(new Keyframe[1] { new Keyframe(0, 90) });
    [Tooltip("Player mouse input required to cancel the auto camera. 0 to disable.")]
    public float stopAutoCameraThreshold = 0f;
    public float maxAutoCameraDur = 2f;
    #endregion

    #region Unity Events
    // Use this for initialization
    void Start()
    {
        //Find player components
        RB = GetComponent<Rigidbody>();
        PM = GetComponent<Movement>();
        PM.collisionEvent += GravityCollision;

        circle = Instantiate(gravityCirclePrefab).GetComponent<GravityCircle>();

        if (GravitySFXPlayer == null)
            GravitySFXPlayer = GetComponentInChildren<AudioSource>();

        //Find UI elements
        GameObject GravityUI = UIManager.stat.LoadOrGetUI("Gravity");
        //meter = GravityUI.GetComponentInChildren<ResourceMeter>();
        //dragonMeter = GravityUI.GetComponentInChildren<BarDisplay>();
        UIGyroscope = GetComponentInChildren<GravityRep>();
        UINormalGravity = GetComponentsInChildren<GravityRep>()[1];

        //Setup starting values
        defaultGravityMagnitude = Physics.gravity.magnitude;
        UINormalGravity.Down = -Vector3.up * defaultGravityMagnitude;
        CustomGravity = Physics.gravity;
        ResetToWorldGravity();
    }

    // Update is called once per frame
    void Update()
    {
        InputLogic();
        CustomGravityTick();
    }
    #endregion

    private void InputLogic()
    {
        //If 'Reset' or 'ChangeGravity' are pressed, change gravity (which checks if it is needed).
        if (Input.GetButtonDown("GravityDown"))
        {
            StartResetTimer();
        }
        else if (Input.GetButtonUp("GravityDown"))
        {
            if (CheckResetTimer())
            {
                GravityReset();
            }
            else
            {
                DownShift();
            }
        }

        if (Input.GetButtonDown("GravityNormal"))
        {
            TrajectoryLineStart();
        }
        else if (Input.GetButtonUp("GravityNormal"))
        {
            TrajectoryLineEnd();
            ForwardShift();
        }

        if (Input.GetButtonDown("TimeSlow"))
        {
            BulletTimeStart();
        }
        else if (Input.GetButtonUp("TimeSlow"))
        {
            BulletTimeEnd();
        }

        if (Input.GetButtonDown("Crouch"))
        {
            MoonGravityModifier = true;
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            MoonGravityModifier = false;
        }

        if (Input.GetButtonDown("Jump") && Time.time > PM._LastGrounded + 0.1f)
        {
            GravityJumpStart();
        }
        else if (Input.GetButtonUp("Jump") && magBeforeDoubleJump > 0)
        {
            GravityJumpEnd();
        }
    }

    #region Abilities
    /// <summary> The magnitude of gravity before a GravityJump activation (so it can be returned on release). </summary>
    private float magBeforeDoubleJump = 0;
    /// <summary> Save current gravity and then set gravity to zero. </summary>
    private void GravityJumpStart()
    {
        magBeforeDoubleJump = CustomGravityMag;
        ChangeGravityMagnitude(0.001f);
    }

    /// <summary> Restore gravity to the value saved when starting the jump. </summary>
    private void GravityJumpEnd()
    {
        ChangeGravityMagnitude(magBeforeDoubleJump / defaultGravityMagnitude);
        magBeforeDoubleJump = 0;
    }

    /// <summary> The Time.time value of the last time the ForwardShift ability was used (for SuperGravity). </summary>
    private float forwardLastActivation = float.NegativeInfinity;
    /// <summary> Calls ContextualShift with the forward direction of the camera. </summary>
    private void ForwardShift()
    {
        if (Time.time < forwardLastActivation + superGravityWindow)
        {
            SuperGravity();
        }
        else
        {
            ContextualShift(PM.MainCamera.transform.forward);
        }
        forwardLastActivation = Time.time;
    }

    /// <summary> The Time.time value of the last time the DownShift ability was used (for SuperGravity). </summary>
    private float downLastActivation = float.NegativeInfinity;
    /// <summary> Calls ContextualShift with the down direction of the camera. </summary>
    private void DownShift()
    {
        if (Time.time < forwardLastActivation + superGravityWindow)
        {
            SuperGravity();
        }
        else
        {
            ContextualShift(-PM.MainCamera.transform.up);
        }
        forwardLastActivation = Time.time;
    }


    /// <summary> Do a raycast with the specified direction (usually camera-forward or camera-down)
    /// and call either GravityShift or FlyingGravity based on the distance to the hit surface. </summary>
    private void ContextualShift(Vector3 direction)
    {
        currentGravityMultipliers = 0;

        if (Physics.Raycast(PM.MainCamera.transform.position, direction, out targetWall))
        {
            RaycastHit Hit;
            //If the raycast hits a surface less than double the flyDistance away, do a second raycast to see if there is a matching surface
            //less than the fly distance away in the direction gravity would be changed to. If so, align gravity with the surface.
            if (targetWall.distance < flyDistance * 2f && Physics.Raycast(transform.position, targetWall.normal * -1, out Hit, flyDistance))
            {
                if (Hit.distance < flyDistance && VectorsAreSimilar(Hit.normal, targetWall.normal))
                {
                    GravityShift();
                    return;
                }
            }

            //If the surface is too far away, call FlyingGravity.
            FlyingGravity();
            return;
        }

        //If no surface was found at all call FlyingGravity, but specify that the targetWall data will not be relevant.
        FlyingGravity(false);
    }

    /// <summary> Change gravity to the normal of the target surface. </summary>
    private void GravityShift()
    {
        if (ShiftGravityDirection(1, targetWall.normal * -1))
        {
            gravityType = GravityType.Aligned;
            StartCollisionMoveCameraCoroutine();
        }
    }

    /// <summary> Change gravity to the direction of the target surface, and cause the next collision to perform a GravityShift. </summary>
    private void FlyingGravity(bool foundTarget = true)
    {
        if (ShiftGravityDirection(1, PM.MainCamera.transform.forward))
        {
            if (!foundTarget)
                circle.Hide();
            gravityType = GravityType.Unaligned;
            StartFallingMoveCameraCoroutine();
            ReduceSenseAndControl();
        }
    }

    [Min(0)]
    private int currentGravityMultipliers = 0;
    /// <summary> Multiply the current gravity magnitude by 2, up to the maximum number of multiplications. </summary>
    private void SuperGravity()
    {
        if (currentGravityMultipliers < maxGravityMultipliers)
        {
            currentGravityMultipliers += 1;
            float newMultiplier = 2f * currentGravityMultipliers;
            if (MoonGravityModifier)
                newMultiplier = 0.5f / currentGravityMultipliers;
            ChangeGravityMagnitude(newMultiplier);
        }
    }

    /// <summary> Slow time. </summary>
    private void BulletTimeStart()
    {

    }

    /// <summary> Slow time. </summary>
    private void BulletTimeEnd()
    {

    }

    /// <summary> Begin simulating the trajectory line. </summary>
    private void TrajectoryLineStart()
    {

    }

    /// <summary> Stop simulating the trajectory line. </summary>
    private void TrajectoryLineEnd()
    {

    }

    /// <summary> Calculate the path of the player with the expected gravity and draw it in the game. </summary>
    private void TrajectoryLine(Vector3 expectedGravity)
    {

    }

    private bool _moonGravityModifier = false;

    private bool MoonGravityModifier
    {
        get { return _moonGravityModifier; }
        set
        {
            _moonGravityModifier = value;
        }
    }

    /// <summary> Reset gravity to the default physics gravity of the world. </summary>
    private void GravityReset()
    {
        ResetToWorldGravity();
    }

    private void StartResetTimer()
    {
        _resetPressedTime = Time.time;
    }

    private float _resetPressedTime = float.NegativeInfinity;
    private bool CheckResetTimer()
    {
        return Time.time > _resetPressedTime + holdTimeToReset;
    }
    #endregion

    #region Gravity-Change Functions

    /// <summary> Change only the magnitude of the current gravity. Uses no resource. </summary>
    private void ChangeGravityMagnitude(float NewMultiplier)
    {
        Vector3 NewGravity = CustomGravityDir * defaultGravityMagnitude * NewMultiplier;

        if (VectorsAreSimilar(NewGravity, Physics.gravity))
        {
            //If the new gravity is less than 10% different from the normal gravity, set gravity to normal.
            ResetToWorldGravity();
        }
        else
        {
            RB.useGravity = false;
            CustomGravity = NewGravity;
            if (gravityType == GravityType.Normal)
                gravityType = GravityType.Aligned;
        }
    }

    /// <summary> Shift gravity in the given direction, and apply the given GravityMultiplier to the force.</summary>
    private bool ShiftGravityDirection(float GravityMultiplier, Vector3 Direction)
    {
        Vector3 NewGravity = Direction * defaultGravityMagnitude * GravityMultiplier;

        if (NewGravity == CustomGravity)
        {
            return false;
        }
        else if (VectorsAreSimilar(NewGravity, Physics.gravity))
        {
            //If the new gravity is less than 10% different from the normal gravity, set gravity to normal.
            return ResetToWorldGravity();
        }
        else if (VectorsAreSimilar(NewGravity, CustomGravity))
        {
            return false;
        }
        else //Vectors ARE similar
        {
            RB.useGravity = false;
            CustomGravity = NewGravity;
            IntuitiveSnapRotation();
            circle.Shift(targetWall.point - CustomGravityDir * 0.01f, Quaternion.LookRotation(transform.forward, transform.up));
            shiftPoint = transform.position;
            return true;
        }
    }

    /// <summary> Change gravity to the default for the scene. </summary>
	private bool ResetToWorldGravity()
    {
        if (gravityType != GravityType.Normal)
        {
            gravityType = GravityType.Normal;
            RB.useGravity = true;
            CustomGravity = Physics.gravity;

            IntuitiveSnapRotation();
            return true;
        }
        return false;
    }

    /// <summary> Change the rotation of the players body so that the 'feet' are pointing 'down' relative to the current gravity direction,
    /// and keep the facing (body-y / camera-x rot) as close to the original as manageable.
    /// NOTE: (accuracy is usually impossible since the position of the camera moves, and players focus is often different from their aim). </summary>
    private void IntuitiveSnapRotation()
    {
        CustomIntuitiveSnapRotation(CustomGravity);
    }

    private void CustomIntuitiveSnapRotation(Vector3 direction)
    {
        Quaternion CameraPreRotation = PM.MainCamera.transform.rotation;
        Vector3 OriginalFacing = PM.MainCamera.transform.forward; //Remember that forward is down (the feet of the player) to let LookRotation work.

        //Rotate the players 'body'.
        transform.rotation = Quaternion.LookRotation(direction, GetComponentInChildren<GravityReference>().transform.right);
        transform.rotation = Quaternion.LookRotation(direction, GetComponentInChildren<GravityReference>().transform.forward);
        Quaternion NewRot = new Quaternion();
        NewRot.eulerAngles = new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
        transform.localRotation = NewRot;

        //Calculate the angle difference between the two rotations, then save the 'number of full rotations' it represents.
        float Signed = Vector3.SignedAngle(OriginalFacing, PM.MainCamera.transform.forward, transform.right);
        PM._CameraAngle -= Signed;
        PM.MainCamera.transform.rotation = CameraPreRotation;
    }

    /// <summary> Apply the gravity force (usually CurrentGravityMagnitude) to the player. </summary>
    private void CustomGravityTick()
    {
        //Apply the gravity force (so long as normal gravity isn't enabled).
        if (gravityType != GravityType.Normal)
            RB.velocity += CustomGravity * Time.deltaTime;
    }
    #endregion

    #region Collision Functions
    private void GravityCollision(Collision col)
    {
        RestoreSenseAndControl();

        float velocityOfImpact = col.impulse.magnitude;
        if (velocityOfImpact > impactVelocity)
        {

            if (col.gameObject.CompareTag("Softwall"))
            {
                //Play soft wall sounds and do 'minijump'
                RB.velocity += -transform.forward * col.impulse.magnitude * 0.2f;
            }
            else
            {
                PM.ImpactEffect.Play();
                PM.SFXPlayer.PlaySound("Impact");
                //ReachedImpactVelocity = false;
                BaseEnemy Enemy = col.transform.GetComponentInParent<BaseEnemy>();
                if (Enemy)
                {
                    AchievementTracker.StompKills += 1;
                    AchievementTracker.EnemyDied();
                    Enemy.Die();
                }
            }
        }
        else
        {
            //TODO: Play softer landing sound/effects
        }

        //If Gravity is unaligned and shift is not being held AND
        //The distance to the target is less than the flyDistance, or the original distance to the target is less than double the flyDistance OR
        //The distance from the target point is less than the distance from the starting point.
        if (gravityType == GravityType.Unaligned &&
            ((Vector3.Distance(targetWall.point, transform.position) < flyDistance /*|| Vector3.Distance(targetWall.point, shiftPoint) < flyDistance * 2*/) ||
            (Vector3.Distance(shiftPoint, transform.position) > Vector3.Distance(targetWall.point, transform.position))))
        {
            ShiftGravityDirection(CustomGravityMag / defaultGravityMagnitude, targetWall.normal * -1);
            StartCollisionMoveCameraCoroutine();
        }
    }
    #endregion

    #region Movement Interactions
    private Coroutine moveCameraCoroutine;

    private void StartCollisionMoveCameraCoroutine()
    {
        if (!doCollisionAutoCamera)
            return;
        if (moveCameraCoroutine != null)
            StopCoroutine(moveCameraCoroutine);
        moveCameraCoroutine = StartCoroutine(MoveCameraUp());
    }

    private void StartFallingMoveCameraCoroutine()
    {
        if (!doFlyingAutoCamera)
            return;
        if (moveCameraCoroutine != null)
            StopCoroutine(moveCameraCoroutine);
        if (CustomGravityMag <= 0)
            return;
        float expectedFallTime = Mathf.Sqrt((2f * (targetWall.distance - PM.playerSphereSize)) / CustomGravityMag);
        if (expectedFallTime <= 0)
            return;
        moveCameraCoroutine = StartCoroutine(MoveCameraUp(false, expectedFallTime));
    }

    private IEnumerator MoveCameraUp(bool collisionVersion = true, float expectedDuration = 1)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        float startTime = Time.time;
        float angleLastFrame = PM._CameraAngle;
        float fallingVersionSpeed = Utility.UnsignedDifference(PM._CameraAngle, 0) / expectedDuration;
        while (PM._CameraAngle.FurtherFromZero(0.1f))
        {
            if (Time.time > startTime + Mathf.Max(maxAutoCameraDur, expectedDuration))
                break; //Break if the routine has been running for too long

            if (stopAutoCameraThreshold > 0)
            {
                float frameDiff = Utility.UnsignedDifference(PM._CameraAngle, angleLastFrame);
                print(frameDiff / Time.deltaTime);
                if (frameDiff / Time.deltaTime > stopAutoCameraThreshold)
                    break; //Break if the player manually moved the camera too much last frame
            }


            float toMove = fallingVersionSpeed;
            if (collisionVersion)
                toMove = autoCameraSpeedByAngle.Evaluate(Utility.UnsignedDifference(PM._CameraAngle, 0));
            else
                toMove = autoCameraSpeedForFlying.Evaluate(Time.time - startTime);

            PM._CameraAngle = Mathf.MoveTowards(PM._CameraAngle, 0, toMove * Time.deltaTime);
            angleLastFrame = PM._CameraAngle;
            yield return wait;
        }
    }

    private void ReduceSenseAndControl()
    {
        if (reduceSenseWhileFlying)
            PM.SensitivityMultiplier = flyingSenseMultiplier;
        if (reduceControlWhileFlying)
            PM.AirControlMultiplier = flyingAirControlMultiplier;
    }

    private void RestoreSenseAndControl()
    {
        PM.SensitivityMultiplier = 1;
        PM.AirControlMultiplier = 1;
    }
    #endregion

    #region Helper Functions
    private bool VectorsAreSimilar(Vector3 vector1, Vector3 vector2)
    {
        return (vector1 - vector2).magnitude < 0.1f;
    }
    #endregion
}


/* TO DO:
 * - max duration for gravity jump
 * - button for slow motion while in mid air
 * - predicted trajectory line
 * - FINISH hold to reset (needs to not be overriden by shift on release)
 * - Make objects that disable gravity (to and/or on collisions)
 * - Object that gives/sets gravity change charges
 * - System for enabling/disabling abiliy parts (list of structs?)
 * - Object that disables parts of ability
 * 
 *>> settings for:
 * - maximum shift distance
 * - cooldown between shifts
 * - limited number of shifts (with option to reset/increase)
 * 
 * Done:
 * - hold button to reset
 * - automatic camera pan after changing gravity
 * - option for disabling or reducing movement after gravity shift
 * - option for lowering sensitivity after gravity shift
 */
