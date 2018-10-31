/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GravityType {
	Normal,
	Point,
	Direction,
	Locked
}

[RequireComponent(typeof(Movement))]
public class Gravity : PlayerAbility {

	private Rigidbody RB;
	private Movement PM;

	//----Old System Variables----
	//----Used with the old system of figuring out the possible gravity directions based on the 'target wall', and THEN choosing.----
	/// <summary> Represents the amount of force that should be added to the player (or object) per second, when gravity was set by the normal of a surface. </summary>
	private Vector3 DirectionalGravity;
	/// <summary> Represents the amount of force that should be added to the player (or object) per second, when gravity was set by a point in space, usually on the surface of an object. </summary>
	private Vector3 PointGravity;
	private RaycastHit TargetWall;

	/// <summary> The amount of force that should be added to the player (or object) per second. </summary>
	private Vector3 GravityDirection;

	private Vector3 TargetFacing;
	private bool NeedToRotate = false;
	private bool RotatePlayer = false;
	private bool RotateCamera = false;
	private Quaternion DesiredCameraRot = new Quaternion ();
	private float AngleOldToNew = 0;
	private float FinishRotationTimer;
	/// <summary> The magnitude that gravity will be set to if it is changed. Used to have a 'custom gravity force' with a flexible number.</summary>
	private float PersonalScale = 0;
	private ResourceMeter Meter;
	private AudioSource SFXPlayer;

	private Transform RemoteGravityTarget;
	private bool UsingRemoteGravity = false;

	public bool AllowRemoteGravity = true;
	public bool UseUnfinishedInputs = false;
	public bool ShiftAligns = false;
	public GravityType Type = GravityType.Normal;
	public float GravityMagnitude = 9.8f;
	public float GsPerSecondHeld = 2;
	public float DegreesPerSecond = 180;

	/// <summary>
	/// Maximum number of seconds to complete the players rotation after a gravity shift.
	/// </summary>
	[Range(0.1f, 2f)]
	public float RotationTime = 1;

	// Use this for initialization
	void Start () {
		MaxResource = 50;
		RegenRate = 10;
		MinToUse = 10;
		Meter = FindObjectOfType<Canvas> ().GetComponentsInChildren<ResourceMeter> () [2];

		RB = GetComponent<Rigidbody> ();
		PM = GetComponent<Movement> ();
		SFXPlayer = GetComponentsInChildren<AudioSource> ()[1];
		DirectionalGravity = Physics.gravity;
		ResetGravity (1);
	}

