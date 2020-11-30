using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Movement : Shootable {

	//--->Debug
    [Header("Debug")]
	public string DebugString = "Default";
    public Vector2 PlaneVelocity = new Vector2 ();
    public float VerticalVelocity = 0;
    public float FUVelocityDelta;
	private float FUVelocityLastFrame;
    public float FULargestDeltaLastSecond;
	private float FULargestDeltaThisSecond;
	private float FULastUpdate;
    public float LateralVelocityBeforeFriction;

	//--->Singleton
	public static Movement ThePlayer;

	//--->Static
	public static Vector3 PlayerStartPosition = new Vector3(7, 5, 95);

    //--->Stored references
    [System.NonSerialized]
	public Camera MainCamera;
    [Header("References")]
    public Transform CameraOrHolder;
    public Camera DeathCamera;
	protected Rigidbody RB;
	private Transform Body;
	private TriggerChecker GroundedTrigger;
	//private Canvas C;
	private GameObject Menu;
	//private AudioSource SFXPlayer;
	private AudioManager SFXPlayer;
	public ParticleSystem ImpactEffect;
	private Slider HealthBar;

	//--->Private values
	private bool OnSomething = false;
	private bool Paused = false;
	private bool Crouching = false;
	private bool UnCrouching = false;
	private float AlteredMaxSpeed = 3;


	//--->Messages between Update and FixedUpdate
	private Vector3 DirectionToMove = new Vector3();
	private float WantToJump = -5;
	private float LastJumpTime = -5;

	//--->Messages from or for other scripts:
	//-->That SHOULD be in the inspector.

	//-->That SHOULDN'T be in the inspector, AKA Public non-serialized values
	[System.NonSerialized]
	public bool Grounded = false;
    [System.NonSerialized]
    public float LastGrounded = -1;
    [System.NonSerialized]
	public bool DisableMovement = false;
	[System.NonSerialized]
	public bool DisableMouseInput = false;  //Disables the movement of the camera, so that the Gravity script can move it smoothly.
    [System.NonSerialized]
    public Vector3 AIFollowPoint;
	[System.NonSerialized]
	public bool ImpactLastFrame = false;
	[System.NonSerialized]
	public float VelocityOfImpact = 0;
	[System.NonSerialized]
	public float CameraAngle = 0;
	[System.NonSerialized]
	public float CameraSpin = 0;
	[System.NonSerialized]
	public bool Invisible = false;
	[System.NonSerialized]
	public bool OnSoftWall = false;
    /// <summary> Used by Gravity script. Becomes true when the player collides with a wall. </summary>
	[System.NonSerialized]
	public bool CheckForWallAlignment = false;
	[System.NonSerialized]
	public Collision LastCollision;


    //Public values (for adjustment)
    //------------------------------
    [Header("Settings")]
    public bool DisableMusic = false;
	public bool DisableDeath = false;
	/// <summary> The min and max angles the camera can face when looking up or down. </summary>
	public float Clamp = 89;
	/// <summary> The number of degrees per second the camera will be rotated so that it is not outside the clamp angle. </summary>
	public float ClampAdjustmentSpeed = 5;
	/// <summary> The in game sensitivity multiplier (for settings). </summary>
	public float Sensitivity = 1;
	/// <summary> The maximum non-vertical speed the player can cause themselves to move at with 'walking' (does not constrain gravity, explosions etc). </summary>
	[Range(1f, 100f)]
	public float MaxSpeed = 3;
	/// <summary> How quickly direction is changed. </summary>
	[Range(0f, 100f)]
	public float Acceleration = 0.5f;
	/// <summary> How quickly direction is returned to 0 when the player is not trying to move (no WASD keys down). </summary>
	[Range(0f, 500f)]
	public float StoppingForce = 0.05f;
	/// <summary> How quickly direction is returned to 0 when the player is not trying to move (no WASD keys down). </summary>
	[Range(0f, 500f)]
	public float FrictionForce = 0.05f;
	/// <summary> The multiplier on movement while in the air. Also effects air-ground friction ratio. Cannot result in greater air speed than ground speed. </summary>
	[Range(0f, 1f)]
	public float AirControlFactor = 0.5f;
	/// <summary> The jump velocity. </summary>
	public float JumpVelocity = 8;
	/// <summary> The jump velocity per tick it is held. </summary>
	public float JumpVelocityPerSecondHeld = 4;
	/// <summary> The maximum amount of seconds jump can be held to jump higher. </summary>
	public float MaxJumpTime = 0.2f;
	/// <summary> The time that a jump request is 'remembered', in case Unity physics doesn't detect a collision on that frame. </summary>
	public float JumpLeniency = 0.1f;
	/// <summary> The maximim size of the player (i.e. head to toe). </summary>
	public float PlayerSphereSize = 2;
	/// <summary> The radius of the players capsule collider. </summary>
	public float PlayerWaistSize = 0.5f;
	/// <summary> The minimum velocity to play impact sounds at (or destroy robots on impact). </summary>
	public float ImpactVelocity = 10;


	void Awake () {
		ThePlayer = this; //Initialize singleton so that AI and (some) abilities can reference this script.
	}

	void Start () {
		EnemyCounter.MaxBasicEnemies = FindObjectsOfType<BaseEnemy> ().Length;
		EnemyCounter.MaxFollowingEnemies = FindObjectsOfType<MovingEnemy> ().Length;
		EnemyCounter.UpdateScoreboard ();

		MainCamera = GetComponentInChildren<Camera> ();
        if (CameraOrHolder == null)
            CameraOrHolder = MainCamera.transform;
        //C = FindObjectOfType<Canvas> ();
        GameObject MenuUI = UIManager.stat.LoadOrGetUI("Menu");
        GameObject GameUI = UIManager.stat.LoadOrGetUI("Shooter");
        Menu = MenuUI.GetComponentInChildren<Menu>().gameObject;
		Menu.SetActive (false);
		HealthBar = GameUI.GetComponentInChildren<Slider> ();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		Time.timeScale = 1;
		RB = GetComponent<Rigidbody> ();
		RB.freezeRotation = true;
		Body = GetComponentInChildren<CapsuleCollider> ().transform;
		AlteredMaxSpeed = MaxSpeed;
		SFXPlayer = GetComponent<AudioManager> ();
        if (ImpactEffect == null)
		    ImpactEffect = GetComponentInChildren<ParticleSystem> ();
		AudioListener.volume = 0.5f;
		DisplayVolume ();

        
		//Grounded Trigger for jumping.
		if (GroundedTrigger == null) {
			var GO = Instantiate (new GameObject (), transform);
			GO.name = "TC: Grounded Check";
			GO.layer = gameObject.layer;
			GroundedTrigger = GO.AddComponent<TriggerChecker>();
			var SC = GO.AddComponent<SphereCollider> ();
			SC.radius = 0.35f;
			SC.isTrigger = true;
			SC.center = new Vector3 (0, 0, 0.8f);
		}

		//if (DisableMusic)
		//	MainCamera.GetComponent<AudioSource> ().enabled = false;

		//transform.position = PlayerStartPosition;

		Resources.Load<GameObject> ("Prefabs/DeadBody");
	}

	void Update () {

        UpdFollowPoint();

        UpdAchievements();

        //PAUSING + CURSOR LOCK
        if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Escape))
            Pause();

        if (Paused)
            return;

        UpdCamera();

        UpdMove();
	}

    private void UpdFollowPoint()
    {
        //SET FOLLOW POINT
        RaycastHit ClosestDownwardSurface;
        if (Physics.Raycast(transform.position, -Vector3.up, out ClosestDownwardSurface))
        {
            AIFollowPoint = ClosestDownwardSurface.point;
        }
    }

    private void UpdAchievements()
    {
        //TELL ACHIEVEMENT TRACKER WHEN GROUNDED
        Grounded = GroundedTrigger.Triggered;
        if (Grounded)
        {
            LastGrounded = Time.time;
            AchievementTracker.TouchedTheGround();
        }
        else
        {
            AchievementTracker.InAir = true;
            OnSoftWall = false;
        }
    }

    private void UpdCamera()
    {
        //CAMERA CONTROL

        //CAMERA X-ROTATION
        //Clamp the angle to a range of -180 to 180 for easier maths.
        if (CameraAngle > 180 || CameraAngle < -180)
            CameraAngle = ClampAngleTo180(CameraAngle);

        //Check if the camera was within the clamps before input (in case it was changed, eg by Gravity)
        bool CameraWasWithinClamp = (CameraAngle <= Clamp) && (CameraAngle >= -Clamp);
        //Apply the mouse input.
        CameraAngle -= Input.GetAxis("Mouse Y") * Sensitivity;
        //Clamp the angle.
        CameraAngle = Mathf.Clamp(CameraAngle, -Clamp, Clamp);

        Quaternion NewRot = new Quaternion();
        NewRot.eulerAngles = new Vector3(CameraAngle, CameraOrHolder.localRotation.y, 0);
        CameraOrHolder.localRotation = NewRot;

        //Rotate player
        float rotationX = -Input.GetAxis("Mouse X") * Sensitivity;
        if (CameraAngle.Outside(-90, 90))
            rotationX *= -1;
        transform.localRotation *= Quaternion.AngleAxis(rotationX, Vector3.forward);
    }

    private void UpdCrouch()
    {
        //CROUCHING
        if (Grounded && Input.GetButton("Crouch"))
        {
            if (!Crouching)
                Crouch();
        }
        else if (Crouching)
            UnCrouch();
    }

    private void UpdMove()
    {
        //--------------------------MOVEMENT PHYSICS + INPUTS--------------------------//
        //INPUT
        Vector3 desiredDirection = new Vector3();
        desiredDirection += transform.up * Input.GetAxis("Vertical");
        desiredDirection += transform.right * Input.GetAxis("Horizontal");

        desiredDirection.Normalize();

        if (Input.GetButtonDown("Jump"))
        {
            if (Grounded && OnSomething)
            {
                Grounded = false;
                OnSomething = false;
                WantToJump = Time.time;
                SFXPlayer.PlaySound("Jump");
            }
        }

        UpdCrouch();

        //Factor crouching and being in the air into the max speed;
        AlteredMaxSpeed = MaxSpeed;
        if (Crouching)
            AlteredMaxSpeed *= 0.8f;
        if (!Grounded)
            AlteredMaxSpeed *= 0.5f;

        Vector3 newVelocity;
        if (Grounded && OnSomething)
        {
            newVelocity = RB.velocity + (desiredDirection * Acceleration * Time.deltaTime);
        }
        else
            newVelocity = RB.velocity + (desiredDirection * Acceleration * AirControlFactor * Time.deltaTime);

        Vector3 TransformedOldVelocity = transform.InverseTransformVector(RB.velocity);
        Vector3 TransformedNewVelocity = transform.InverseTransformVector(newVelocity);

        //If the local non-vertical (lateral) velocity of the player is above the max speed, do not allow any increases in speed due to input.
        Vector3 LateralVelocityOld = new Vector3(TransformedOldVelocity.x, TransformedOldVelocity.y, 0);
        Vector3 LateralVelocityNew = new Vector3(TransformedNewVelocity.x, TransformedNewVelocity.y, 0);
        if (LateralVelocityNew.magnitude > AlteredMaxSpeed)
        {
            //If the new movement would speed up the player.
            if (LateralVelocityNew.magnitude > LateralVelocityOld.magnitude)
            {
                //If the player was not at max speed yet, set them to the max speed, otherwise revert to the old speed (but with direction changes).
                if (LateralVelocityOld.magnitude < AlteredMaxSpeed)
                    LateralVelocityNew = LateralVelocityNew.normalized * AlteredMaxSpeed;
                else
                    LateralVelocityNew = LateralVelocityNew.normalized * LateralVelocityOld.magnitude;
            }

            //FRICTION
            //If the new lateral velocity is still greater than the max speed, reduce it by the relevant amount until it is AT the max speed.
            if (LateralVelocityNew.magnitude > MaxSpeed)
            {
                if (Grounded)
                    LateralVelocityNew = LateralVelocityNew.normalized * Mathf.Max(MaxSpeed, LateralVelocityNew.magnitude - FrictionForce);
                //else
                //	LateralVelocityNew = LateralVelocityNew.normalized * Mathf.Max (MaxSpeed, LateralVelocityNew.magnitude - (FrictionForce * AirControlFactor));
            }

            //Add the vertical component back, convert it to world-space, and set the new velocity to it.
            LateralVelocityNew += new Vector3(0, 0, TransformedNewVelocity.z);
            newVelocity = transform.TransformVector(LateralVelocityNew);
        }

        //DEBUG DISPLAY.
        PlaneVelocity.x = Mathf.Round(LateralVelocityNew.x * 100f) / 100f;
        PlaneVelocity.y = Mathf.Round(LateralVelocityNew.y * 100f) / 100f;
        VerticalVelocity = Mathf.Round(TransformedNewVelocity.z * 100f) / 100f;

        Vector3 FinalVelocityChange = newVelocity - RB.velocity;

        DebugString = "Not";
        //If standing on a surface, and the player is not trying to move or jump, or if movement is disabled, slow movement.
        if ((Grounded && OnSomething && desiredDirection.magnitude < 0.01f && !Input.GetButton("Jump")) || DisableMovement)
        {

            Vector3 NewVelocity = RB.velocity;

            //Jump to zero velocity when below max speed and on the ground to give more control and prevent gliding.
            if (RB.velocity.magnitude < AlteredMaxSpeed / 2)
                RB.velocity = new Vector3();
            else
            {
                //Apply a 'friction' force to the player.
                DebugString = "Stopping";
                NewVelocity = NewVelocity.normalized * Mathf.Max(0, NewVelocity.magnitude - (StoppingForce * Time.deltaTime));
                RB.velocity = NewVelocity;
            }
        }
        else
        {
            if (Time.time < WantToJump + JumpLeniency)
            {
                RB.velocity += -transform.forward * JumpVelocity;
                LastJumpTime = Time.time;
            }

            WantToJump = -5;

            if (Time.time < LastJumpTime + MaxJumpTime)
            {
                if (Input.GetButton("Jump"))
                    RB.velocity += -transform.forward * JumpVelocityPerSecondHeld * Time.deltaTime;
            }

            //Move the player the chosen direction (could move to fixed update to regulate speed).
            RB.velocity += FinalVelocityChange;// * Time.deltaTime;
        }
        DisableMovement = false;

        ImpactLastFrame = false;
    }

    public void Pause() {
        Paused = !Paused;
        if (Paused)
        {
            if (!Input.GetKeyDown(KeyCode.BackQuote))
            {
                Menu.SetActive(true);
                AchievementTracker.UpdateAchievements();
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        }
        else
        {
            Menu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
        }
    }

	public void Crouch () {
		Crouching = true;
		Body.localScale = new Vector3 (1, 0.5f, 1);
		Body.localPosition = new Vector3 (0, -0.5f, 0);
		MainCamera.transform.localPosition = new Vector3 (0, -0.08f, 0.226f);
	}

	public void UnCrouch () {
		Crouching = false;
		UnCrouching = true;
		Body.localScale = new Vector3 (1, 1, 1);
		Body.localPosition = new Vector3 (0, 0, 0);
		MainCamera.transform.localPosition = new Vector3 (0, 0.42f, 0.226f);
	}

	public void Teleport (Vector3 Location) {
		//Play sounds or animations here.
		transform.position = Location;
		RB.velocity = Vector3.zero;
	}

	public override void Hit(float Damage) {
		Health -= Damage;
		HealthBar.value = Health / 100f;
		if (Health <= 0 && !DisableDeath) {
			DeathCamera.transform.position = MainCamera.transform.position;
			DeathCamera.transform.rotation = MainCamera.transform.rotation;

			transform.position = new Vector3 (14.9f, -51.59f, 14.9f);
			Quaternion Rot = new Quaternion ();
			Rot.eulerAngles = new Vector3 (90, 180, 135);
			transform.rotation = Rot;
			CameraAngle = 0;
			Health = MaxHealth;
			HealthBar.value = Health / 100f;
			RB.velocity = Vector3.zero;
			//Debug.Log ("YOU DIED");
			//Cursor.lockState = CursorLockMode.None;
			//SceneManager.LoadScene ("Death");
		}
	}

    /// <summary> Disables all abilities except the specified one. OBSOLETE. </summary>
	public void DisableAbilities (PlayerAbility NotMe) {
		foreach (PlayerAbility Ability in GetComponentsInChildren<PlayerAbility>()) {
            if (Ability != NotMe)
                Ability.Disable();
		}
	}

    /// <summary> Enables all abilities on the player. OBSOLETE. </summary>
	public void EnableAbilities () {
		foreach (PlayerAbility Ability in GetComponentsInChildren<PlayerAbility>()) {
			Ability.Enable();
		}
	}

    //MENU FUNCTIONS:

	public void SetPhaseMode (Dropdown Option) {
		if (Option.value == 0)
			GetComponent<Phasing> ().Mode = InputMode.Tap;
		else if (Option.value == 1)
			GetComponent<Phasing> ().Mode = InputMode.Hold;
		else
			GetComponent<Phasing> ().Mode = InputMode.Toggle;
	}

	public void SetSensitivity (GameObject TextObject) {
		Sensitivity = float.Parse(TextObject.GetComponent<Text>().text);
		DisplaySensitivity ();
	}

	public void DisplaySensitivity () {
		Menu.GetComponentsInChildren<MenuDisplayText>()[0].text = "Sensitivity = " + Sensitivity;
	}

	public void SetVolume (GameObject TextObject) {
		AudioListener.volume = Mathf.Clamp01 (float.Parse (TextObject.GetComponent<Text> ().text) / 10);
		DisplayVolume ();
	}

	public void DisplayVolume () {
		Menu.GetComponentsInChildren<MenuDisplayText>()[1].text = "Volume = " + (AudioListener.volume * 10);
	}

	public void SkipTutorial () {
		transform.position = new Vector3 (3, 90.5f, 82);
		Quaternion NewRot = new Quaternion ();
		NewRot.eulerAngles = new Vector3 (90, 90, 180);
		transform.rotation = NewRot;
	}

	public void ToggleEnemySounds () {
		MovingEnemy.PlaySounds = !MovingEnemy.PlaySounds;
	}

	/// <summary>
	/// Clamps the angle so that it is within the range [-180, 180], while maintaining the relative direction of the angle.
	/// </summary>
	public static float ClampAngleTo180 (float angle) {
		while (angle > 180)
			angle -= 360;

		while (angle < -180)
			angle += 360;

		return angle;
	}


	/// <summary>
	/// Returns a new angle that has been moved towards the nearest edge of the clamped zone, but not past it. Use degrees for the maximum angle to move by.
	/// </summary>
	public static float ClampSmooth (float Xangle, float Yangle, float min, float max) {

		if (Yangle > 90 || Yangle < -90) {
			if (Xangle >= 180)
				return 1;
			else if (Xangle < 180)
				return -1;
		}

		return 0;
	}

	/*
	void OnTriggerExit (Collider col) {
		Grounded = false;
	}

	void OnTriggerStay (Collider col) {
		Grounded = true;
	}
	*/
	void OnCollisionEnter (Collision col) {
		if (col.impulse.magnitude > ImpactVelocity) {
			//If this is a fake collision caused by 'uncrouching' and hitting your head on an object, re-crouch and cancel effect.
			if (UnCrouching) {
				UnCrouching = false;
				Crouch ();
				return;
			}

			if (col.gameObject.CompareTag ("Softwall")) {
				//Play soft wall sounds and do 'minijump'
				RB.velocity += -transform.forward * col.impulse.magnitude * 0.2f;
				OnSoftWall = true;
			} else {
				ImpactEffect.Play ();
				ImpactLastFrame = true;
				VelocityOfImpact = col.impulse.magnitude;
				SFXPlayer.PlaySound ("Impact");
				//ReachedImpactVelocity = false;
				BaseEnemy Enemy = col.transform.GetComponentInParent<BaseEnemy> ();
				if (Enemy) {
					AchievementTracker.StompKills += 1;
					AchievementTracker.EnemyDied ();
					Enemy.Die ();
				}
			}
		}

		LastCollision = col;
		CheckForWallAlignment = true;
	}

	void OnCollisionExit (Collision col) {
		OnSomething = false;
	}

	void OnCollisionStay (Collision col) {
		OnSomething = true;

		if (col.gameObject.CompareTag ("Softwall"))
			OnSoftWall = true;
	}
}
