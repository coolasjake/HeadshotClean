using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InputMode {
	Toggle,
	Hold,
	Tap
}


public class ChangedObject {
	public ElectricalComponent visibleObject;
	public Material originalMaterial;

	public ChangedObject (ElectricalComponent VisibleObject, Material material) {
		visibleObject = VisibleObject;
		originalMaterial = visibleObject.GetComponent<Renderer>().sharedMaterial;
		visibleObject.GetComponent<Renderer>().material = material;
	}

	public void Reset () {
		if (visibleObject != null)
			visibleObject.GetComponent<Renderer>().material = originalMaterial;
	}
}

[RequireComponent(typeof(Movement))]
public class Phasing : PlayerAbility {

	//private PhaseFailsafe FailSafe;
	private Canvas C;
	private Image ShadowEffect;
	private Movement PM;
	private Image IntersectWarning;
	private Image RobotIntersectWarning;
	private ResourceMeter Meter;
	private Camera PhaseCamera;
	private TriggerChecker CameraCheck;
	//private float PhaseResource = 3f;
	private bool CurrentlyPhasing = false;
	private bool UnphaseQueued = false;
	private float TapDelayTime;
	private AudioSource SFXPlayer;
	private int PhaseMask;

	public float FailsafeCapsuleRadius = 0.5f;
	public InputMode Mode = InputMode.Tap;
	public float TapDelay = 0.2f;
	public float MaxPhaseTime = 3f;
	public float XRayDistance = 20f;
	public Material Silhoutte;
	public List<ChangedObject> OriginalMaterials = new List<ChangedObject>();

	// Use this for initialization
	void Start () {
		PM = GetComponent<Movement> ();

		PhaseMask = 1 << 0 | 1 << 11 | 1 << 16 | 1 << 17;

		/*
		GameObject Child = new GameObject ();
		Child.transform.SetParent (transform);
		Child.transform.localPosition = Vector3.zero;
		Child.transform.localRotation = new Quaternion ();
		Child.name = "TC: Phase Failsafe";
		Child.layer = 14;
		CapsuleCollider CC = Child.AddComponent<CapsuleCollider> ();
		CC.direction = 2;
		CC.radius = FailsafeCapsuleRadius * 0.99f;
		CC.height = PM.PlayerSphereSize * 0.99f;
		CC.isTrigger = true;
		FailSafe = Child.AddComponent<PhaseFailsafe> ();
		*/

		C = FindObjectOfType<Canvas> ();
		ShadowEffect = C.GetComponentInChildren<Image> ();
		IntersectWarning = C.GetComponentsInChildren<Image> () [1];
		RobotIntersectWarning = C.GetComponentsInChildren<Image> () [2];
		Meter = C.GetComponentInChildren<ResourceMeter> ();
		RobotIntersectWarning.enabled = false;
		PhaseCamera = GetComponentsInChildren<Camera> () [2];
		PhaseCamera.enabled = false;
		ShadowEffect.enabled = false;
		SFXPlayer = GetComponentsInChildren<AudioSource> ()[2];

		var GO = new GameObject ();
		GO.transform.SetParent (transform);
		GO.transform.localPosition = Vector3.zero;
		GO.transform.localRotation = new Quaternion ();
		GO.name = "TC: Camera Check";
		GO.layer = 14;
		CameraCheck = GO.AddComponent<TriggerChecker>();
		var SC = GO.AddComponent<SphereCollider> ();
		SC.radius = 0.2f;
		SC.isTrigger = true;
		SC.center = new Vector3 (0, 0.3f, -0.4f);
		/*
		*/

		//StopPhasing ();
	}
	
