using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityTrialController : MonoBehaviour {

	public TrialStartButton StartButton;
	public FailFloor FF;

	public List<TimerDisplay> Timers;

	private int TotalButtons = 0;
	private int LitButtons = 0;
	private float StartTime = 0;
	private float FinishTime = 0;
	private bool UpdateTimer = false;
	private ChallengeState State = ChallengeState.None;

	void Start () {
		TotalButtons = GetComponentsInChildren<FlyingCube> ().Length;
		foreach (FlyingCube FC in GetComponentsInChildren<FlyingCube> ())
			FC.ChangeToBlack ();
		FF.NormalMode ();
	}

	void Update () {
		if (UpdateTimer) {
			foreach (TimerDisplay TD in Timers) {
				TD.TimerValue = Time.time - StartTime;
			}
		}
	}

	public void ButtonActivated () {
		LitButtons += 1;
		Debug.Log ("Lit Buttons: " + LitButtons);
		if (LitButtons >= TotalButtons)
			FinishChallenge ();
	}

	private void FinishChallenge () {
		State = ChallengeState.Succeed;
		FF.VictoryMode ();
		FinishTime = Time.time;
		StartButton.PopUp ();
		HSRemoteTrigger RT = GetComponent<HSRemoteTrigger> ();
		if (RT)
			RT.TriggerEvents ();
		UpdateTimer = false;
	}

	public void FailChallenge () {
		if (State == ChallengeState.Active) {
			State = ChallengeState.Fail;
			FF.NormalMode ();
			LitButtons = 0;
			foreach (FlyingCube FC in GetComponentsInChildren<FlyingCube> ())
				FC.ChangeToBlack ();
			StartButton.PopUp ();
		}
		UpdateTimer = false;
	}

	public void StartChallenge () {
		State = ChallengeState.Active;
		FF.DangerMode ();
		LitButtons = 0;
		StartTime = Time.time;
		foreach (FlyingCube FC in GetComponentsInChildren<FlyingCube> ())
			FC.ChangeToRed ();
		UpdateTimer = true;
	}
}

public enum ChallengeState {
	None,
	Active,
	Fail,
	Succeed
}