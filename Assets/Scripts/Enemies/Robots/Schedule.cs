using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Schedule : MonoBehaviour {

	//TERMS:
	//Employee - enemy which is associated with this schedule.
	//Rotation - when ALL the employees are given the next destination in the patrol loop, based on the one they had previously. 

	/// <summary> The points of the 'group patrol'. Must be in order, but can start anywhere on the loop. </summary>
	public List<Vector3> PatrolPoints = new List<Vector3>();
	/// <summary> The current rotation, used to find the next point for each enemy. </summary>
	private int RotationNumber = 0;
	/// <summary> The enemies associated with this schedule. Enemies 'clock in' on start, so should be empty before then. </summary>
	private List<ScheduleEnemy> Employees = new List<ScheduleEnemy>();
	/// <summary> Maximum amount of time after the first enemy has reached their point that they should wait (before all destinations are rotated). </summary>
	public float MaxPauseTime = 5;
	/// <summary> Maximum amount of time between rotations even if no enemies reach their points (should stop enemies getting out of synch because of bugs or player distraction). </summary>
	public float MaxShiftLength = 60;
	/// <summary> The time since the last enemy reached their point (or a rotation was forced), so that shift and pause length can be determined. </summary>
	private float LastShiftCompletionTime = 0;
	/// <summary> Decides whether pause time or shift length should be used to force rotations (if true use MaxPauseTime). </summary>
	private bool EmployeeIsPausing = false;
	/// <summary> Number of enemies pausing - if this number is equal to the number of 'employees', a rotation occurs (since all enemies have reached their destinations). </summary>
	private int EmployeesPausing = 0;

	public bool RunTimelinessCheck = true;

	void Start () {
		//Call 'check for max pause/shift' coroutine
		StartCoroutine ("CheckOnSchedule");
	}

	/// <summary> Called when an enemy reaches one of the points (by the enemy). Is the main trigger for rotations. </summary>
	public void ReachedPoint () {
		EmployeesPausing += 1;
		if (EmployeesPausing >= Employees.Count)
			Rotation ();
		else {
			EmployeeIsPausing = true;
			LastShiftCompletionTime = Time.time;
		}
	}

	/// <summary> Gives each 'employee' the next point in the patrol loop, based on where they are in the list. </summary>
	private void Rotation () {
		int EmployeeNumber = 0;
		foreach (ScheduleEnemy Employee in Employees) {
			int NextPoint = Mathf.FloorToInt ((EmployeeNumber * PatrolPoints.Count) / Employees.Count) + 1;
			NextPoint = (NextPoint + RotationNumber) % PatrolPoints.Count;

			Employee.GiveNextPoint (PatrolPoints [NextPoint]);
			EmployeeNumber += 1;

			EmployeesPausing = 0;
			EmployeeIsPausing = false;

			LastShiftCompletionTime = Time.time;
		}
		RotationNumber += 1;
	}

	public Vector3 GetFirstPoint (ScheduleEnemy Enemy) {
		Employees.Add (Enemy);
		LastShiftCompletionTime = Time.time;
		return PatrolPoints [(Employees.Count - 1) % PatrolPoints.Count];
	}

	private IEnumerator CheckOnSchedule () {
		while (RunTimelinessCheck) {
			if (EmployeeIsPausing) {
				if (Time.time > LastShiftCompletionTime + MaxPauseTime) {
					Rotation ();
				}
			} else {
				if (Time.time > LastShiftCompletionTime + MaxShiftLength) {
					Rotation ();
				}
			}
			yield return new WaitForSeconds (1f);
		}
	}

}
