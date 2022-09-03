using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GravityType {
	Normal,		//Gravity is set by the RigidBody, and the resource can regenerate.
	Unaligned,	//Gravity was set by the camera direction, and will change on a collision.
	Aligned		//Gravity is aligned to a surface (or to normal, but with a different magnitude).
}

[RequireComponent(typeof(Movement))]
public class Gravity : PlayerAbility {

    #region Auto References
    private Rigidbody RB;
	private Movement PM;
	private GravityRep UIGyroscope;
	private GravityRep UINormalGravity;
    #endregion

    #region Ability Variables
    /// <summary> Represents the amount of force that should be added to the player (or object) per second. </summary>
    private Vector3 customGravity;
    /// <summary> Normalized direction of gravity, so that the gravity magnitude can be changed by VariableGravity to a negative (and not lose its sign straight after). </summary>
    private Vector3 originalDirection;
    /// <summary> Reflects (does not define) the unsigned magnitude of gravity, required for VariableGravity to work. </summary>
    private float customGravityMagnitude;
    /// <summary> The position from which unaligned gravity was set, so that collisions before reaching the target don't align gravity too early. </summary>
	private Vector3 shiftPosition;
    /// <summary> The raycast hit of an unaligned gravity shift, used to find the normal to align to later, and the distance to it. </summary>
	private RaycastHit targetWall;
    /// <summary> The magnitude that gravity will be set to if it is changed. Used for a 'custom gravity force' with a flexible number. </summary>
    private float personalScale = 0;
    /// <summary> Saves the magnitude of gravity before a jump-hover activation, so it can be returned on release. </summary>
    private float magBeforeDoubleJump = 0;
    /// <summary> The clamp value set in Movement. Clamp is disabled while in mid air. </summary>
    private float groundedClamp = 89;
    /// <summary> Speed at which the clamp returns to normal when grounded, in degrees per second. </summary>
    private float clampReturnRate = 2;
    /// <summary> The minimum velocity to play impact sounds at (or destroy robots on impact). </summary>
    public float impactVelocity = 10;
    #endregion

    #region Debug
    [Header("Debug Properties")]
    /// <summary> The current status of gravity, see GravityType declaration above for detail. </summary>
	public GravityType type = GravityType.Normal;

    public bool startAtMaxResource = true;
    #endregion

    #region Inspector References
    [Header("Object References")]
    /// <summary> Reference to the object that shows where gravity has been shifted to. </summary>
	public GravityCircle circle;
    /// <summary> Reference to the UI element which shows how much resource the player has. </summary>
	private ResourceMeter meter;
    /// <summary> Reference to the UI element which shows the magnitude of gravity. </summary>
	private BarDisplay dragonMeter;
    /// <summary> Reference to an AudioSource which plays sounds for gravity abilities. </summary>
	private AudioSource SFXPlayer;
    #endregion

    #region Inspector Settings
    [Header("Gravity Ability Settings")]
    /// <summary> Aiming at a wall closer than this distance will align gravity to the normal of the hit, further will align gravity to the aim direction. </summary>
    [Tooltip("Aiming at a wall closer than this distance will align gravity to the normal of the hit, further will align gravity to the aim direction.")]
    public float autoLockDistance = 2f;
    /// <summary> The normal value of gravity. Used for resetting and as a multiplier for abilities. </summary>
    [Tooltip("The normal value of gravity. Used for resetting and as a multiplier for abilities.")]
    public float normalGravityMagnitude = 9.8f;
    /// <summary> Multiplier on the impact force, which is used to calculate the resource loss from the impact. </summary>
    [Tooltip("Multiplier on the impact force, which is used to calculate the resource loss from the impact.")]
    public float impactDamageMag = 2f;
    /// <summary> Affects how quickly the player will come to a halt while holding C. </summary>
    [Tooltip("Affects how quickly the player will come to a halt while holding C.")]
    public float stabilizationForce = 2f;
    /// <summary> Multiplier for how fast the scroll wheel changes gravity. </summary>
    [Tooltip("Multiplier for how fast the scroll wheel changes gravity.")]
    public float scrollChangeRate = 2f;
    /// <summary> The magnitude of the gravity (relative to normal G) when the shift key is pressed once. </summary>
    [Tooltip("The magnitude of the gravity (relative to normal G) when the shift key is pressed once.")]
    public float defaultLashingMagnitude = 0.5f;
    /// <summary> Time between gravity changes (in the same direction) where it will double the gravity instead of resetting. </summary>
    [Tooltip("Time between gravity changes (in the same direction) where it will double the gravity instead of resetting.")]
    public float multipleLashingWindow = 0.2f;
    /// <summary> Maximum number of times the player can double their gravity. </summary>
    [Tooltip("Maximum number of times the player can double their gravity.")]
    public int maxLashings = 3;

