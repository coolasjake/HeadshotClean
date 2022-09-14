using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class Gravity : MonoBehaviour//PlayerAbility
{
    #region Auto References
    private Rigidbody RB;
    private Movement PM;
    private GravityRep UIGyroscope;
    private GravityRep UINormalGravity;
    /// <summary> Reference to the object that shows where gravity has been shifted to. </summary>
	private GravityCircle circle;
    private ResourceMeter gravJumpMeter;
    #endregion

    #region Script Variables
    private float defaultGravityMagnitude = 9.8f;
    private float defaultFixedTimeInterval;

    private float lastGrounded = float.NegativeInfinity;
    private float tempClamp = 180;

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
    private GravityType gravityType = GravityType.Normal;

    /// <summary> Last point the player shifted gravity from. </summary>
    private Vector3 shiftPoint;
    /// <summary> . </summary>
    private RaycastHit targetWall;
    #endregion

    #region Inspector
    #region Manual References
    public GameObject gravityCirclePrefab;
    public GameObject abilityMeterPrefab;
    /// <summary> Reference to an AudioSource which plays sounds for gravity abilities. </summary>
	public AudioSource GravitySFXPlayer;
    #endregion

    #region AbilitySettings

    public enum GravityAbility
    {
        GravityJump,
        GravityShift,
        FlyingGravity,
        SuperGravity,
        TimeSlow,
        TrajectoryLine,
        DownShift,
        GravityReset,
        MoonGravity,
        ForceTransfer,
    }
    public enum AbilityStatus
    {
        Enabled,
        Disabled,
        Cooldown,
        LimitedUses
    }
    [System.Serializable]
    public class AbilitySettings
    {
        [SerializeField]
        private string name;
        [SerializeField]
        private GravityAbility ability;
        [SerializeField]
        private AbilityStatus status = AbilityStatus.Enabled;
        [SerializeField]
        private float settingOne;

        private float hiddenVariable;

        public AbilitySettings(GravityAbility Ability)
        {
            ability = Ability;
            name = ability.ToString();
        }

        public AbilitySettings(GravityAbility Ability, AbilityStatus Status)
        {
            ability = Ability;
            status = Status;
            name = ability.ToString();
        }

        public GravityAbility Ability
        {
            get { return ability; }
        }

        public void SetStatus(AbilityStatus Status)
        {
            status = Status;
        }

        public void SetAsCooldown(float CooldownDuration)
        {
            status = AbilityStatus.Cooldown;
            settingOne = CooldownDuration;
        }

        public void SetAsLimitedUses(float NumberOfUses)
        {
            status = AbilityStatus.LimitedUses;
            settingOne = NumberOfUses;
            hiddenVariable = settingOne;
        }

        public bool TryUse()
        {
            switch (status)
            {
                case AbilityStatus.Enabled:
                    return true;
                case AbilityStatus.Disabled:
                    return false;
                case AbilityStatus.Cooldown:
                    if (Time.time > hiddenVariable + settingOne)
                    {
                        hiddenVariable = Time.time;
                        return true;
                    }
                    return false;
                case AbilityStatus.LimitedUses:
                    if (hiddenVariable > 0)
                    {
                        hiddenVariable -= 1;
                        return true;
                    }
                    return false;
            }
            return false;
        }
    }

    [Header("Ability List:")]
    public List<AbilitySettings> abilitySettings = new List<AbilitySettings> {
        new AbilitySettings((GravityAbility)0), new AbilitySettings((GravityAbility)1), new AbilitySettings((GravityAbility)2),
        new AbilitySettings((GravityAbility)3), new AbilitySettings((GravityAbility)4), new AbilitySettings((GravityAbility)5),
        new AbilitySettings((GravityAbility)6), new AbilitySettings((GravityAbility)7), new AbilitySettings((GravityAbility)8),
        new AbilitySettings((GravityAbility)9)
    };

    private Dictionary<GravityAbility, int> abilitiesDict = new Dictionary<GravityAbility, int>();

    private void SetupAbilitySettings()
    {
        for (int i = 0; i < abilitySettings.Count; ++i)
        {
            if (abilitiesDict.ContainsKey(abilitySettings[i].Ability))
            {
                abilitySettings.RemoveAt(i);
                --i;
            }
            else
            {
                abilitiesDict.Add(abilitySettings[i].Ability, i);
            }
        }

        foreach (GravityAbility ability in System.Enum.GetValues(typeof(GravityAbility)))
        {
            if (!abilitiesDict.ContainsKey(ability))
            {
                abilitySettings.Add(new AbilitySettings(ability, AbilityStatus.Disabled));
                abilitiesDict.Add(ability, abilitySettings.Count - 1);
            }
        }

        _gravJumpCharge = gravityJumpMaxCharge;
    }

    private AbilitySettings GetAbility(GravityAbility ability)
    {
        //TODO needs error checking HERE
        return abilitySettings[abilitiesDict[ability]];
    }

    public void SetAbilityStatus(GravityAbility ability, AbilityStatus status)
    {
        GetAbility(ability).SetStatus(status);
    }
    #endregion

    #region Settings
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
    /// <summary> Maximum duration of the Gravity Jump when the charge is full. </summary>
    [Tooltip("Maximum duration of the Gravity Jump when the charge is full.")]
    public float gravityJumpMaxCharge = 3f;
    /// <summary> Time it takes for the Gravity Jump charge to go from empty to full. </summary>
    [Tooltip("Time it takes for the Gravity Jump charge to go from empty to full.")]
    public float gravityJumpRegenTime = 3f;
    /// <summary> Pause before Gravity Jump charge starts regenerating after it is fully depleted. </summary>
    [Tooltip("Pause before Gravity Jump charge starts regenerating after it is fully depleted.")]
    public float gravJumpRegenDelay = 0.5f;
    /// <summary> The minimum velocity to play impact sounds at (or destroy robots on impact). </summary>
    [Tooltip("The minimum velocity to play impact sounds at (or destroy robots on impact).")]
    public float impactVelocity = 10;
    /// <summary> If true the player can only GravityShift/FlyingGravity to surfaces with the correct Tag. </summary>
    [Tooltip("If true the player can only GravityShift/FlyingGravity to surfaces with the correct Tag.")]
    public bool limitedGravityChanging = true;
    /// <summary> The Tag of objects the player is allowed to shift gravity towards (if limitedGravityChanging is on). </summary>
    [Tooltip("If true the player can only GravityShift/FlyingGravity to surfaces with the correct Tag.")]
    public string gravChangeEnabledTag = "CanShiftTo";
    #endregion

    #region ExperimentalSettings
    [Header("Experimental Settings")]
    /// <summary> How long it takes to reset gravity to world default when holding the gravity down button [C]. </summary>
    public float holdTimeToReset = 0.5f;
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
    public bool onlyStopForDown = true;
    public float maxAutoCameraDur = 2f;

    [Header("Time Slow")]
    public bool usedFixedSlowFactor = true;
    /// <summary> How much time is slowed at different speeds ('Time' = speed, 'Value' = factor). </summary>
    [Tooltip("How much time is slowed at different speeds ('Time' = speed, 'Value' = factor).")]
    public AnimationCurve timeSlowFactor = new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0.8f), new Keyframe(19.6f, 0.33f) });
    /// <summary> How much time is slowed at different speeds ('Time' = speed, 'Value' = factor). </summary>
    [Tooltip("How much time is slowed.")]
    [Range(0.1f, 1f)]
    public float fixedTimeSlowFactor = 0.5f;
    #endregion
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
        gravJumpMeter = Instantiate(abilityMeterPrefab, UIManager.stat.canvas.transform).GetComponent<ResourceMeter>();

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
        defaultFixedTimeInterval = Time.fixedDeltaTime;
        UINormalGravity.Down = -Vector3.up * defaultGravityMagnitude;
        CustomGravity = Physics.gravity;
        ResetToWorldGravity();

        SetupAbilitySettings();
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
        
        if (PM._Grounded)
            lastGrounded = Time.time;

        if (Input.GetButtonDown("Jump") && Time.time > lastGrounded)
        {
            GravityJumpStart();
        }
        else if (Input.GetButtonUp("Jump") && magBeforeGravJump > 0)
        {
            GravityJumpEnd();
        }
    }

    #region Abilities

    /// <summary> The magnitude of gravity before a GravityJump activation (so it can be returned on release). Must be 0 if not in use. </summary>
    private float magBeforeGravJump = 0;
    private float _gravJumpCharge = 0;
    private Coroutine _gravJumpCoroutine;
    /// <summary> Save current gravity and then set gravity to zero. </summary>
    private void GravityJumpStart()
    {
        if (GetAbility(GravityAbility.GravityJump).TryUse() && _gravJumpCharge > 0)
        {
            magBeforeGravJump = CustomGravityMag;
            ChangeGravityMagnitude(0.001f);

            if (_gravJumpCoroutine == null)
                _gravJumpCoroutine = StartCoroutine(GravityJumpChargeManagement());
        }
    }

    /// <summary> Restore gravity to the value saved when starting the jump. </summary>
    private void GravityJumpEnd()
    {
        ChangeGravityMagnitude(magBeforeGravJump / defaultGravityMagnitude);
        magBeforeGravJump = 0;
    }

    private IEnumerator GravityJumpChargeManagement()
    {
        float _lastUse = Time.time;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (magBeforeGravJump != 0 || _gravJumpCharge < gravityJumpMaxCharge)
        {
            yield return wait;

            if (magBeforeGravJump != 0) //If using ability reduce charge
            {
                _lastUse = Time.time;
                _gravJumpCharge -= Time.deltaTime;
                if (_gravJumpCharge <= 0)
                {
                    GravityJumpEnd();
                    _gravJumpCharge = 0;
                }
            }
            else if (Time.time > _lastUse + gravJumpRegenDelay)
            {
                _gravJumpCharge += (gravityJumpMaxCharge / gravityJumpRegenTime) * Time.deltaTime;
            }

            gravJumpMeter.ChangeValue(_gravJumpCharge / gravityJumpMaxCharge);
        }
        _gravJumpCharge = gravityJumpMaxCharge;
        _gravJumpCoroutine = null;
    }

    /// <summary> The Time.time value of the last time the ForwardShift ability was used (for SuperGravity). </summary>
    private float forwardLastActivation = float.NegativeInfinity;
    /// <summary> Calls ContextualShift with the forward direction of the camera. </summary>
    private void ForwardShift()
    {
        if (Time.time < forwardLastActivation + superGravityWindow)
        {
            if (SuperGravity())
            {
                forwardLastActivation = Time.time;
            }
        }
        else
        {
            if (DoShiftFromContext(PM.MainCamera.transform.forward))
            {
                forwardLastActivation = Time.time;
            }
            else
            {
                print("Cancelling because target is not valid.");
            }
        }
    }

    /// <summary> The Time.time value of the last time the DownShift ability was used (for SuperGravity). </summary>
    private float downLastActivation = float.NegativeInfinity;
    /// <summary> Calls ContextualShift with the down direction of the camera. </summary>
    private void DownShift()
    {
        if (!GetAbility(GravityAbility.DownShift).TryUse())
            return;

        if (Time.time < downLastActivation + superGravityWindow)
        {
            if (SuperGravity())
            {
                downLastActivation = Time.time;
            }
        }
        else
        {
            if (DoShiftFromContext(-PM.MainCamera.transform.up))
            {
                downLastActivation = Time.time;
            }
            else
            {
                print("Cancelling because target is not valid.");
            }
        }
    }


    /// <summary> Do a raycast with the specified direction (usually camera-forward or camera-down)
    /// and call either GravityShift or FlyingGravity based on the distance to the hit surface. </summary>
    private bool DoShiftFromContext(Vector3 direction)
    {
        RaycastHit tempTargetWall;
        if (Physics.Raycast(PM.MainCamera.transform.position, direction, out tempTargetWall))
        {
            if (limitedGravityChanging && tempTargetWall.transform.CompareTag(gravChangeEnabledTag) == false)
                return false;

            RaycastHit Hit;
            //If the raycast hits a surface less than double the flyDistance away, do a second raycast to see if there is a matching surface
            //less than the fly distance away in the direction gravity would be changed to. If so, align gravity with the surface.
            if (tempTargetWall.distance < flyDistance * 2f && Physics.Raycast(transform.position, tempTargetWall.normal * -1, out Hit, flyDistance))
            {
                if (Hit.distance < flyDistance && VectorsAreSimilar(Hit.normal, tempTargetWall.normal))
                {
                    return GravityShift(tempTargetWall);
                }
            }

            //If the surface is too far away, call FlyingGravity.
            return FlyingGravity(direction, tempTargetWall);
        }

        if (limitedGravityChanging)
            return false;

        //If no surface was found at all call FlyingGravity, but specify that the targetWall data will not be relevant.
        return FlyingGravity(direction, tempTargetWall, false);
    }

    /// <summary> Change gravity to the normal of the target surface. </summary>
    private bool GravityShift(RaycastHit newTargetWall)
    {
        if (!GetAbility(GravityAbility.GravityShift).TryUse())
        {
            print("Cancelling because GravityShift (short distance shift) is disabled.");
            return false;
        }

        if (ShiftGravityDirection(1, newTargetWall.normal * -1))
        {
            targetWall = newTargetWall;
            circle.Shift(targetWall.point - CustomGravityDir * 0.01f, Quaternion.LookRotation(transform.forward, transform.up));
            gravityType = GravityType.Aligned;
            StartCollisionMoveCameraCoroutine();
        }
        return true;
    }

    /// <summary> Change gravity to the direction of the target surface, and cause the next collision to perform a GravityShift. </summary>
    private bool FlyingGravity(Vector3 direction, RaycastHit newTargetWall, bool foundTarget = true)
    {
        if (!GetAbility(GravityAbility.FlyingGravity).TryUse())
        {
            print("Cancelling because flying (long distance shift) is disabled.");
            return false;
        }

        if (ShiftGravityDirection(1, direction))
        {
            targetWall = newTargetWall;

            if (!foundTarget)
                circle.Hide();
            else
                circle.Shift(targetWall.point - CustomGravityDir * 0.01f, Quaternion.LookRotation(transform.forward, transform.up));

            gravityType = GravityType.Unaligned;
            StartFallingMoveCameraCoroutine();
            ReduceSenseAndControl();
        }
        return true;
    }

    [Min(0)]
    private int currentGravityMultipliers = 0;
    /// <summary> Multiply the current gravity magnitude by 2, up to the maximum number of multiplications. </summary>
    private bool SuperGravity()
    {
        if (!GetAbility(GravityAbility.SuperGravity).TryUse())
            return false;

        if (currentGravityMultipliers < maxGravityMultipliers)
        {
            currentGravityMultipliers += 1;
            float newMultiplier = 2f * currentGravityMultipliers;
            if (MoonGravityModifier)
                newMultiplier = 0.5f / currentGravityMultipliers;
            ChangeGravityMagnitude(newMultiplier);
            return true;
        }
        return false;
    }

    private bool _slowingTime = false;
    private Coroutine _slowTimeCoroutine;
    /// <summary> Slow time. </summary>
    private void BulletTimeStart()
    {
        if (!GetAbility(GravityAbility.TimeSlow).TryUse())
            return;

        if (_slowTimeCoroutine != null)
            StopCoroutine(_slowTimeCoroutine);
        _slowingTime = true;

        if (usedFixedSlowFactor)
        {
            Time.fixedDeltaTime = defaultFixedTimeInterval * fixedTimeSlowFactor;
            Time.timeScale = fixedTimeSlowFactor;
        }
        else
        {
            _slowTimeCoroutine = StartCoroutine(ScaleTimeWithSpeed());
        }
    }

    private IEnumerator ScaleTimeWithSpeed()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.5f);
        while (_slowingTime)
        {
            float timeFactor = Mathf.Clamp(timeSlowFactor.Evaluate(RB.velocity.magnitude), 0.1f, 2f);
            Time.fixedDeltaTime = defaultFixedTimeInterval * timeFactor;
            Time.timeScale = timeFactor;
            yield return wait;
        }
    }

    /// <summary> Slow time. </summary>
    private void BulletTimeEnd()
    {
        _slowingTime = false;
        Time.fixedDeltaTime = defaultFixedTimeInterval;
        Time.timeScale = 1;
    }

    /// <summary> Begin simulating the trajectory line. </summary>
    private void TrajectoryLineStart()
    {
        if (!GetAbility(GravityAbility.TrajectoryLine).TryUse())
            return;

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
            if (!GetAbility(GravityAbility.MoonGravity).TryUse())
                _moonGravityModifier = false;
            _moonGravityModifier = value;
        }
    }

    /// <summary> Reset gravity to the default physics gravity of the world. </summary>
    private void GravityReset()
    {
        if (!GetAbility(GravityAbility.GravityReset).TryUse())
            return;

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
            print("Cancelling because no change.");
            return false;
        }
        else if (VectorsAreSimilar(NewGravity, Physics.gravity))
        {
            //If the new gravity is less than 10% different from the normal gravity, set gravity to normal.
            return ResetToWorldGravity();
        }
        else if (VectorsAreSimilar(NewGravity, CustomGravity))
        {
            print("Cancelling because too similar.");
            return false;
        }
        else //Do shift
        {
            currentGravityMultipliers = 0;
            RB.useGravity = false;
            CustomGravity = NewGravity;
            IntuitiveSnapRotation();
            shiftPoint = transform.position;
            return true;
        }
    }

    /// <summary> Change gravity to the default for the scene. </summary>
	public bool ResetToWorldGravity()
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
        if (gravityType == GravityType.Unaligned)
        {
            float distToTarget = Vector3.Distance(targetWall.point, transform.position);
            if (distToTarget < flyDistance || (distToTarget < Vector3.Distance(shiftPoint, transform.position)))
            {
                gravityType = GravityType.Aligned;
                if (ShiftGravityDirection(CustomGravityMag / defaultGravityMagnitude, targetWall.normal * -1))
                {
                    StartCollisionMoveCameraCoroutine();
                }
            }
        }
    }
    #endregion

    #region Movement Interactions
    private Coroutine _moveCameraCoroutine;

    private void StartCollisionMoveCameraCoroutine()
    {
        if (!doCollisionAutoCamera)
            return;
        if (_moveCameraCoroutine != null)
            StopCoroutine(_moveCameraCoroutine);
        _moveCameraCoroutine = StartCoroutine(MoveCameraUp());
    }

    private void StartFallingMoveCameraCoroutine()
    {
        if (!doFlyingAutoCamera)
            return;
        if (_moveCameraCoroutine != null)
            StopCoroutine(_moveCameraCoroutine);
        if (CustomGravityMag <= 0)
            return;
        float expectedFallTime = Mathf.Sqrt((2f * (targetWall.distance - PM.playerSphereSize)) / CustomGravityMag);
        if (expectedFallTime <= 0)
            return;
        _moveCameraCoroutine = StartCoroutine(MoveCameraUp(false, expectedFallTime));
    }

    private IEnumerator MoveCameraUp(bool collisionVersion = true, float expectedDuration = 1)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        float startTime = Time.time;
        float totalVerticalInput = 0;
        float fallingVersionSpeed = Utility.UnsignedDifference(PM._CameraAngle, 0) / expectedDuration;
        while (PM._CameraAngle.FurtherFromZero(0.1f))
        {
            if (Time.time > startTime + Mathf.Max(maxAutoCameraDur, expectedDuration))
                yield break; //Break if the routine has been running for too long

            if (stopAutoCameraThreshold > 0)
            {
                if (onlyStopForDown == false || Input.GetAxis("Mouse Y") < 0)
                {
                    totalVerticalInput += Mathf.Abs(Input.GetAxis("Mouse Y"));
                    if (totalVerticalInput > stopAutoCameraThreshold)
                        yield break;
                }
            }


            float toMove = fallingVersionSpeed;
            if (collisionVersion)
                toMove = autoCameraSpeedByAngle.Evaluate(Utility.UnsignedDifference(PM._CameraAngle, 0));
            else
                toMove = autoCameraSpeedForFlying.Evaluate(Time.time - startTime);

            PM._CameraAngle = Mathf.MoveTowards(PM._CameraAngle, 0, toMove * Time.deltaTime);
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
 * - predicted trajectory line
 * - Object that gives/sets gravity change charges
 * - SuperGravity uses unscaled time?
 * - try to fix rotation for forced-shifts
 * - movement stopping time
 * - fix camera move cancel bug
 * 
 *>> settings for:
 * - maximum shift distance
 * - limited number of shifts (with option to reset/increase) [untested]
 * 
 * Done:
 * - hold button to reset
 * - automatic camera pan after changing gravity
 * - option for disabling or reducing movement after gravity shift
 * - option for lowering sensitivity after gravity shift
 * - max duration for gravity jump
 * - button for slow motion while in mid air
 * - FINISH hold to reset (needs to not be overriden by shift on release)
 * - System for enabling/disabling abiliy parts (list of structs?)
 * - Object that disables parts of ability
 * - Make objects that disable gravity (to and/or on collisions)
 * - Let clamp go outside max when resetting
 * - cooldown between shifts
 * - Fix SuperGravity bug (can use even if shift didn't work)
 * - Fix bug where collisions stop working (was caused by '_unCrouch' never turning off)
 */
