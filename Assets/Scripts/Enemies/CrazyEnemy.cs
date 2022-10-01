using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrazyEnemy : ShootingEnemy {

	// Use this for initialization
	void Start () {
		//Laser will be stopped by:
		//Default, Player, Ground, Window.
		raycastShootingMask = 1 << 0 | 1 << 8 | 1 << 10 | 1 << 11;
		//Sight will be stopped by:
		//Default, Player, Ground, OpaqueGrating.
		raycastLookingMask = 1 << 0 | 1 << 8 | 1 << 10 | 1 << 17;

		State = AIState.Searching;
        if (Head == null)
            Head = GetComponentInChildren<Head> ().transform;
		FP = GetComponentInChildren<FiringPoint> ();
		FiringDelayTimer = Time.time + Random.Range (2f, 20f);

		//GameObject DI = Instantiate (Resources.Load<GameObject> ("Prefabs/DetectionIndicator"), FindObjectOfType<Canvas> ().transform);
		//DI.GetComponent<DetectionIndicator> ().Target = transform;
	}
	
	// Update is called once per frame
	void Update () {
		StateMachine ();
	}

	protected override void StateMachine () {
		DetectPlayer ();
		RotateHead ();

		if (State == AIState.Staring)
			Staring ();

		if (State == AIState.Searching)
			Searching ();

		if (State == AIState.Firing)
			Firing ();
		else if (Time.time > FiringDelayTimer) {
			State = AIState.Firing;
			StartedFiring = Time.time;
			FP.DangerLight.intensity = 1.5f;
			FP.EffectLine.enabled = true;
			//FP.ParticleEffect.Play ();
			StartHeadRotation = Head.transform.rotation;
			StartPlayerDirection = Random.rotation;
			GetComponent<AudioSource> ().Play ();

			FiringDelayTimer = Time.time + Random.Range (2f, 20f);
		}
	}

	protected override void Staring () {
		if (PlayerVisibility == 0) {
			State = AIState.Searching;
			StartLookingAround ();
		}
	}

	protected override void Searching () {
		if (PlayerVisibility > 0) {
			State = AIState.Staring;
			LookAround = false;
		}
	}

	protected override void Firing () {

		if (Time.time < StartedFiring + FireDuration) {
			//Rotate by: Time since this started firing, divided by half of the fire duration.
			float RotationFactor = (Time.time - StartedFiring) / (FireDuration * 0.5f);
			Head.transform.rotation = Quaternion.LerpUnclamped (StartHeadRotation, StartPlayerDirection, RotationFactor);

			RaycastHit Hit;
			if (Physics.Raycast (FP.transform.position - FP.transform.up, -FP.transform.up, out Hit, 600, raycastShootingMask)) {
				//Line.transform.localPosition = new Vector3 (0, -2, 0);
				Vector3 LinePoint = FP.EffectLine.transform.InverseTransformPoint (Hit.point);
				FP.EffectLine.SetPosition (1, LinePoint);//new Vector3(0, -Vector3.Distance(FP.transform.position, Hit.point), 0));
				//FP.ParticleEffect.transform.localScale = new Vector3(0, Vector3.Distance(FP.transform.position, Hit.point), 0);

				Shootable SH = Hit.collider.GetComponentInParent<Shootable> ();
				if (SH)
					SH.Hit(DPS * Time.deltaTime);
				else if (Time.timeScale > 0)
					SpawnBurnDecal (Hit);
			}
		} else {
			//Stop Firing
			State = AIState.Staring;
			FP.DangerLight.enabled = false;
			FP.EffectLine.enabled = false;
		}
	}
}