    //Variables used for ability settings:
    /// <summary> Time since the last gravity change (for doing multiple lashings). </summary>
    private float multipleLashingLastTime = -10f;
    /// <summary> Maximum number of times the player can double their gravity. </summary>
    private int currentLashings = 0;

    public enum LimitType
    {
        None,
        Resource,
        Cooldown,
        Charges
    }
    [Header("Resource/Cooldown Settings")]
    public LimitType abilityLimitMethod = LimitType.Cooldown;

    /// <summary> Time between uses of the main gravity abilities. </summary>
    public float cooldownTime = 5f;

    /// <summary> Time between uses of the main gravity abilities. </summary>
    public int numberOfCharges = 5;

    /// <summary> Reset gravity to world normal when the resource is drained. </summary>
    public bool resetAtZeroResource = false;
    /// <summary> If false, resource regeneration will only occur when the players feet are on the ground. </summary>
    public bool regenMidair = false;
    /// <summary> If true, resource regeneration will not be effected by having abnormal gravity,
    /// standing on a soft-wall, or being mid air (overrides RegenMidair). </summary>
    public bool flatRegenRate = false;
    /// <summary> Resource used per gravity change (caused by player). </summary>
    public float resourcePerUse = 5f;
    /// <summary> Resource used per second that gravity is not normal. </summary>
    public float stabilizingRPS = 2f;
    /// <summary> Time after ability use before the resource will begin regenerating. </summary>
    public float regenDelay = 0.2f;
    /// <summary> How long it takes to reset gravity to world default when holding the gravity button [F]. </summary>
    private float gravityButtonHeldTime = 0f;
    /// <summary> The last time an ability was used. </summary>
    private float lastLimitedAbilityUse = 0f;
    #endregion

    #region ExperimentalSettings
    [Header("Experimental Settings")]
    /// <summary> How long it takes to reset gravity to world default when holding the gravity button [F]. </summary>
    public float holdToResetTime = 1f;
    /// <summary> When true sensitivity is reduced after gravity has changed until the player lands. </summary>
    public bool reduceSenseWhileFlying = false;
    /// <summary> Amount to reduce sensitivity by when flying if setting is on. </summary>
    [Range(0.001f, 1)]
    public float flyingSenseMultiplier = 0.5f;
    
    public AnimationCurve autoCameraSpeedByAngle = new AnimationCurve();
    [Tooltip("Player mouse input required to cancel the auto camera. 0 to disable.")]
    public float stopAutoCameraThreshold = 10f;
    public float maxAutoCameraDur = 2;

    #endregion

    #region Unity Events
    // Use this for initialization
    void Start ()
    {
        //Find player components
        RB = GetComponent<Rigidbody>();
        PM = GetComponent<Movement>();
        PM.collisionEvent += GravityCollision;
        SFXPlayer = GetComponentsInChildren<AudioSource>()[1];

        //Find UI elements
        GameObject GravityUI = UIManager.stat.LoadOrGetUI("Gravity");
        meter = GravityUI.GetComponentInChildren<ResourceMeter> ();
		dragonMeter = GravityUI.GetComponentInChildren<BarDisplay> ();
		UIGyroscope = GetComponentInChildren<GravityRep> ();
		UINormalGravity = GetComponentsInChildren<GravityRep> ()[1];

        //Setup starting values
        normalGravityMagnitude = Physics.gravity.magnitude;
        UINormalGravity.Down = -Vector3.up * normalGravityMagnitude;
		customGravityMagnitude = normalGravityMagnitude;
		ChangeGravity(Physics.gravity);
		ResetGravity();

        if (abilityLimitMethod == LimitType.Resource)
        {
            if (startAtMaxResource)
                FillResource();
        }
        else if (abilityLimitMethod == LimitType.Cooldown)
        {
            MaxResource = cooldownTime;
            if (startAtMaxResource)
                FillResource();
            MinToUse = cooldownTime;
            RegenPerSecond = 1;
            flatRegenRate = true;
        }
    }

