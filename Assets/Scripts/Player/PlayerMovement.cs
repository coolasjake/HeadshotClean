using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {

    #region Variables
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
	public static PlayerMovement ThePlayer;

	//--->Static
	public static Vector3 PlayerStartPosition = new Vector3(7, 5, 95);
    public static Vector3 CameraPos
    {
        get
        {
            if (ThePlayer.MainCamera != null)
                return ThePlayer.MainCamera.transform.position;
            
            return ThePlayer.transform.position;
        }
    }

    //--->Stored references
    [Header("References")]
    public Camera MainCamera;
    public Transform CameraTransform;
    public Transform RotationBody;
    public Camera DeathCamera;
	protected Rigidbody RB;
	private Transform Body;
	private TriggerChecker GroundedTrigger;
	//private Canvas C;
	private GameObject Menu;
	//private AudioSource SFXPlayer;
    [HideInInspector]
	public AudioManager SFXPlayer;
	public ParticleSystem ImpactEffect;

	//--->Private values
	private bool _onSomething = false;
	private bool _paused = false;
    private bool _movementEnabled = true;
	private bool _crouching = false;
	private bool _unCrouching = false;
	private float _alteredMaxSpeed = 3;
    private float _sensitivityMultiplier = 1f;
    private float _airControlMultiplier = 1f;


    //--->Messages between Update and FixedUpdate
    private Vector3 _directionToMove = new Vector3();
	private float _wantToJump = -5;
	private float _lastJumpTime = -5;

	//--->Messages from or for other scripts:
	//-->That SHOULD be in the inspector.

	//-->That SHOULDN'T be in the inspector, AKA Public non-serialized values
	[System.NonSerialized]
	public bool _Grounded = false;
    [System.NonSerialized]
    public float _LastGrounded = -1;
    /// <summary> Disables player movement for this frame only. </summary>
    //[System.NonSerialized]
	//public bool _DisableMovement = false;
	[System.NonSerialized]
	public bool _DisableMouseInput = false;  //Disables the movement of the camera, so that the Gravity script can move it smoothly.
    [System.NonSerialized]
    public Vector3 _AIFollowPoint;
	[System.NonSerialized]
	public float _CameraAngle = 0;
	[System.NonSerialized]
	public float _CameraSpin = 0;
	[System.NonSerialized]
	public bool _Invisible = false;
	[System.NonSerialized]
	public bool _OnSoftWall = false;
    // <summary> Used by Gravity script. Becomes true when the player collides with a wall. </summary>
	//[System.NonSerialized]
	//public bool CheckForWallAlignment = false;


    //Public values (for adjustment)
    //------------------------------
    [Header("Settings")]
    public bool disableMusic = false;
	public bool disableDeath = false;
	/// <summary> The min and max angles the camera can face when looking up or down. </summary>
	public float clamp = 89.99f;
	/// <summary> The number of degrees per second the camera will be rotated so that it is not outside the clamp angle. </summary>
	public float clampAdjustmentSpeed = 180;
	/// <summary> The in game sensitivity multiplier (for settings). </summary>
	public float userSensitivity = 1;
	/// <summary> The maximum non-vertical speed the player can cause themselves to move at with 'walking' (does not constrain gravity, explosions etc). </summary>
	[Range(1f, 100f)]
	public float maxSpeed = 4;
	/// <summary> How quickly direction is changed. </summary>
	[Range(0f, 100f)]
	public float acceleration = 20f;
	/// <summary> How quickly direction is returned to 0 when the player is not trying to move (no WASD keys down). </summary>
	[Range(0f, 500f)]
	public float stoppingForce = 500f;
	/// <summary> How quickly direction is returned to 0 when the player is not trying to move (no WASD keys down). </summary>
	[Range(0f, 500f)]
	public float frictionForce = 100f;
	/// <summary> The multiplier on movement while in the air. Also effects air-ground friction ratio. Cannot result in greater air speed than ground speed. </summary>
	[Range(0f, 1f)]
	public float airControlFactor = 0.5f;
	/// <summary> The jump velocity. </summary>
	public float jumpVelocity = 4;
	/// <summary> The jump velocity per tick it is held. </summary>
	public float jumpVelocityPerSecondHeld = 30;
	/// <summary> The maximum amount of seconds jump can be held to jump higher. </summary>
	public float maxJumpTime = 0.2f;
	/// <summary> The time that a jump request is 'remembered', in case Unity physics doesn't detect a collision on that frame. </summary>
	public float jumpLeniency = 0.1f;
	/// <summary> The maximim size of the player (i.e. head to toe). </summary>
	public float playerSphereSize = 2;
	/// <summary> The radius of the players capsule collider. </summary>
	public float playerWaistSize = 0.5f;
    #endregion

    void Awake () {
		ThePlayer = this; //Initialize singleton so that AI and (some) abilities can reference this script.
	}

	void Start () {
        if (MainCamera == null)
		    MainCamera = GetComponentInChildren<Camera> ();
        if (CameraTransform == null)
            CameraTransform = MainCamera.transform;
        //C = FindObjectOfType<Canvas> ();
        GameObject MenuUI = UIManager.stat.LoadOrGetUI("Menu");
        //GameObject GameUI = UIManager.stat.LoadOrGetUI("Shooter");
        Menu = MenuUI.GetComponentInChildren<Menu>().gameObject;
		Menu.SetActive (false);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		Time.timeScale = 1;
		RB = GetComponent<Rigidbody> ();
		RB.freezeRotation = true;
		Body = GetComponentInChildren<CapsuleCollider> ().transform;
		_alteredMaxSpeed = maxSpeed;
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

		Resources.Load<GameObject> ("Prefabs/Enemies/DeadBody");
	}

	void Update () {

        UpdFollowPoint();

        UpdAchievements();

        //PAUSING + CURSOR LOCK
        if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Escape))
            Pause();

        if (_paused || _movementEnabled == false)
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
            _AIFollowPoint = ClosestDownwardSurface.point;
        }
    }

    private void UpdAchievements()
    {
        //TELL ACHIEVEMENT TRACKER WHEN GROUNDED
        _Grounded = GroundedTrigger.Triggered;
        if (_Grounded)
        {
            _LastGrounded = Time.time;
        }
        else
        {
            _OnSoftWall = false;
        }
    }

    private void UpdCamera()
    {
        //CAMERA CONTROL
        CameraAngle -= Input.GetAxis("Mouse Y") * Sensitivity;

        //Rotate player
        float rotationX = -Input.GetAxis("Mouse X") * Sensitivity;
        if (CameraAngle.Outside(-90, 90))
            rotationX *= -1;
        transform.localRotation *= Quaternion.AngleAxis(rotationX, Vector3.forward);
    }

    public float CameraAngle
    {
        get { return _CameraAngle; }
        set
        {
            //CAMERA X-ROTATION
            //Clamp the angle to a range of -180 to 180 instead of 0 to 360 for easier maths.
            if (_CameraAngle > 180 || _CameraAngle < -180)
                _CameraAngle = ClampAngleTo180(_CameraAngle);

            //Check if the camera was within the clamps before input (in case it was changed, eg by Gravity)
            bool CameraWasWithinClamp = (_CameraAngle <= clamp) && (_CameraAngle >= -clamp);
            //Apply the mouse input.
            _CameraAngle = value;
            //Clamp the angle.
            if (_CameraAngle > clamp || _CameraAngle < -clamp)
            {
                if (CameraWasWithinClamp)
                    _CameraAngle = Mathf.Clamp(_CameraAngle, -clamp, clamp);
                else
                {
                    if (_CameraAngle > clamp)
                        _CameraAngle -= clampAdjustmentSpeed * Time.deltaTime;
                    if (_CameraAngle < -clamp)
                        _CameraAngle += clampAdjustmentSpeed * Time.deltaTime;
                }
            }


            Quaternion NewRot = new Quaternion();
            NewRot.eulerAngles = new Vector3(_CameraAngle, CameraTransform.localRotation.y, 0);
            CameraTransform.localRotation = NewRot;
        }
    }

    protected float Sensitivity
    {
        get { return userSensitivity * _sensitivityMultiplier; }
    }

    public float SensitivityMultiplier
    {
        set { _sensitivityMultiplier = value; }
    }

    protected float AirControl
    {
        get { return airControlFactor * _airControlMultiplier; }
    }

    public float AirControlMultiplier
    {
        set { _airControlMultiplier = value; }
    }

    private void UpdCrouch()
    {
        //CROUCHING
        if (_Grounded && Input.GetButton("Crouch"))
        {
            if (!_crouching)
                Crouch();
        }
        else if (_crouching)
            UnCrouch();
        else
            _unCrouching = false;
    }

    private void UpdMove()
    {
        //--------------------------MOVEMENT PHYSICS + INPUTS--------------------------//
        //INPUT
        Vector3 desiredDirection = new Vector3();
        desiredDirection += RotationBody.transform.forward * Input.GetAxis("Vertical");
        desiredDirection += RotationBody.transform.right * Input.GetAxis("Horizontal");

        desiredDirection.Normalize();

        if (Input.GetButtonDown("Jump"))
        {
            if (_Grounded && _onSomething)
            {
                _Grounded = false;
                _onSomething = false;
                _wantToJump = Time.time;
                SFXPlayer.PlaySound("Jump");
            }
        }

        UpdCrouch();

        //Factor crouching and being in the air into the max speed;
        _alteredMaxSpeed = maxSpeed;
        if (_crouching)
            _alteredMaxSpeed *= 0.8f;
        if (!_Grounded)
            _alteredMaxSpeed *= 0.5f;

        Vector3 newVelocity;
        if (_Grounded && _onSomething)
        {
            newVelocity = RB.velocity + (desiredDirection * acceleration * Time.deltaTime);
        }
        else
            newVelocity = RB.velocity + (desiredDirection * acceleration * AirControl * Time.deltaTime);

        Vector3 TransformedOldVelocity = transform.InverseTransformVector(RB.velocity);
        Vector3 TransformedNewVelocity = transform.InverseTransformVector(newVelocity);

        //If the local non-vertical (lateral) velocity of the player is above the max speed, do not allow any increases in speed due to input.
        Vector3 LateralVelocityOld = new Vector3(TransformedOldVelocity.x, TransformedOldVelocity.y, 0);
        Vector3 LateralVelocityNew = new Vector3(TransformedNewVelocity.x, TransformedNewVelocity.y, 0);
        if (LateralVelocityNew.magnitude > _alteredMaxSpeed)
        {
            //If the new movement would speed up the player.
            if (LateralVelocityNew.magnitude > LateralVelocityOld.magnitude)
            {
                //If the player was not at max speed yet, set them to the max speed, otherwise revert to the old speed (but with direction changes).
                if (LateralVelocityOld.magnitude < _alteredMaxSpeed)
                    LateralVelocityNew = LateralVelocityNew.normalized * _alteredMaxSpeed;
                else
                    LateralVelocityNew = LateralVelocityNew.normalized * LateralVelocityOld.magnitude;
            }

            //FRICTION
            //If the new lateral velocity is still greater than the max speed, reduce it by the relevant amount until it is AT the max speed.
            if (LateralVelocityNew.magnitude > maxSpeed)
            {
                if (_Grounded)
                    LateralVelocityNew = LateralVelocityNew.normalized * Mathf.Max(maxSpeed, LateralVelocityNew.magnitude - frictionForce);
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
        if (_Grounded && _onSomething && (desiredDirection.magnitude < 0.01f && !Input.GetButton("Jump")))
        {

            Vector3 NewVelocity = RB.velocity;

            //Jump to zero velocity when below max speed and on the ground to give more control and prevent gliding.
            if (RB.velocity.magnitude < _alteredMaxSpeed)
                RB.velocity = new Vector3();
            else
            {
                //Apply a 'friction' force to the player.
                DebugString = "Stopping";
                NewVelocity = NewVelocity.normalized * Mathf.Max(0, NewVelocity.magnitude - (stoppingForce * Time.deltaTime));
                RB.velocity = NewVelocity;
            }
        }
        else
        {
            if (Time.time < _wantToJump + jumpLeniency)
            {
                RB.velocity += -transform.forward * jumpVelocity;
                _lastJumpTime = Time.time;
            }

            _wantToJump = -5;

            if (Time.time < _lastJumpTime + maxJumpTime)
            {
                if (Input.GetButton("Jump"))
                    RB.velocity += -transform.forward * jumpVelocityPerSecondHeld * Time.deltaTime;
            }

            //Move the player the chosen direction (could move to fixed update to regulate speed).
            RB.velocity += FinalVelocityChange;// * Time.deltaTime;
        }
    }

    public void Pause() {
        _paused = !_paused;
        if (_paused)
        {
            if (!Input.GetKeyDown(KeyCode.BackQuote))
            {
                Menu.SetActive(true);
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

	private void Crouch () {
		_crouching = true;
		Body.localScale = new Vector3 (1, 0.5f, 1);
		Body.localPosition = new Vector3 (0, -0.5f, 0);
		MainCamera.transform.localPosition = new Vector3 (0, -0.08f, 0.226f);
	}

	private void UnCrouch () {
		_crouching = false;
		_unCrouching = true;
		Body.localScale = new Vector3 (1, 1, 1);
		Body.localPosition = new Vector3 (0, 0, 0);
		MainCamera.transform.localPosition = new Vector3 (0, 0.42f, 0.226f);
	}

    public void EnableMovement()
    {
        _movementEnabled = true;
        RB.isKinematic = false;
    }

    public void DisableMovement()
    {
        _movementEnabled = false;
        RB.isKinematic = true;
    }

    public void Teleport (Vector3 Location) {
		//Play sounds or animations here.
		transform.position = Location;
		RB.velocity = Vector3.zero;
    }

    public void Teleport(Vector3 Location, float XRot, float YRot, bool ResetVel)
    {
        //Play sounds or animations here.
        transform.position = Location;
        _CameraAngle = YRot;
        //transform.localRotation = Quaternion.AngleAxis(XRot, Vector3.forward);
        if (ResetVel)
            RB.velocity = Vector3.zero;
    }

    //MENU FUNCTIONS:

	public void SetSensitivity (GameObject TextObject) {
		userSensitivity = float.Parse(TextObject.GetComponent<Text>().text);
		DisplaySensitivity ();
	}

	private void DisplaySensitivity () {
		Menu.GetComponentsInChildren<MenuDisplayText>()[0].text = "Sensitivity = " + userSensitivity;
	}

	public void SetVolume (GameObject TextObject) {
		AudioListener.volume = Mathf.Clamp01 (float.Parse (TextObject.GetComponent<Text> ().text) / 10);
		DisplayVolume ();
	}

	private void DisplayVolume () {
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


    public delegate void CollisionEvent(Collision collision);
    [HideInInspector]
    public CollisionEvent collisionEvent;
    
	void OnCollisionEnter (Collision col)
    {
        //If this is a fake collision caused by 'uncrouching' and hitting your head on an object, re-crouch and cancel effect.
        if (_unCrouching)
        {
            _unCrouching = false;
            Crouch();
            return;
        }
        
        collisionEvent.Invoke(col);
	}

	void OnCollisionExit (Collision col) {
		_onSomething = false;
	}

	void OnCollisionStay (Collision col) {
		_onSomething = true;

		if (col.gameObject.CompareTag ("Softwall"))
			_OnSoftWall = true;
	}
}
