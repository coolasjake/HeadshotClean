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
	private bool Intersecting;

	public float FailsafeCapsuleRadius = 0.5f;
	public InputMode Mode = InputMode.Tap;
	public bool AutoUnphase = true;
	public float TapDelay = 0.2f;
	public float MaxPhaseTime = 3f;
	public float XRayDistance = 20f;
	[Range (0, 1)]
	public float PhasedObjectOpacity = 0.5f;
	public Material Silhoutte;
	public Material TestPhaseChangeMat;
	public List<ChangedObject> OriginalMaterials = new List<ChangedObject>();
	public List<Material> PhaseTransparentMats = new List<Material> ();

	// Use this for initialization
	void Start () {
		PM = GetComponent<Movement> ();

		PhaseMask = 1 << 0 | 1 << 11 | 1 << 16 | 1 << 17;

		//C = FindObjectOfType<Canvas> ();
        GameObject phaseUI = UIManager.stat.LoadOrGetUI("Phase");
		ShadowEffect = phaseUI.GetComponentInChildren<Image> ();
		IntersectWarning = phaseUI.GetComponentsInChildren<Image> () [1];
		RobotIntersectWarning = phaseUI.GetComponentsInChildren<Image> () [2];
		Meter = phaseUI.GetComponentInChildren<ResourceMeter> ();
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
            ConsumeResource(Time.deltaTime);
			if (CameraCheck.Triggered) {
				Color C = ShadowEffect.color;
				C.a = 1;
				ShadowEffect.color = C;
			} else {
				Color C = ShadowEffect.color;
				C.a = 0.5f;
				ShadowEffect.color = C;
			}
		} else {
            if (Resource < MaxResource)
                RegenerateResource();
		}


		if (Input.GetKey (KeyCode.T)) {
			Color C = TestPhaseChangeMat.color;
			C.a = 0.5f;
			TestPhaseChangeMat.color = C;
		}


		Intersecting = false;
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
						if (Intersecting)
							UnphaseQueued = true;
						else
							StopPhasing ();
					}
				}
			}
        }

        if (CurrentlyPhasing)
            ConsumeResource(Time.deltaTime);
        else
            RegenerateResource();

        if (Resource <= 0) {
			if (!CheckBodyIntersections ())
				StopPhasing ();
			else
				PM.Hit (Time.deltaTime);
		}

		Meter.ChangeValue (Resource/MaxResource);
	}

	private bool CheckBodyIntersections () {
		float HalfHeight = PM.playerSphereSize / 2;
		return Physics.CheckCapsule (transform.position + transform.up * HalfHeight, transform.position - transform.up * HalfHeight, PM.playerWaistSize, PhaseMask);
	}

	private bool CheckHeadIntersections () {
		return Physics.CheckSphere (transform.position + transform.up * 0.3f + transform.forward * -0.4f, 0.2f, PhaseMask);
	}

	private BaseEnemy CheckEnemyIntersections () {
		int EnemiesOnly = 1 << 12;
		float HalfHeight = PM.playerSphereSize / 2;
		Collider[] Hits = Physics.OverlapCapsule (transform.position + transform.up * HalfHeight, transform.position - transform.up * HalfHeight, PM.playerWaistSize, EnemiesOnly);
		if (Hits.Length > 0)
			return Hits[0].GetComponent<BaseEnemy>();
		else
			return null;
	}

	private void StartPhasing () {
		if (Resource > MinToUse) {
			if (Mode == InputMode.Tap)
				TapDelayTime = TapDelay;
			//PM.DisableAbilities (this);
			gameObject.layer = 9;
			foreach (Transform Child in GetComponentsInChildren<Transform>()) {
				if (Child.gameObject.layer == 8)
					Child.gameObject.layer = 9;
			}
			if (CurrentlyPhasing)
				return;
			CurrentlyPhasing = true;
			ShadowEffect.enabled = true;
			Color C = ShadowEffect.color;
			C.a = 0.5f;
			ShadowEffect.color = C;
			PhaseCamera.enabled = true;
			SFXPlayer.Play ();
		} else {
			//Play 'Ability failed' sound effect.
		}
	}

	private void StopPhasing () {
		if (Intersecting)
			return;
		BaseEnemy IntersectingEnemy = CheckEnemyIntersections ();
		//Debug.Log (IntersectingEnemy);
		if (IntersectingEnemy != null) {
			IntersectingEnemy.Kill ();
			//return collided enemy
		}

		//PM.EnableAbilities ();
		CurrentlyPhasing = false;
		UnphaseQueued = false;
		gameObject.layer = 8;
		foreach (Transform Child in GetComponentsInChildren<Transform>()) {
			if (Child.gameObject.layer == 9)
				Child.gameObject.layer = 8;
		}
		ShadowEffect.enabled = false;
		PhaseCamera.enabled = false;
		OriginalMaterials = new List<ChangedObject> ();
	}
}