	// Update is called once per frame
	void Update ()
    {
		if (!Disabled)
        {
            PlayerInput();
		}

        CustomGravityTick();
        FreeLookInAir();
        PassiveResourceChange();

		//Update the visual rep of the meter.
		meter.ChangeValue (Resource / MaxResource);
	}
    #endregion

    //--------------------FUNCTIONS--------------------
    #region Update Functions
    /// <summary> Perform actions based on the players input. </summary>
    private void PlayerInput()
    {
        //If 'Reset' or 'ChangeGravity' are pressed, change gravity (which checks if it is needed).
        if (Input.GetButtonDown("GravityReset"))
        {
            //Disable gravity when [C] is pressed down.
            //NoGravity ();
        }
        else if (Input.GetButtonUp("GravityReset"))
        {
            //Reset gravity to normal when [C] is released, or half normal when [SHIFT] is also held.
            if (Input.GetButton("AlignModifier"))
            {
                //ResetGravity (0.5f);
            }
            else if (type != GravityType.Normal)
            {
                ContextualShiftDown();
            }
        }
        else if (Input.GetButton("GravityReset"))
        {
            //Reset gravity to normal when [C] is released, or half normal when [SHIFT] is also held.
            //CustomIntuitiveSnapRotation(-PM.MainCamera.transform.up);
            NoGravityFreeLook();
        }
        else if (Input.GetButtonDown("GravityNormal"))
        {
            //Gravity Trajectory?
        }
        else if (Input.GetButtonUp("GravityNormal"))
        {
            //Run the change gravity function when [F] is released.
            ContextualGravityShift();

            //Prevents magnitude being reset when a shift is done during a double-jump
            magBeforeDoubleJump = 0;

            //Set the timer for resetting gravity on hold back to zero
            gravityButtonHeldTime = 0f;
        }

        if (Input.GetButton("GravityNormal"))
        {
            gravityButtonHeldTime += Time.deltaTime;
            if (gravityButtonHeldTime >= holdToResetTime)
                ResetGravity();
        }

        if (Input.GetButton("Crouch") && !PM.Grounded)
            //Provide a tiny slowing force when [Ctrl] is HELD.
            Stabilize();

        if (Time.time > PM.LastGrounded + 0.1f && Input.GetButtonDown("Jump"))
        {
            magBeforeDoubleJump = customGravity.magnitude / normalGravityMagnitude;
            ShiftGravityMagnitude(0.001f);
        }
        else if (Input.GetButtonUp("Jump"))
        {
            if (magBeforeDoubleJump > 0)
                ShiftGravityMagnitude(magBeforeDoubleJump);
            magBeforeDoubleJump = 0;
        }
    }

    /// <summary> Apply the gravity force (usually CurrentGravityMagnitude) to the player. </summary>
    private void CustomGravityTick()
    {
        //Apply the gravity force (so long as normal gravity isn't enabled).
        if (type != GravityType.Normal)
            RB.velocity += customGravity * Time.deltaTime;
    }

    private void FreeLookInAir()
    {
        if (!PM.Grounded)
        {
            PM.Clamp = 360;
        }
        else
            PM.Clamp = Mathf.Lerp(PM.Clamp, groundedClamp, clampReturnRate * Time.deltaTime);
    }