	// Update is called once per frame
	void Update () {
		if (CurrentlyPhasing) {
			Resource -= Time.deltaTime;
			if (CameraCheck.Triggered) {
				PhaseCamera.enabled = true;
			} else
				PhaseCamera.enabled = false;
		} else {
			if (Resource < MaxResource)
				Resource += RegenRate * Time.deltaTime;
		}


		bool Intersecting = false;
		if (CheckBodyIntersections ())
			Intersecting = true;
		else if (UnphaseQueued)
			StopPhasing ();
		IntersectWarning.enabled = (Intersecting && CurrentlyPhasing);
		//RobotIntersectWarning.enabled = FailSafe.InsideEnemy;

		if (Mode == InputMode.Toggle) {
			if (Input.GetButtonDown("Phase") && !Disabled) {
				if (CurrentlyPhasing) {
					if (!Intersecting)
						StopPhasing ();
					else
						UnphaseQueued = true;
				} else
					StartPhasing ();
			}
		} else if (Mode == InputMode.Hold) {

			if (Input.GetButtonDown("Phase") && !Disabled)
				StartPhasing ();
			else if (Input.GetButtonUp("Phase") && !Disabled) {
				if (!Intersecting)
					StopPhasing ();
			} else if (!Input.GetButton("Phase") && !Disabled) {
				if (CurrentlyPhasing && !Intersecting)
					StopPhasing ();
			}
		} else {
			if (TapDelayTime >= 0)
				TapDelayTime -= Time.deltaTime;

			if (Input.GetButtonDown("Phase") && !Disabled)
				StartPhasing ();
			else if (!Input.GetButton("Phase") && !Disabled) {
				if (TapDelayTime <= 0) {
					TapDelayTime = -1;
					if (CurrentlyPhasing && !Intersecting) {
						//Debug.Log ("Trying to stop");
						StopPhasing ();
					}
				}
			}
		}

		if (Resource <= 0) {
			if (!CheckBodyIntersections ())
			//if (!FailSafe.InsideSomething)
				StopPhasing ();
			else
				PM.Hit (Time.deltaTime);
		}

		Meter.ChangeValue (Resource/MaxResource);
	}

	private bool CheckBodyIntersections () {
		float HalfHeight = PM.PlayerSphereSize / 2;
		return Physics.CheckCapsule (transform.position + transform.up * HalfHeight, transform.position - transform.up * HalfHeight, PM.PlayerWaistSize, PhaseMask);
	}

	private bool CheckHeadIntersections () {
		return Physics.CheckSphere (transform.position + transform.up * 0.3f + transform.forward * -0.4f, 0.2f, PhaseMask);
	}

	private BaseEnemy CheckEnemyIntersections () {
		int EnemiesOnly = 1 << 12;
		float HalfHeight = PM.PlayerSphereSize / 2;
		Collider[] Hits = Physics.OverlapCapsule (transform.position + transform.up * HalfHeight, transform.position - transform.up * HalfHeight, PM.PlayerWaistSize, EnemiesOnly);
		if (Hits.Length > 0)
			return Hits[0].GetComponent<BaseEnemy>();
		else
			return null;
	}

	private void StartPhasing () {
		if (Resource > MinToUse) {
			if (Mode == InputMode.Tap)
				TapDelayTime = TapDelay;
			PM.DisableAbilities (this);
			gameObject.layer = 9;
			foreach (Transform Child in GetComponentsInChildren<Transform>()) {
				if (Child.gameObject.layer == 8)
					Child.gameObject.layer = 9;
			}
			ShadowEffect.enabled = true;
			if (CurrentlyPhasing)
				return;
			CurrentlyPhasing = true;
			foreach (ElectricalComponent VisibleComponent in FindObjectsOfType<ElectricalComponent>()) {
				if (Vector3.Distance (VisibleComponent.transform.position, transform.position) < XRayDistance)
					OriginalMaterials.Add (new ChangedObject (VisibleComponent, Silhoutte));
			}
			SFXPlayer.Play ();
		} else {
			//Play 'Ability failed' sound effect.
		}
	}

	private void StopPhasing () {
		BaseEnemy IntersectingEnemy = CheckEnemyIntersections ();
		Debug.Log (IntersectingEnemy);
		if (IntersectingEnemy != null) {
			IntersectingEnemy.Kill ();
			//return collided enemy
		}

		PM.EnableAbilities ();
		CurrentlyPhasing = false;
		//PhaseResource = MaxPhaseTime;
		UnphaseQueued = false;
		gameObject.layer = 8;
		foreach (Transform Child in GetComponentsInChildren<Transform>()) {
			if (Child.gameObject.layer == 9)
				Child.gameObject.layer = 8;
		}
		ShadowEffect.enabled = false;
		PhaseCamera.enabled = false;
		foreach (ChangedObject CO in OriginalMaterials)
			CO.Reset ();
		OriginalMaterials = new List<ChangedObject> ();
	}
}