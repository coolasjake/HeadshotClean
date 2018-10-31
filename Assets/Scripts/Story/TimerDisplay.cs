using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerDisplay : MonoBehaviour {

	public bool TakeText = false;
	public float TimerValue = 0;
	public string ConvertedTimerValue = "00:00:00";
	private TextMesh Display;

	// Use this for initialization
	void Start () {
		Display = GetComponent<TextMesh> ();
	}
	
	// Update is called once per frame
	void Update () {
		int Mins = Mathf.FloorToInt (TimerValue/60);
		int Seconds = (int)TimerValue % 60;
		int Milli = (int)((TimerValue - (Mins * 60) - Seconds) * 100);

		string MinuteConversion = Mins + ":" + Seconds + ":" + Milli;


		Display.text = MinuteConversion;
	}
}
