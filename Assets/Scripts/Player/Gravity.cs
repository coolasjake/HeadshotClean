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
	private Vector3 OriginalDirectionNorm;
	private float GravityMagnitude;
	private Vector3 ShiftPosition;
	private RaycastHit TargetWall;
	public GravityType Type = GravityType.Normal;

	private Vector3 TargetFacing;
	private bool NeedToRotate = false;
	private bool RotatePlayer = false;
	private bool RotateCamera = false;
	private Quaternion DesiredCameraRot = new Quaternion ();
	private float AngleOldToNew = 0;
	private float FinishRotationTimer;
	/// <summary> The magnitude that gravity will be set to if it is changed. Used to have a 'custom gravity force' with a flexible number.</summary>
	private float PersonalScale = 0;
	public Vector3 StableSway;
	private ResourceMeter Meter;
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

	// Use this for initialization
	void Start () {
		MaxResource = 50;
		RegenRate = 10;
		MinToUse = 10;
		Meter = FindObjectOfType<Canvas> ().GetComponentsInChildren<ResourceMeter> () [2];

		RB = GetComponent<Rigidbody> ();
		PM = GetComponent<Movement> ();
		SFXPlayer = GetComponentsInChildren<AudioSource> ()[1];
		UIGyroscope = GetComponentInChildren<GravityRep> ();
		UINormalGravity = GetComponentsInChildren<GravityRep> ()[1];
		UINormalGravity.Down = -Vector3.up * NormalGravityMagnitude;
		GravityDirection = Physics.gravity;
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
			else if (Input.GetButton ("GravityNormal")) {
				//if (Input.mouseScrollDelta != 0)
				float OldMag = GravityDirection.magnitude;
				GravityMagnitude += Input.mouseScrollDelta.y;
				GravityDirection = OriginalDirectionNorm.normalized * GravityMagnitude;
				//Debug.Log ("Gravity changed from " + OldMag + " to " + GravityMagnitude);
			} else if (Input.GetButton ("Crouch") && !PM.Grounded)
				//Provide a tiny slowing force when [Ctrl] is HELD.
				Stabilize ();

			//Lock on to the surface the player collided with, if gravity isn't locked, the surface isn't an enemy, and if the 'modifier key' (SHIFT) is not held.
			if (PM.CheckForWallAlignment) {
				//Debug.Log ("Got message from Movement");
				PM.CheckForWallAlignment = false;

				//If Gravity is unaligned and shift is not being held AND
				//The distance to the target is less than 2, or the original distance to the target is less than 3 OR
				//The distance from the starting point is greater than the original distance from the starting point.
				if (Type == GravityType.Unaligned && !Input.GetButton ("AlignModifier") && (
					(Vector3.Distance(TargetWall.point, transform.position) < 2 || Vector3.Distance(TargetWall.point, ShiftPosition) < 3) ||
					(Vector3.Distance(ShiftPosition, transform.position) > Vector3.Distance(TargetWall.point, transform.position)))) {

					//Debug.Log ("Conditions met, aligning gravity");
					Type = GravityType.Aligned;
					//bool PointingAtLevelGround = ((Physics.gravity.normalized - TargetWall.normal * -1).magnitude < 0.1f);
					ShiftGravityDirection (1, TargetWall.normal * -1);

				}
			}
		}

		//Update the scale and rotation of the 'Gyroscope'.
		//UIGyroscope.transform.rotation = Quaternion.LookRotation(GravityDirection);
		//UIGyroscope.transform.localScale = new Vector3 (1, 1, GravityDirection.magnitude / NormalGravityMagnitude);

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

	/// <summary> Brings the player towards zero velocity, then simulates a smooth swaying to feel more natural. </summary>
	private void Stabilize () {
		if (Resource > 0) {
			Resource -= StabilizingRPS * Time.deltaTime;
			if (RB.velocity.magnitude > 0.1f)
				//RB.velocity = RB.velocity.normalized * (RB.velocity.magnitude - (RB.velocity.magnitude * Time.deltaTime));
				RB.velocity = RB.velocity.normalized * (RB.velocity.magnitude - (StabilizationForce * Time.deltaTime));
		}
		//else {
		//	if ((RB.velocity - StableSway).magnitude < 0.1f)
		//		StableSway = Random.insideUnitSphere * 0.5f;
		//	RB.velocity = Vector3.MoveTowards (RB.velocity, StableSway, 1f * Time.deltaTime);
		//}
	}

	/// <summary> Rotate the players body so that their feet are pointing towards the direction of gravity, and try to rotate the camera so that it is pointing in the same direction as before, but only by changing the x-axis angle. </summary>
	private void SnapRotation () {

		Quaternion CameraPreRotation = PM.MainCamera.transform.rotation;

		//Rotate the players 'body'.
		transform.rotation = Quaternion.LookRotation (GravityDirection, GetComponentInChildren<GravityReference>().transform.right);
		//transform.rotation = Quaternion.LookRotation (DirectionalGravity, GetComponentInChildren<GravityReference>().transform.forward);
		Quaternion NewRot = new Quaternion ();
		NewRot.eulerAngles = new Vector3 (transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
		transform.localRotation = NewRot;

		//Calculate the angle difference between the two rotations, then save the 'number of full rotations' it represents.
		AngleOldToNew = Quaternion.Angle(CameraPreRotation, PM.MainCamera.transform.rotation);
		PM.CameraAngle += AngleOldToNew;

		PM.MainCamera.transform.rotation = CameraPreRotation;
	}

	private void IntuitiveSnapRotation () {

		Quaternion CameraPreRotation = PM.MainCamera.transform.rotation;

		Vector3 OriginalFacing = PM.MainCamera.transform.forward; //Remember that forward is down (the feet of the player) to let LookRotation work.

		//Rotate the players 'body'.
		transform.rotation = Quaternion.LookRotation (GravityDirection, GetComponentInChildren<GravityReference>().transform.right);
		//transform.rotation = Quaternion.LookRotation (DirectionalGravity, GetComponentInChildren<GravityReference>().transform.forward);
		Quaternion NewRot = new Quaternion ();
		NewRot.eulerAngles = new Vector3 (transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
		transform.localRotation = NewRot;

		//Calculate the angle difference between the two rotations, then save the 'number of full rotations' it represents.
		//AngleOldToNew = Quaternion.Angle(CameraPreRotation, PM.MainCamera.transform.rotation);
		float Signed = Vector3.SignedAngle (OriginalFacing, PM.MainCamera.transform.forward, transform.right);
		PM.CameraAngle -= Signed;

		PM.MainCamera.transform.rotation = CameraPreRotation;
	}

	private void ContextualGravityShift () {
		if (Physics.Raycast (PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out TargetWall)) {
			//Debug.Log ("Current: " + Physics.gravity.normalized + ", New: " + (TargetWall.normal * -1) + "Sum: " + (Physics.gravity.normalized - TargetWall.normal * -1).magnitude);
			//bool PointingAtLevelGround = false;//((Physics.gravity.normalized - TargetWall.normal * -1).magnitude < 0.1f);
			RaycastHit Hit;
			//Align if pointing to a wall 2 * AutoLockDistance away, or half the distance away if the modifier is held (SHIFT).
			if (TargetWall.distance < AutoLockDistance * 2 && !Input.GetButton ("AlignModifier") && Physics.Raycast (transform.position, TargetWall.normal * -1, out Hit)) {
				if (Hit.distance < AutoLockDistance && (Hit.normal - TargetWall.normal).magnitude < 0.1f) {
					Type = GravityType.Aligned;
					ShiftGravityDirection (1, TargetWall.normal * -1);
					return;
				}
			}
			//Otherwise shift to the direction the player is pointing.
			Type = GravityType.Unaligned;
			ShiftGravityDirection (1, PM.MainCamera.transform.forward);
		}
	}

	private void NoGravity () {
		RB.useGravity = false;
		GravityDirection = Physics.gravity.normalized * 0.01f;
		UIGyroscope.Down = GravityDirection;
		Type = GravityType.Aligned;

		IntuitiveSnapRotation ();
		PM.CheckForWallAlignment = false;
		//SFXPlayer.Play ();
	}

	/// <summary> Shift gravity in either the direction of the camera, or the direction of a surface normal (based on the align key; default shift), and apply the given GravityMultiplier to the force. </summary>
	private void ShiftGravityDirection (float GravityMultiplier, Vector3 Direction) {
		Vector3 NewGravity = Direction * NormalGravityMagnitude * GravityMultiplier;

		if ((NewGravity - Physics.gravity).magnitude < 0.1f) {
			//If the new gravity is less than 10% different from the normal gravity, set gravity to normal.
			ResetGravity (GravityMultiplier);
		} else if (Resource > MinToUse && (GravityDirection - NewGravity).magnitude > 0.1f) {
			//If resource is high enough, and the new gravity is not the same as the current one (with 10% leeway), set the gravity to it.
			Resource -= ResourcePerUse;
			RB.useGravity = false;
			GravityDirection = NewGravity;
			OriginalDirectionNorm = NewGravity.normalized;
			GravityMagnitude = NewGravity.magnitude;
			UIGyroscope.Down = GravityDirection;
			if (Type == GravityType.Normal)
				Type = GravityType.Aligned;
		}

		/*
		else if (PointingAtLevelGround) {
			//If the player is pointing at level ground which is close enough (given through bool), set gravity to normal.
			ResetGravity (GravityMultiplier);
		} 
		*/

		//SnapRotation ();
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
				GravityDirection = Physics.gravity * GravityMultiplier;
			} else {
				//Play ability failed SFX
			}
		} else {
			Type = GravityType.Normal;
			RB.useGravity = true;
			GravityDirection = Physics.gravity;
		}

		UIGyroscope.Down = GravityDirection;
		PM.CheckForWallAlignment = false;

		//Reset player rotation.

		if (Rotate) {
			Quaternion CameraPreRotation = PM.MainCamera.transform.rotation;
			Vector3 OriginalFacing = PM.MainCamera.transform.forward;

			//Rotate the players 'body'.
			transform.rotation = Quaternion.LookRotation (GravityDirection, GetComponentInChildren<GravityReference>().transform.right);
			transform.rotation = Quaternion.LookRotation (GravityDirection, GetComponentInChildren<GravityReference>().transform.forward);
			Quaternion NewRot = new Quaternion ();
			NewRot.eulerAngles = new Vector3 (transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
			transform.localRotation = NewRot;

			//Calculate the angle difference between the two rotations, then save the 'number of full rotations' it represents.
			//AngleOldToNew = Quaternion.Angle(CameraPreRotation, PM.MainCamera.transform.rotation);
			float Signed = Vector3.SignedAngle (OriginalFacing, PM.MainCamera.transform.forward, transform.right);
			PM.CameraAngle -= Signed;
			//PM.CameraAngle -= AngleOldToNew;
			PM.CameraSpin = PM.MainCamera.transform.localRotation.eulerAngles.y;
		}
	}

	/// <summary> Change which is the default gravity setting (Align or Camera Angle), and therefore which one SHIFT will switch to when held. </summary>
	public void ToggleGravitySHIFTSetting () {
		ShiftAligns = !ShiftAligns;
	}

	//Lock on to the surface the player collided with, if gravity isn't locked, the surface isn't an enemy, and if the 'modifier key' (SHIFT) is not held.
	/* void OnCollisionEnter (Collision col) {
		if (Type == GravityType.Unaligned && !Input.GetButton ("AlignModifier")) {
			BaseEnemy EnemyCollision = col.gameObject.GetComponentInParent<BaseEnemy> ();
			if (!EnemyCollision) {
				Type = GravityType.Aligned;
				ShiftGravityDirection (1, TargetWall.normal * -1);
			}
		}
	}*/
}