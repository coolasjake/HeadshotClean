using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSlowing : MonoBehaviour {

	public float SlowedTimeFactor = 0.5f;
    private float DefaultFixedTimeInterval;

	private float SlowStartTime;
	private bool CanSlow = false;
	private bool CurrentlySlowing = false;

    void Start()
    {
        DefaultFixedTimeInterval = Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void Update () {
		if (Input.GetKeyDown (KeyCode.R)) {
            if (CurrentlySlowing)
            {
                Time.fixedDeltaTime = DefaultFixedTimeInterval;
                Time.timeScale = 1;
                CurrentlySlowing = false;
            }
            else
            {
                Time.fixedDeltaTime = DefaultFixedTimeInterval * SlowedTimeFactor;
                Time.timeScale = SlowedTimeFactor;
                CurrentlySlowing = true;
            }
		}
	}
}
