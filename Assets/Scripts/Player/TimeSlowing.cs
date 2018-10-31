using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSlowing : MonoBehaviour {

	public float SlowedTimeFactor = 0.2f;
	public float SlowTimeDuration = 2f;
	public float Cooldown = 10f;

	private float SlowStartTime;
	private bool CanSlow = false;
	private bool CurrentlySlowing = false;

	// Update is called once per frame
	void Update () {
		if (Time.time > SlowStartTime + Cooldown)
			CanSlow = true;

		if (CanSlow && Input.GetKeyDown (KeyCode.R)) {
			SlowStartTime = Time.time;
			Time.timeScale = SlowedTimeFactor;
			CurrentlySlowing = true;
			CanSlow = false;
		}

		if (CurrentlySlowing && Time.time > SlowStartTime + SlowTimeDuration) {
			Time.timeScale = 1;
			CurrentlySlowing = false;
		}
	}
}
