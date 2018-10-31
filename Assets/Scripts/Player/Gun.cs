using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : PlayerAbility {

	//public LineRenderer Line;
	public Transform GunModel;

	public float DecalMinDistance = 0.3f;
	public float RecoilAngle = -20f;
	public float RecoilTime = 0.4f;
	private float LastShot = 0f;

	public bool WannaShoot = false;
	public bool Laser = false;
	private int Frames = 0;

	private AudioManager SFXPlayer;

	private Vector3 LastDecalPoint = new Vector3 ();

	//Default, Player, Ground, Window.
	private int RaycastShootingMask = 1 << 0 | 1 << 10 | 1 << 11 | 1 << 12;

	void Start () {
		SFXPlayer = GetComponent<AudioManager> ();
		if (Disabled)
			HideGun ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0) && !Disabled)
			WannaShoot = true;
		if (Input.GetKeyDown (KeyCode.Tab))
			Laser = !Laser;
		if (Time.time - LastShot > RecoilTime && Input.GetMouseButton (0) && !Disabled && !WannaShoot)
			WannaShoot = true;
		Frames += 1;
		//Debug.Log ("Average FPS: " + Frames / Time.time);
	}

	void FixedUpdate () {

		//Normailised, 	0 = Gun should be highest, 1 = Gun should be reset
		//((Time.time - LastShot)/RecoilTime)
		//				0 = -20, 1 = -0
		//-20 + (((Time.time - LastShot)/RecoilTime) * 20)

		if (Time.time - LastShot > RecoilTime) {
			Quaternion GunRotation = new Quaternion ();
			GunRotation.eulerAngles = new Vector3 (0, 0, 0);
			GunModel.localRotation = GunRotation;
		} else {
			Quaternion GunRotation = new Quaternion ();
			GunRotation.eulerAngles = new Vector3 (RecoilAngle + (((Time.time - LastShot)/RecoilTime) * -RecoilAngle), 0, 0);
			GunModel.localRotation = GunRotation;
		}

		if (Laser && !Disabled) {
			//Tracking mode, call hit on target every update.
			RaycastHit Hit;

			if (Physics.Raycast (transform.position, transform.forward, out Hit, 600, RaycastShootingMask)) {
				//Debug.DrawRay (transform.position, transform.forward, Color.red, 2f);
				Shooteable SH = Hit.transform.GetComponent<Shooteable> ();
				if (SH) {
					SH.Hit (35f);
				} else {
					if (Vector3.Distance (Hit.point, LastDecalPoint) > DecalMinDistance)
						SpawnBurnDecal (Hit);
				}
			}
		} else if (WannaShoot && Time.time - LastShot > RecoilTime) {
			LastShot = Time.time;


			GameObject ShotAlarm = new GameObject ();
			ShotAlarm.layer = 15;
			ShotAlarm.transform.position = transform.position;
			ShotAlarm.name = "Gunshot Alarm";
			Alarm Al1 = ShotAlarm.AddComponent<Alarm> ();
			Al1.Radius = 30;

			RaycastHit Hit;

			if (Physics.Raycast (transform.position, transform.forward, out Hit, 600, RaycastShootingMask)) {
				//Debug.DrawRay (transform.position, transform.forward, Color.red, 2f);

				GameObject LineObject;
				Vector3 LinePoint;

				/*
				LineObject = Instantiate (Resources.Load<GameObject> ("Prefabs/Line"), GunModel);
				LineObject.transform.localPosition = new Vector3 (0.0023f, 0.3197f, 0.606f);
				LineObject.transform.SetParent (null);
				LineObject.transform.position = Vector3.MoveTowards (Hit.point, LineObject.transform.position, 5f);
				LinePoint = LineObject.transform.InverseTransformPoint (Hit.point);
				LineObject.GetComponent<LineRenderer>().SetPosition (1, LinePoint);
				LineObject.GetComponent<BulletTrail> ().DestroyTime = RecoilTime * 2f;
				*/

				LineObject = Instantiate (Resources.Load<GameObject> ("Prefabs/Line"), GunModel);
				LineObject.name = "Line";
				LineObject.transform.localPosition = new Vector3 (0.0023f, 0.3197f, 0.606f);
				LineObject.transform.SetParent (null);
				LineObject.transform.position = Vector3.MoveTowards (LineObject.transform.position, Hit.point, 1f);
				LinePoint = LineObject.transform.InverseTransformPoint (Hit.point);
				LineObject.GetComponent<LineRenderer>().SetPosition (1, LinePoint);
				LineObject.GetComponent<BulletTrail> ().DestroyTime = RecoilTime;

				Shooteable SH = Hit.collider.GetComponentInParent<Shooteable> ();
				if (SH) {
					SH.Hit (35f);
				} else {
					SpawnBurnDecal (Hit);
					GameObject HitAlarm = new GameObject ();
					HitAlarm.layer = 15;
					HitAlarm.transform.position = transform.position;
					HitAlarm.name = "Gunshot Alarm";
					Alarm Al2 = HitAlarm.AddComponent<Alarm> ();
					Al2.Radius = 30;
				}

				//Play sound
				SFXPlayer.PlaySound ("Gunshot");
				//GunModel.GetComponent<AudioSource>().Play();
			}
		}
		WannaShoot = false;
	}

	public void HideGun () {
		GunModel.gameObject.SetActive (false);
	}

	public void RevealGun () {
		GunModel.gameObject.SetActive (true);
	}

	private void SpawnBurnDecal (RaycastHit Hit) {
		Quaternion DecalRotation = Quaternion.LookRotation (Hit.normal);
		LastDecalPoint = Hit.point + (Hit.normal * 0.001f);
		GameObject LaserBurn = Instantiate (Resources.Load<GameObject> ("Prefabs/LaserBurn"), LastDecalPoint, DecalRotation);
		LaserBurn.GetComponent<Decal> ().Duration = 3;
	}
}
