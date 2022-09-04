using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallClimbing : PlayerAbility {

	private TriggerChecker FacingWall;
	private TriggerChecker NearTopOfWall;
	private bool Climbing = false;
	private bool ClimbStop = false;
	private Rigidbody RB;
	private Movement PM;

	public float ClimbingSpeed = 3f;

	// Use this for initialization
	void Start () {
		RB = GetComponent<Rigidbody> ();
		PM = GetComponent<Movement> ();

		//Frontal Trigger for jumping.
		//FacingWall = GetComponentInChildren<TriggerChecker> ();
		if (FacingWall == null) {
			var GO = Instantiate (new GameObject (), transform);
			GO.name = "TC: Facing Wall";
			FacingWall = GO.AddComponent<TriggerChecker>();
			var SC = GO.AddComponent<SphereCollider> ();
			SC.radius = 0.3f;
			SC.isTrigger = true;
			SC.center = new Vector3 (0, 0.3f, 0.2f);
		}

		if (NearTopOfWall == null) {
			var GO = Instantiate (new GameObject (), transform);
			GO.name = "TC: Near Top of Wall";
			NearTopOfWall = GO.AddComponent<TriggerChecker>();
			var SC = GO.AddComponent<SphereCollider> ();
			SC.radius = 0.3f;
			SC.isTrigger = true;
			SC.center = new Vector3 (0, 0.3f, -0.5f);
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKey (KeyCode.Space) && FacingWall.Triggered && !NearTopOfWall.Triggered && !Disabled) {
			Climbing = true;
			PM._DisableMovement = true;
			if (Input.GetKey (KeyCode.LeftControl))
				ClimbStop = true;
		}
	}

	void FixedUpdate () {
		if (ClimbStop)
			RB.velocity = new Vector3(0, 0, 0);
		else if (Climbing)
			RB.velocity = new Vector3(RB.velocity.x, ClimbingSpeed, RB.velocity.z);
		Climbing = false;
		ClimbStop = false;
	}
}