	// Update is called once per frame
	void Update () {

		if (!Disabled) {
			//Take any inputs relevant to gravity shifting, and call the relevant function.
			if (Input.GetKeyDown (KeyCode.V) && UseUnfinishedInputs) {
				SetGravitySpecial (1);
			} else if (Input.GetButton("CustomGravity")) {

				if (Input.GetButton("GravityReset") || Input.GetButton("GravityNormal")) {
					PersonalScale += Time.deltaTime * GsPerSecondHeld;
				}

				if (Input.GetButtonUp("GravityReset")) {
					ResetGravity (PersonalScale);
					PersonalScale = 0;
				} else if (Input.GetButtonUp("GravityNormal")) {
					if (Physics.Raycast (PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out TargetWall))
						ShiftGravityDirection (PersonalScale);
					PersonalScale = 0;
				}
			} else if (AllowRemoteGravity && Input.GetButtonDown("RemoteGravity")) {
				Debug.Log ("Remote Gravity Started");
				RaycastHit Hit;
				if (Physics.Raycast (PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out Hit)) {
					BaseEnemy Enemy = Hit.transform.GetComponentInParent<BaseEnemy> ();
					if (Enemy) {
						RemoteGravityTarget = Enemy.transform;
						UsingRemoteGravity = true;
					}
				}
			} else if (AllowRemoteGravity && Input.GetButtonUp("RemoteGravity") && UsingRemoteGravity) {
				Debug.Log ("Remote Gravity Released");
				RaycastHit Hit;
				if (Physics.Raycast (PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out Hit)) {
					if (RemoteGravityTarget != null) {
						if (!RemoteGravityTarget.gameObject.GetComponent<RemoteGravity>())
							RemoteGravityTarget.gameObject.AddComponent<RemoteGravity> ();
						RemoteGravityTarget.GetComponent<RemoteGravity> ().DirectionalGravity = (Hit.point - RemoteGravityTarget.position).normalized * GravityMagnitude * 2;
					}
					UsingRemoteGravity = false;
				}
			} else {
				
				if (Input.GetButtonDown("GravityReset")) {
					if (Input.GetButton("AlignModifier"))
						ResetGravity (0.5f);
					else if (Type != GravityType.Normal) {
						ResetGravity (1f);
					}
				} else if (Input.GetButtonDown("GravityDouble")) {
					if (Physics.Raycast (PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out TargetWall))
						ShiftGravityDirection (2);
				} else if (Input.GetButtonDown("GravityHalf")) {
					if (Physics.Raycast (PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out TargetWall))
						ShiftGravityDirection (0.5f);
				} else if (Input.GetButtonDown("GravityNormal")) {
					if (Physics.Raycast (PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out TargetWall))
						ShiftGravityDirection (1);
				}

			}

		}
		//(End of inputs and disableable code)

		//Update the scale and rotation of the 'Gyroscope'.
		GetComponentInChildren<GravityRep> ().transform.rotation = Quaternion.LookRotation(DirectionalGravity);
		GetComponentInChildren<GravityRep> ().transform.localScale = new Vector3 (1, 1, DirectionalGravity.magnitude / GravityMagnitude);

		//Apply the gravity force (so long as normal gravity isn't enabled).
		if (Type != GravityType.Normal) {
			RB.velocity += DirectionalGravity * Time.deltaTime;
		}

		//Rotate Player.
		if (RotatePlayer) {
		}

		//Rotate towards target.
		if (RotatePlayer) {

			Quaternion OldCameraRotation = PM.MainCamera.transform.rotation;
			//float ZAngle = transform.localRotation.eulerAngles.z;
			transform.rotation = Quaternion.LookRotation (DirectionalGravity, GetComponentInChildren<GravityReference>().transform.right);
			transform.rotation = Quaternion.LookRotation (DirectionalGravity, GetComponentInChildren<GravityReference>().transform.forward);
			PM.MainCamera.transform.rotation = OldCameraRotation;
			Quaternion NewRot = new Quaternion ();
			NewRot.eulerAngles = new Vector3 (transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
			transform.localRotation = NewRot;
			//transform.rotation = Quaternion.FromToRotation (transform.up, TargetFacing);
			NeedToRotate = false;
			PM.DisableMouseInput = false;
		}


		//Remove resource if an impact has occured (impacts are a collision with impulse above a threshold).
		if (PM.ImpactLastFrame)
			Resource -= Mathf.Clamp(PM.VelocityOfImpact * 2, 0, Resource + 20);

		//Drain or regenerate the resource.
		if (Type == GravityType.Normal && Resource < MaxResource && PM.Grounded)
			Resource += RegenRate * Time.deltaTime;
		else if (Type != GravityType.Normal) {
			Resource -= Time.deltaTime;
			if (Resource <= 0)
				ResetGravity (1);
		}

		//Update the visual rep of the meter.
		Meter.ChangeValue (Resource / MaxResource);
	}

	/// <summary> Rotate the players body so that their feet are pointing towards the direction of gravity, and try to rotate the camera so that it is pointing in the same direction as before, but only by changing the x-axis angle. </summary>
	private void SnapRotation () {

		Quaternion CameraPreRotation = PM.MainCamera.transform.rotation;

		//Rotate the players 'body'.
		transform.rotation = Quaternion.LookRotation (DirectionalGravity, GetComponentInChildren<GravityReference> ().transform.right);
		//transform.rotation = Quaternion.LookRotation (DirectionalGravity, GetComponentInChildren<GravityReference> ().transform.forward);
		Quaternion NewRot = new Quaternion ();
		NewRot.eulerAngles = new Vector3 (transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
		transform.localRotation = NewRot;

		//Calculate the angle difference between the two rotations, then save the 'number of full rotations' it represents.
		AngleOldToNew = Quaternion.Angle(CameraPreRotation, PM.MainCamera.transform.rotation);
		PM.CameraAngle += AngleOldToNew;

		PM.MainCamera.transform.rotation = CameraPreRotation;
	}

	/// <summary> Shift gravity in either the direction of the camera, or the direction of a surface normal (based on the align key; default shift), and apply the given GravityMultiplier to the force. </summary>
	private void ShiftGravityDirection (float GravityMultiplier) {
		if (Resource > MinToUse) {
			Resource -= 5;

			bool Unlocked = Input.GetButton ("AlignModifier");

			if (!Unlocked ^ ShiftAligns) {
				//FACE - Shift is OFF.
				//Set Gravity to align with the FACE the player is looking at.
				Type = GravityType.Direction;
				DirectionalGravity = TargetWall.normal * GravityMagnitude * GravityMultiplier * -1;
			} else {
				//POINT - Shift is ON.
				//Set Gravity to face the POINT the player is looking at.
				Type = GravityType.Point;
				DirectionalGravity = PM.MainCamera.transform.forward * GravityMagnitude * GravityMultiplier;
			}
			RB.useGravity = false;

			if ((DirectionalGravity - Physics.gravity).magnitude < 0.1f) {
				Type = GravityType.Normal;
				RB.useGravity = true;
				DirectionalGravity = Physics.gravity;
			}

			GetComponentInChildren<GravityRep> ().Down = DirectionalGravity;
			FinishRotationTimer = RotationTime;
			SnapRotation ();
			SFXPlayer.Play ();
		} else {
			//Ability failed SFX
		}
	}

	/// <summary> Shift gravity based on the keys currently being pressed (space/ctrl = up/down). CURRENTLY NOT IN USE</summary>
	private void SetGravitySpecial(float GravityMultiplier) {

		Type = GravityType.Locked;
		RB.useGravity = false;
		Vector3 DirectionFromInputs = new Vector3 ();;

		if (Input.GetKey (KeyCode.D))
			DirectionFromInputs += PM.MainCamera.transform.right;
		if (Input.GetKey (KeyCode.A))
			DirectionFromInputs -= PM.MainCamera.transform.right;
		if (Input.GetKey (KeyCode.W))
			DirectionFromInputs += PM.MainCamera.transform.forward;
		if (Input.GetKey (KeyCode.S))
			DirectionFromInputs -= PM.MainCamera.transform.forward;
		if (Input.GetKey (KeyCode.Space))
			DirectionFromInputs += PM.MainCamera.transform.up;
		else if (!Input.GetKey (KeyCode.LeftControl))
			DirectionFromInputs -= PM.MainCamera.transform.up;

		Debug.Log (DirectionFromInputs.normalized);

		DirectionalGravity = DirectionFromInputs.normalized * GravityMagnitude * GravityMultiplier;

		SnapRotation ();
		FinishRotationTimer = RotationTime;
	}

	private void ResetGravity(float GravityMultiplier) {
		//If we are changing gravity:
		//If making gravity normal, and gravity is not already normal OR we are giving gravity a diffent magnitude to it's current one:
		bool Rotate = false;
		if ((GravityMultiplier == 1 && DirectionalGravity != Physics.gravity) || (DirectionalGravity.magnitude != Physics.gravity.magnitude * GravityMultiplier)) {
			SFXPlayer.Play ();
			Rotate = true; //If this is false, the magnitude code might still need to run, but the rotation code doesn't. (also still give feedback ;)
		}
			
		//Reset Gravity.
		if (GravityMultiplier != 1) {
			if (Resource > MinToUse) {
				Resource -= 5;
					
				Type = GravityType.Locked;
				DirectionalGravity = Physics.gravity * GravityMultiplier;
			} else {
				//Play ability failed SFX
			}
		} else {
			
			Type = GravityType.Normal;
			RB.useGravity = true;
			DirectionalGravity = Physics.gravity;
		}

		GetComponentInChildren<GravityRep> ().Down = DirectionalGravity;

		//Reset player rotation.
		//TargetRotation = new Quaternion ();
		//TargetRotation.SetLookRotation (Vector3.forward);
		//TargetFacing = Vector3.up;

		if (Rotate) {
			Quaternion CameraPreRotation = PM.MainCamera.transform.rotation;

			//Rotate the players 'body'.
			transform.rotation = Quaternion.LookRotation (DirectionalGravity, GetComponentInChildren<GravityReference> ().transform.right);
			transform.rotation = Quaternion.LookRotation (DirectionalGravity, GetComponentInChildren<GravityReference> ().transform.forward);
			Quaternion NewRot = new Quaternion ();
			NewRot.eulerAngles = new Vector3 (transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
			transform.localRotation = NewRot;

			//Calculate the angle difference between the two rotations, then save the 'number of full rotations' it represents.
			AngleOldToNew = Quaternion.Angle(CameraPreRotation, PM.MainCamera.transform.rotation);
			PM.CameraAngle -= AngleOldToNew;
			PM.CameraSpin = PM.MainCamera.transform.localRotation.eulerAngles.y;
		}
		FinishRotationTimer = RotationTime;
	}

	/// <summary> Change which is the default gravity setting (Align or Camera Angle), and therefore which one SHIFT will switch to when held. </summary>
	public void ToggleGravitySHIFTSetting () {
		Debug.Log ("Called. " + ShiftAligns);
		ShiftAligns = !ShiftAligns;
	}

	//Regenerate resource when resting on or against a SoftWall.
	void OnCollisionStay (Collision col) {
		if (col.transform.GetComponentInParent<SoftWall> () && Resource < MaxResource) {
			Resource += Time.deltaTime + (Time.deltaTime * RegenRate);
		}
	}
}
*/