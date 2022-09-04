using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class Teleport : PlayerAbility {

	private Movement PM;
	private float Distance;
	private bool ChoosingPosition = false;
	private ResourceMeter Meter;
	private GameObject PointRep;

	public GameObject PointRepPrefab;
	public float MinimumDistance;
	public float DistanceIncreasePerSecond;

	//Default, Ground, Window.
	private int RaycastMask = 1 << 0 | 1 << 10 | 1 << 11;


	// Use this for initialization
	void Start () {
		PM = GetComponent<Movement> ();
		//Meter = FindObjectOfType<Canvas> ().GetComponentsInChildren<ResourceMeter> () [3];
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (1) && Resource > MinToUse && !Disabled) {
            //When the button is pressed, Create the object representing the teleport position, and initialise values
            ConsumeResource(1f);
			ChoosingPosition = true;
			Distance = MinimumDistance;
			PointRep = Instantiate (PointRepPrefab, PM.MainCamera.transform.position + (PM.MainCamera.transform.forward * Distance), new Quaternion ());
		} else if (Input.GetMouseButtonUp (1) && ChoosingPosition) {
			//When the button is released, do a final raycast and teleport the player to the position chosen.
			RaycastHit Hit;
			if (Physics.Raycast (PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out Hit, Distance, RaycastMask))
				PM.transform.position = PM.MainCamera.transform.position + (PM.MainCamera.transform.forward * (Hit.distance - PM.playerSphereSize));
			else
				PM.transform.position = PM.MainCamera.transform.position + (PM.MainCamera.transform.forward * Distance);
            ConsumeResource(Distance / 300);
			Destroy (PointRep);
			ChoosingPosition = false;
		} else if (Input.GetMouseButton (1) && ChoosingPosition && !Disabled) {
			//While the button is held down, 
			Distance += (DistanceIncreasePerSecond * (Distance / 10)) * Time.deltaTime;

			RaycastHit Hit;
			if (Physics.Raycast (PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out Hit, Distance, RaycastMask)) {
				PointRep.transform.position = PM.MainCamera.transform.position + (PM.MainCamera.transform.forward * (Hit.distance - PM.playerSphereSize));
				Distance = Vector3.Distance (transform.position, Hit.point);
			} else
				PointRep.transform.position = PM.MainCamera.transform.position + (PM.MainCamera.transform.forward * Distance);
		}

        if (!ChoosingPosition && Resource < MaxResource)
            RegenerateResource();

		//Meter.ChangeValue (Resource / MaxResource);
	}
}