    /// <summary> Decreases or Increases the ability resource based on settings and various factors. </summary>
    private void PassiveResourceChange()
    {
        if (abilityLimitMethod == LimitType.None || abilityLimitMethod == LimitType.Charges)
            return;

        if (flatRegenRate)
        {
            RegenWithDelay(1f);
            return;
        }

        //Drain or regenerate the resource.
        if (PM.OnSoftWall || (type == GravityType.Normal && (regenMidair || PM.Grounded)))
        {
            if (PM.OnSoftWall)
                RegenWithDelay(2f);
            else
                RegenWithDelay(1f);
        }
        else if (type != GravityType.Normal)
        {
            if (!ConsumeResource(Time.deltaTime))
            {
                if (resetAtZeroResource)
                    ResetGravity();
            }
        }
    }
    #endregion

    #region Collision Functions
    private void GravityCollision(Collision col)
    {
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
                
                if (abilityLimitMethod == LimitType.Resource)
                    ConsumeResourceGreedy(velocityOfImpact * impactDamageMag, -20);
                    //The minimum value of -20 creates the delay after a strong collision while the resource value is in the negatives.
            }
        }
        else
        {
            //TODO: Play softer landing sound/effects
        }

        //If Gravity is unaligned and shift is not being held AND
        //The distance to the target is less than 2, or the original distance to the target is less than 3 OR
        //The distance from the target point is less than the distance from the starting point.
        if (type == GravityType.Unaligned && !Input.GetButton("AlignModifier") && (
            (Vector3.Distance(targetWall.point, transform.position) < 2 || Vector3.Distance(targetWall.point, shiftPosition) < 3) ||
            (Vector3.Distance(shiftPosition, transform.position) > Vector3.Distance(targetWall.point, transform.position))))
        {
            ShiftGravityDirection(customGravity.magnitude / normalGravityMagnitude, targetWall.normal * -1, false, GravityType.Aligned);
            StartMoveCameraCoroutine();
        }
    }
    #endregion
    
    #region Helper Functions
    /// <summary> Pauses regeneration if an ability was used more recently than RegenDelay. </summary>
    private void RegenWithDelay(float multiplier)
    {
        if (Time.time > lastLimitedAbilityUse + regenDelay)
            RegenerateResource(multiplier);
    }

    private bool VectorsAreSimilar(Vector3 vector1, Vector3 vector2)
    {
        return (vector1 - vector2).magnitude < 0.1f;
    }
    #endregion
    
    #region Gravity-Change Functions
    /// <summary> Sets gravityDirection, originalDirection, UIGyroscope.Down, and the dragonMeter to the new Value. </summary>
    private void ChangeGravity(Vector3 newValue)
    {
        customGravity = newValue;
        originalDirection = customGravity.normalized;
        UIGyroscope.Down = customGravity;
        dragonMeter.ChangeValue(customGravityMagnitude / (normalGravityMagnitude * 2));
    }

    /// <summary> Changes just the magnitude of gravity based on input from the scroll wheel. </summary>
    private void VariableGravity () {
		customGravityMagnitude += Input.mouseScrollDelta.y;

		if (customGravityMagnitude > normalGravityMagnitude * 2)
			customGravityMagnitude = normalGravityMagnitude * 2;
		else if (customGravityMagnitude < -(normalGravityMagnitude * 2))
			customGravityMagnitude = -(normalGravityMagnitude * 2);

		customGravity = originalDirection * customGravityMagnitude;
		dragonMeter.ChangeValue (customGravityMagnitude/(normalGravityMagnitude * 2));

		if (type == GravityType.Normal && (customGravity - Physics.gravity).magnitude > 0.1f) {
			type = GravityType.Aligned;
			RB.useGravity = false;
		}
	}

    /// <summary> When a modifier key is held (shift), and player is in the air, convert movement into 'bursts' of non-rotating gravity. </summary>
    private void GravityBurst()
    {
        //Check horizontal/vertical input.
        //Update a 'bursts' vector3 with the new desired burst.
        //Apply the current burst to the players gravity (or as a 'seperate' force?)
    }

	/// <summary> Brings the player towards zero velocity, but not below 0.1, so it doesnt feel unnatural. </summary>
	private void Stabilize () {
        if (abilityLimitMethod == LimitType.Resource)
        {
            if (!ConsumeResource(stabilizingRPS * Time.deltaTime))
                return;
        }
        if (RB.velocity.magnitude > 0.1f)
            RB.velocity = RB.velocity.normalized * (RB.velocity.magnitude - (stabilizationForce * Time.deltaTime));
    }

	/// <summary> Check if the target of the shift is within autolock distance, then call the gravity shift with the relevant values.
    /// Also checks for repeated taps for multiplied gravity. </summary>
	private void ContextualGravityShift () {
        //If the gravity button is pressed rapidly, double the gravity instead of changing it.
        if (Time.time < multipleLashingLastTime + multipleLashingWindow)
        {
            if (currentLashings < maxLashings)
            {
                ++currentLashings;
                //Play 'multiple lashings' SFX here.
                multipleLashingLastTime = Time.time;
                ChangeGravity(customGravity * 2);
                //customGravity = customGravity * 2;
                SFXPlayer.Play();
            }
            return;
        }
        else
            currentLashings = 0;

        if (Physics.Raycast (PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out targetWall)) {
			RaycastHit Hit;
			//Align if pointing to a wall 2 * AutoLockDistance away, or half the distance away if the modifier is held (SHIFT).
			if (targetWall.distance < autoLockDistance * 2 && !Input.GetButton ("AlignModifier") && Physics.Raycast (transform.position, targetWall.normal * -1, out Hit)) {
				if (Hit.distance < autoLockDistance && (Hit.normal - targetWall.normal).magnitude < 0.1f) {
					ShiftGravityDirection (defaultLashingMagnitude, targetWall.normal * -1, GravityType.Aligned);
                    StartMoveCameraCoroutine();
                    multipleLashingLastTime = Time.time;
                    return;
				}
			}
		}

		//Otherwise shift to the direction the player is pointing.
		//type = GravityType.Unaligned;
		ShiftGravityDirection (defaultLashingMagnitude, PM.MainCamera.transform.forward, GravityType.Unaligned);
        multipleLashingLastTime = Time.time;
    }

    /// <summary> Do a contextual gravity shift down relative to the camera. </summary>
    private void ContextualShiftDown()
    {
        if (Physics.Raycast(PM.MainCamera.transform.position, -PM.MainCamera.transform.up, out targetWall))
        {
            RaycastHit Hit;
            //Align if pointing to a wall 2 * AutoLockDistance away, or half the distance away if the modifier is held (SHIFT).
            if (targetWall.distance < autoLockDistance * 2 && !Input.GetButton("AlignModifier") && Physics.Raycast(transform.position, targetWall.normal * -1, out Hit))
            {
                if (Hit.distance < autoLockDistance && (Hit.normal - targetWall.normal).magnitude < 0.1f)
                {
                    ShiftGravityDirection(1, targetWall.normal * -1, GravityType.Unaligned);
                    return;
                }
            }
        }

        //Otherwise shift to the direction the player is pointing.
        ShiftGravityDirection(1, -PM.MainCamera.transform.up, GravityType.Unaligned);
    }

    /// <summary> Set gravity to down, but with a tiny magnitude, to create the effect of Zero-Gravity. </summary>
    private void NoGravity () {
		RB.useGravity = false;
		customGravityMagnitude = Physics.gravity.magnitude * 0.01f;
		ChangeGravity(Physics.gravity.normalized * 0.01f);
		type = GravityType.Aligned;

		IntuitiveSnapRotation ();
    }

    /// <summary> Removes gravity, and sets the reference frame relative to the *camera* to allow free/intuitive looking. (Copy of NoGravity) </summary>
    private void NoGravityFreeLook()
    {
        RB.useGravity = false;
        customGravityMagnitude = Physics.gravity.magnitude * 0.01f;
        ChangeGravity(-PM.MainCamera.transform.up * 0.01f);
        type = GravityType.Unaligned;

        IntuitiveSnapRotation();
    }

    /// <summary> Change only the magnitude of the current gravity. Uses no resource. </summary>
    private void ShiftGravityMagnitude(float GravityMultiplier)
    {
        Vector3 NewGravity = customGravity.normalized * normalGravityMagnitude * GravityMultiplier;

        if (VectorsAreSimilar(NewGravity, Physics.gravity))
        {
            //If the new gravity is less than 10% different from the normal gravity, set gravity to normal.
            ResetGravity(GravityMultiplier, false);
        }
        else
        {
            RB.useGravity = false;
            customGravityMagnitude = NewGravity.magnitude;
            ChangeGravity(NewGravity);
        }

        //TODO: sfx
    }

    /// <summary> Shift gravity in the given direction, and apply the given GravityMultiplier to the force.
    /// Checks for repeat changes and changes that would set gravity back to normal, and doesn't use resource for them. </summary>
    private void ShiftGravityDirection(float GravityMultiplier, Vector3 Direction, GravityType newGravityType)
    {
        ShiftGravityDirection(GravityMultiplier, Direction, true, newGravityType);
    }

    /// <summary> Shift gravity in the given direction, and apply the given GravityMultiplier to the force. 
    /// Checks for repeat changes and changes that would set gravity back to normal, and doesn't use resource for them. </summary>
    private void ShiftGravityDirection (float GravityMultiplier, Vector3 Direction, bool useResources, GravityType newGravityType) {
		Vector3 NewGravity = Direction * normalGravityMagnitude * GravityMultiplier;

        if (NewGravity == customGravity)
            return;

        if (VectorsAreSimilar(NewGravity, Physics.gravity))
        {
            //If the new gravity is less than 10% different from the normal gravity, set gravity to normal.
            ResetGravity(GravityMultiplier, true);
        }
        else if (!VectorsAreSimilar(customGravity, NewGravity))
        {
            if (useResources)
            {
                if (abilityLimitMethod == LimitType.Resource)
                {
                    //If limit type is resource, return if there is not enough
                    if (!StartConsumeResource(resourcePerUse))
                        return;
                }
                else if (abilityLimitMethod == LimitType.Cooldown)
                {
                    //If limit type is cooldown, return if cooldown is not finished, otherwise set the resource meter to 0
                    if (Time.time < lastLimitedAbilityUse + cooldownTime)
                        return;
                    else
                        ConsumeResourceGreedy(MaxResource, 0);
                }
                else if (abilityLimitMethod == LimitType.Charges)
                {
                    //If limit type is charges, return if there are no charges left, otherwise use a charge
                    if (numberOfCharges > 0)
                        numberOfCharges -= 1;
                    else
                        return;
                }
            }

            if (useResources)
                lastLimitedAbilityUse = Time.time;

            RB.useGravity = false;
            customGravityMagnitude = NewGravity.magnitude;
            ChangeGravity(NewGravity);
            type = newGravityType;
            if (type == GravityType.Normal)
                type = GravityType.Aligned;
        }

		IntuitiveSnapRotation ();
		SFXPlayer.Play ();

        if (circle == null)
            Debug.LogError("Gravity Target Circle not set in inspector!");
        else if (targetWall.point != null)
		    circle.Shift (targetWall.point - customGravity.normalized * 0.01f, Quaternion.LookRotation (transform.forward, transform.up));

		shiftPosition = transform.position;
    }

    /// <summary> Change gravity to the default for the scene. </summary>
	private void ResetGravity() {
        ResetGravity(1, false);
    }

    /// <summary> Change gravity to the default for the scene. Takes a multiplier for the magnitude (1 = normal gravity). </summary>
	private void ResetGravity(float GravityMultiplier, bool countsAsUse) {
		//If we are changing gravity:
		//If making gravity normal, and gravity is not already normal OR we are giving gravity a diffent magnitude to it's current one:
		//--->Play a sound effect, and rotate the players body.
		bool Rotate = false;
		if (countsAsUse && (GravityMultiplier == 1 && customGravity != Physics.gravity) || (customGravity.magnitude != Physics.gravity.magnitude * GravityMultiplier)) {
			SFXPlayer.Play ();
			Rotate = true; //If this is false, the magnitude code might still need to run, but the rotation code doesn't. (also still give feedback ;)
		}
			
		//Reset Gravity.
		if (GravityMultiplier != 1) {//Normal gravity but different magnitude.
			if (StartConsumeResource(resourcePerUse)) {
				type = GravityType.Aligned;
				customGravityMagnitude = (Physics.gravity * GravityMultiplier).magnitude;
				ChangeGravity(Physics.gravity * GravityMultiplier);
			} else {
				//Play ability failed SFX
			}
		} else {
			type = GravityType.Normal;
			RB.useGravity = true;
			customGravityMagnitude = Physics.gravity.magnitude;
			ChangeGravity(Physics.gravity);
		}

		//Reset player rotation.
		if (Rotate)
            IntuitiveSnapRotation ();
    }

    /// <summary> Change the rotation of the players body so that the 'feet' are pointing 'down' relative to the current gravity direction,
    /// and keep the facing (body-y / camera-x rot) as close to the original as manageable.
    /// NOTE: (accuracy is usually impossible since the position of the camera moves, and players focus is often different from their aim). </summary>
    private void IntuitiveSnapRotation()
    {
        Quaternion CameraPreRotation = PM.MainCamera.transform.rotation;
        Vector3 OriginalFacing = PM.MainCamera.transform.forward; //Remember that forward is down (the feet of the player) to let LookRotation work.

        //Rotate the players 'body'.
        transform.rotation = Quaternion.LookRotation(customGravity, GetComponentInChildren<GravityReference>().transform.right);
        transform.rotation = Quaternion.LookRotation(customGravity, GetComponentInChildren<GravityReference>().transform.forward);
        Quaternion NewRot = new Quaternion();
        NewRot.eulerAngles = new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
        transform.localRotation = NewRot;

        //Calculate the angle difference between the two rotations, then save the 'number of full rotations' it represents.
        float Signed = Vector3.SignedAngle(OriginalFacing, PM.MainCamera.transform.forward, transform.right);
        PM.CameraAngle -= Signed;
        PM.MainCamera.transform.rotation = CameraPreRotation;
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
        PM.CameraAngle -= Signed;
        PM.MainCamera.transform.rotation = CameraPreRotation;
    }
    #endregion

    #region Public Functions
    public void SetChargesTo(int numCharges)
    {
        numberOfCharges = numCharges;
    }

    public void IncreaseChargesBy(int numCharges)
    {
        numberOfCharges += numCharges;
    }

    //Disable override for PlayerAbility parent class
    public override void Disable()
    {
        Disabled = true;
        ResetGravity();
    }
    #endregion

    #region Coroutines
    private Coroutine moveCameraCoroutine;

    private void StartMoveCameraCoroutine()
    {
        if (moveCameraCoroutine != null)
            StopCoroutine(moveCameraCoroutine);
        moveCameraCoroutine = StartCoroutine(MoveCameraUp());
    }

    private IEnumerator MoveCameraUp()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        float startTime = Time.time;
        float angleLastFrame = PM.CameraAngle;
        while (PM.CameraAngle.FurtherFromZero(0.1f))
        {
            if (Time.time > startTime + maxAutoCameraDur)
                break; //Break if the routine has been running for too long

            if (stopAutoCameraThreshold > 0)
            {
                float frameDiff = Utility.UnsignedDifference(PM.CameraAngle, angleLastFrame);
                print(frameDiff / Time.deltaTime);
                if (frameDiff / Time.deltaTime > stopAutoCameraThreshold)
                    break; //Break if the player manually moved the camera too much last frame
            }

            float remaining = Utility.UnsignedDifference(PM.CameraAngle, 0);
            PM.CameraAngle = Mathf.MoveTowards(PM.CameraAngle, 0, autoCameraSpeedByAngle.Evaluate(remaining) * Time.deltaTime);
            angleLastFrame = PM.CameraAngle;
            yield return wait;
        }
    }
    #endregion
}


/* TO DO:
 * - option for disabling or reducing movement after gravity shift
 * - option for lowering sensitivity after gravity shift
 * - button for slow motion while in mid air
 * - automatic camera pan after changing gravity
 * - predicted trajectory line
 * 
 *>> settings for:
 * - maximum shift distance
 * - cooldown between shifts
 * - limited number of shifts (with option to reset/increase)
 * 
 * Done:
 * - hold button to reset
 */
