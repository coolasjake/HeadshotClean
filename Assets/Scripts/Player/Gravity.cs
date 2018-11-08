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

	private Rigidbody RB;
	private Movement PM;
	private GravityRep UIGyroscope;
	private GravityRep UINormalGravity;

	/// <summary> Represents the amount of force that should be added to the player (or object) per second, when gravity was set by the normal of a surface. </summary>
	private Vector3 GravityDirection;
	private Vector3 OriginalDirection;				//Original direction, so that the gravity magnitude can be changed by VariableGravity to a negative (and not lose its sign straight after).
	private float GravityMagnitude;					//Unsigned magnitude of gravity, also to enable VariableGravity to work.
	private Vector3 ShiftPosition;					//The position from which unaligned gravity was set, so that collisions before reaching the target don't align gravity too early.
	private RaycastHit TargetWall;					//The raycast hit of an unaligned gravity shift, used to find the normal to align to later, and the distance to it /\.
	public GravityType Type = GravityType.Normal;	//The current status of gravity, see GravityType for detail.

	private float AngleOldToNew = 0;
	private float FinishRotationTimer;
	/// <summary> The magnitude that gravity will be set to if it is changed. Used to have a 'custom gravity force' with a flexible number.</summary>
	private float PersonalScale = 0;
	public Vector3 StableSway;
	private ResourceMeter Meter;
	private BarDisplay DragonMeter;
	private AudioSource SFXPlayer;

	private Transform RemoteGravityTarget;
	private bool UsingRemoteGravity = false;

	public bool AllowRemoteGravity = true;
	public bool UseUnfinishedInputs = false;
	public bool ShiftAligns = false;
	public float AutoLockDistance;
	public float NormalGravityMagnitude = 9.8f;
	public float GsPerSecondHeld = 2;
	public float DegreesPerSecond = 180;
	/// <summary> Resource used per second that gravity is not normal. </summary>
	public float ResourcePerSecond = 1f;
	/// <summary> Resource used per gravity change (caused by player). </summary>
	public float ResourcePerUse = 5f;
	/// <summary> Resource used per second that gravity is not normal. </summary>
	public float StabilizingRPS = 2f;
	/// <summary> Multiplier on the impact force, which is used to calculate the resource loss from the impact. </summary>
	public float ImpactDamageMag = 2f;
	/// <summary> Affects how quickly the player will come to a halt while holding C. </summary>
	public float StabilizationForce = 2f;
	/// <summary> Multiplier for how fast the scroll wheel changes gravity. </summary>
	public float ScrollChangeRate = 2f;

	/// <summary>
	/// Maximum number of seconds to complete the players rotation after a gravity shift.
	/// </summary>
	[Range(0.1f, 2f)]
	public float RotationTime = 1;

	private Vector3 ChangeGravity {
		set {
			GravityDirection = value;
			OriginalDirection = GravityDirection;
			UIGyroscope.Down = GravityDirection;
			DragonMeter.ChangeValue (GravityMagnitude/(NormalGravityMagnitude * 2));
		}
	}

	// Use this for initialization
	void Start () {
		MaxResource = 50;
		RegenRate = 10;
		MinToUse = 10;
		Meter = FindObjectOfType<Canvas> ().GetComponentsInChildren<ResourceMeter> () [2];
		DragonMeter = FindObjectOfType<Canvas> ().GetComponentInChildren<BarDisplay> ();

		RB = GetComponent<Rigidbody> ();
		PM = GetComponent<Movement> ();
		SFXPlayer = GetComponentsInChildren<AudioSource> ()[1];
		UIGyroscope = GetComponentInChildren<GravityRep> ();
		UINormalGravity = GetComponentsInChildren<GravityRep> ()[1];
		UINormalGravity.Down = -Vector3.up * NormalGravityMagnitude;
		//GravityDirection = Physics.gravity;
		GravityMagnitude = Physics.gravity.magnitude;
		ChangeGravity = Physics.gravity;
		ResetGravity (1);
	}

	// Update is called once per frame
	void Update () {
		
		if (!Disabled) {
			//If 'Reset' or 'ChangeGravity' are pressed, change gravity (which checks if it is needed).
			if (Input.GetButtonDown ("GravityReset")) {
				//Disable gravity when [C] is pressed down.
				NoGravity ();
			} else if (Input.GetButtonUp ("GravityReset")) {
				//Reset gravity to normal when [C] is released, or half normal when [SHIFT] is also held.
				if (Input.GetButton ("AlignModifier"))
					ResetGravity (0.5f);
				else if (Type != GravityType.Normal) {
					ResetGravity (1f);
				}
			} else if (Input.GetButtonDown ("GravityNormal"))
				//Run the change gravity function when [F] is pressed.
				ContextualGravityShift ();
			else if (Input.GetButton ("Crouch") && !PM.Grounded)
				//Provide a tiny slowing force when [Ctrl] is HELD.
				Stabilize ();
			else if (Input.GetButton ("AlignModifier"))
				VariableGravity ();

			/*
			else if (Input.GetButton ("GravityNormal")) {
				VariableGravity ();
			}
			*/

			//Lock on to the surface the player collided with, if gravity isn't locked, the surface isn't an enemy, and if the 'modifier key' (SHIFT) is not held.
			if (PM.CheckForWallAlignment) {
				//Debug.Log ("Got message from Movement");
				PM.CheckForWallAlignment = false;

				//If Gravity is unaligned and shift is not being held AND
				//The distance to the target is less than 2, or the original distance to the target is less than 3 OR
				//The distance from the starting point is greater than the original distance from the starting point.
				if (Type == GravityType.Unaligned && !Input.GetButton ("AlignModifier") && (
					(Vector3.Distance(TargetWall.point, transform.position) < 2 || Vector3.Distance(TargetWall.point, ShiftPosition) < 3) ||
					(Vector3.Distance(ShiftPosition, transform.position) > Vector3.Distance(TargetWall.point, transform.position))))
				{
					Type = GravityType.Aligned;
					//bool PointingAtLevelGround = ((Physics.gravity.normalized - TargetWall.normal * -1).magnitude < 0.1f);
					ShiftGravityDirection (1, TargetWall.normal * -1, false);
				}
			}
		}

		//Apply the gravity force (so long as normal gravity isn't enabled).
		if (Type != GravityType.Normal)
			RB.velocity += GravityDirection * Time.deltaTime;

		//Remove resource if an impact has occured (impacts are a collision where the impulse is above a threshold).
		//The maximum reduction (Resource + 20) creates the delay after a strong collision while the resource value is in the negatives.
		if (PM.ImpactLastFrame)
			Resource -= Mathf.Clamp(PM.VelocityOfImpact * ImpactDamageMag, 0, Resource + 20);
		
		//Drain or regenerate the resource.
		if (Resource < MaxResource && (PM.OnSoftWall || (Type == GravityType.Normal && PM.Grounded))) {
			if (PM.OnSoftWall)
				Resource += RegenRate * Time.deltaTime * 2;
			else
				Resource += RegenRate * Time.deltaTime;
		} else if (Type != GravityType.Normal) {
			Resource -= Time.deltaTime;
			if (Resource <= 0)
				ResetGravity (1);
		}

		//Update the visual rep of the meter.
		Meter.ChangeValue (Resource / MaxResource);
	}

	private void VariableGravity () {
		GravityMagnitude += Input.mouseScrollDelta.y;

		if (GravityMagnitude > NormalGravityMagnitude * 2)
			GravityMagnitude = NormalGravityMagnitude * 2;
		else if (GravityMagnitude < -(NormalGravityMagnitude * 2))
			GravityMagnitude = -(NormalGravityMagnitude * 2);

		GravityDirection = OriginalDirection.normalized * GravityMagnitude;
		//UIGyroscope.Down = GravityDirection;
		DragonMeter.ChangeValue (GravityMagnitude/(NormalGravityMagnitude * 2));

		if (Type == GravityType.Normal && (GravityDirection - Physics.gravity).magnitude > 0.1f) {
			Type = GravityType.Aligned;
			RB.useGravity = false;
		}
	}

	/// <summary> Brings the player towards zero velocity, but not below 0.1, so it doesnt feel unnatural. </summary>
	private void Stabilize () {
		if (Resource > 0) {
			Resource -= StabilizingRPS * Time.deltaTime;
			if (RB.velocity.magnitude > 0.1f)
				//RB.velocity = RB.velocity.normalized * (RB.velocity.magnitude - (RB.velocity.magnitude * Time.deltaTime));
				RB.velocity = RB.velocity.normalized * (RB.velocity.magnitude - (StabilizationForce * Time.deltaTime));
		}
	}

	/// <summary> Change the rotation of the players body so that the 'feet' are pointing 'down' relative to the current gravity direction, and keep the facing (body-y / camera-x rot) as close to the original as manageable.
	/// NOTE: (accuracy is usually impossible since the position of the camera moves, and players focus is often different from their aim). </summary>
	private void IntuitiveSnapRotation () {

		Quaternion CameraPreRotation = PM.MainCamera.transform.rotation;
		Vector3 OriginalFacing = PM.MainCamera.transform.forward; //Remember that forward is down (the feet of the player) to let LookRotation work.

		//Rotate the players 'body'.
		transform.rotation = Quaternion.LookRotation (GravityDirection, GetComponentInChildren<GravityReference>().transform.right);
		transform.rotation = Quaternion.LookRotation (GravityDirection, GetComponentInChildren<GravityReference>().transform.forward);
		Quaternion NewRot = new Quaternion ();
		NewRot.eulerAngles = new Vector3 (transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
		transform.localRotation = NewRot;

		//Calculate the angle difference between the two rotations, then save the 'number of full rotations' it represents.
		float Signed = Vector3.SignedAngle (OriginalFacing, PM.MainCamera.transform.forward, transform.right);
		PM.CameraAngle -= Signed;
		PM.MainCamera.transform.rotation = CameraPreRotation;
	}

	/// <summary> Check if the target of the shift is within autolock distance, then call the gravity shift with the relevant values. </summary>
	private void ContextualGravityShift () {
		if (Physics.Raycast (PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out TargetWall)) {
			RaycastHit Hit;
			//Align if pointing to a wall 2 * AutoLockDistance away, or half the distance away if the modifier is held (SHIFT).
			if (TargetWall.distance < AutoLockDistance * 2 && !Input.GetButton ("AlignModifier") && Physics.Raycast (transform.position, TargetWall.normal * -1, out Hit)) {
				if (Hit.distance < AutoLockDistance && (Hit.normal - TargetWall.normal).magnitude < 0.1f) {
					Type = GravityType.Aligned;
					ShiftGravityDirection (1, TargetWall.normal * -1, true);
					return;
				}
			}
		}
		//Otherwise shift to the direction the player is pointing.
		Type = GravityType.Unaligned;
		ShiftGravityDirection (1, PM.MainCamera.transform.forward, true);
	}

	/// <summary> Set gravity to down, but with a tiny magnitude, to create the effect of Zero-Gravity. </summary>
	private void NoGravity () {
		RB.useGravity = false;
		//GravityDirection = Physics.gravity.normalized * 0.01f;
		GravityMagnitude = Physics.gravity.magnitude * 0.01f;
		ChangeGravity = Physics.gravity.normalized * 0.01f;
		Type = GravityType.Aligned;

		IntuitiveSnapRotation ();
		PM.CheckForWallAlignment = false;
		//SFXPlayer.Play ();
	}

	/// <summary> Shift gravity in the given direction, and apply the given GravityMultiplier to the force. Checks for repeat shifts and shifts setting gravity back to normal, and doesn't use resource for them. </summary>
	private void ShiftGravityDirection (float GravityMultiplier, Vector3 Direction, bool UseResource) {
		Vector3 NewGravity = Direction * NormalGravityMagnitude * GravityMultiplier;

		if ((NewGravity - Physics.gravity).magnitude < 0.1f) {
			//If the new gravity is less than 10% different from the normal gravity, set gravity to normal.
			ResetGravity (GravityMultiplier);
		} else if ((Resource > MinToUse || !UseResource) && (GravityDirection - NewGravity).magnitude > 0.1f) {
			//If resource is high enough, and the new gravity is not the same as the current one (with 10% leeway), set the gravity to it.
			if (UseResource)
				Resource -= ResourcePerUse;
			RB.useGravity = false;
			//GravityDirection = NewGravity;
			GravityMagnitude = NewGravity.magnitude;
			ChangeGravity = NewGravity;
			OriginalDirection = NewGravity.normalized;
			if (Type == GravityType.Normal)
				Type = GravityType.Aligned;
		}

		IntuitiveSnapRotation ();
		PM.CheckForWallAlignment = false;
		SFXPlayer.Play ();

		ShiftPosition = transform.position;
	}

	private void ResetGravity(float GravityMultiplier) {
		//If we are changing gravity:
		//If making gravity normal, and gravity is not already normal OR we are giving gravity a diffent magnitude to it's current one:
		//--->Play a sound effect, and rotate the players body.
		bool Rotate = false;
		if ((GravityMultiplier == 1 && GravityDirection != Physics.gravity) || (GravityDirection.magnitude != Physics.gravity.magnitude * GravityMultiplier)) {
			SFXPlayer.Play ();
			Rotate = true; //If this is false, the magnitude code might still need to run, but the rotation code doesn't. (also still give feedback ;)
		}
			
		//Reset Gravity.
		if (GravityMultiplier != 1) {
			if (Resource > MinToUse) {
				Resource -= ResourcePerUse;
				Type = GravityType.Aligned;
				GravityMagnitude = (Physics.gravity * GravityMultiplier).magnitude;
				ChangeGravity = Physics.gravity * GravityMultiplier;
			} else {
				//Play ability failed SFX
			}
		} else {
			Type = GravityType.Normal;
			RB.useGravity = true;
			GravityMagnitude = Physics.gravity.magnitude;
			ChangeGravity = Physics.gravity;
		}
		
		PM.CheckForWallAlignment = false;

		//Reset player rotation.
		if (Rotate) {
			IntuitiveSnapRotation ();
			/*
			Quaternion CameraPreRotation = PM.MainCamera.transform.rotation;
			Vector3 OriginalFacing = PM.MainCamera.transform.forward;

			//Rotate the players 'body'.
			transform.rotation = Quaternion.LookRotation (GravityDirection, GetComponentInChildren<GravityReference>().transform.right);
			transform.rotation = Quaternion.LookRotation (GravityDirection, GetComponentInChildren<GravityReference>().transform.forward);
			Quaternion NewRot = new Quaternion ();
			NewRot.eulerAngles = new Vector3 (transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
			transform.localRotation = NewRot;

			//Calculate the angle difference between the two rotations, then save the 'number of full rotations' it represents.
			float Signed = Vector3.SignedAngle (OriginalFacing, PM.MainCamera.transform.forward, transform.right);
			PM.CameraAngle -= Signed;
			PM.CameraSpin = PM.MainCamera.transform.localRotation.eulerAngles.y;
			*/
		}
	}

	/// <summary> Change which is the default gravity setting (Align or Camera Angle), and therefore which one SHIFT will switch to when held. </summary>
	public void ToggleGravitySHIFTSetting () {
		ShiftAligns = !ShiftAligns;
	}
}