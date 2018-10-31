using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour {

	public List<Quest> ActiveQuests = new List<Quest> ();
	public List<Quest> CompletedQuests = new List<Quest> ();
	private int CurrentQuest = 0;

	// Use this for initialization
	void Start () {

		//Create a bunch of test quests
		Quest NewQuest;
		Objective NewObjective;

		//FIRST QUEST
		NewQuest = new Quest("Sightseeing");

		NewObjective = new Objective ("Visit the Prison");
		NewObjective.SubObjectives.Add(new SubObjective("Prison", new Vector3 (0, 14, -2), new Vector3 (5, 5, 5), new Vector3 ()));
		NewQuest.Objectives.Add (NewObjective);

		NewObjective = new Objective ("Visit the Factory");
		NewObjective.SubObjectives.Add(new SubObjective("Factory", new Vector3 (-33, 12, -58), new Vector3 (5, 5, 5), new Vector3 ()));
		NewQuest.Objectives.Add (NewObjective);

		NewObjective = new Objective ("Visit the Roof");
		NewObjective.SubObjectives.Add(new SubObjective("Roof", new Vector3 (0, 91, 0), new Vector3 (5, 5, 5), new Vector3 ()));
		NewQuest.Objectives.Add (NewObjective);

		ActiveQuests.Add(NewQuest);

		//SECOND QUEST
		CreateExterminateQuest (300);

		ActiveQuests [0].StartQuest ();

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.O)) {
			Debug.Log ("-----PRINTING QUESTS-----");
			if (ActiveQuests.Count > 0) {
				foreach (Quest Q in ActiveQuests)
					Q.Print ();
			}
			if (CompletedQuests.Count > 0) {
				Debug.Log ("COMPLETED");
				foreach (Quest Q in CompletedQuests)
					Q.Print ();
			}
		}
			
	}

	public void CheckQuests () {
		Debug.Log ("Checking Quests");
		if (ActiveQuests.Count > 0) {
			int Repetitions = 0;
			foreach (Quest Q in ActiveQuests) {
				Repetitions += 1;
				if (Repetitions > 100) {
					Debug.Log ("Broke Infinite Loop");
					break;
				}
				if (Q.Active && !Q.Completed && Q.CheckCompleted ()) {
					//CompletedQuests.Add (Q);
					if (ActiveQuests.Count > CurrentQuest + 1) {
						CurrentQuest += 1;
						ActiveQuests [CurrentQuest].StartQuest ();
						ActiveQuests [CurrentQuest].Active = true;
						break;
					}
				}
			}
		}

		//foreach (Quest Q in CompletedQuests) {
		//	if (ActiveQuests.Contains (Q))
		//		ActiveQuests.Remove (Q);
		//}
	}

	public void CreateExterminateQuest (float Range) {

		Quest NewQuest;
		Objective NewObjective = new Objective("THIS SHOULDNT EXIST");

		NewQuest = new Quest("Exterminate");

		int NumInObjective = 0;
		Vector3 PlayerPosition = Movement.ThePlayer.transform.position;
		foreach (BaseEnemy Enemy in FindObjectsOfType<BaseEnemy>()) {
			if (Vector3.Distance(Enemy.transform.position, PlayerPosition) < Range) {
				if (NumInObjective == 0)
					NewObjective = new Objective ("Kill All Enemies");

				if (NumInObjective < 3)
					NewObjective.SubObjectives.Add (new SubObjective ("Kill", Enemy.gameObject));

				NumInObjective += 1;

				if (NumInObjective == 3) {
					NewObjective.SubObjectivesRequiredToComplete = NumInObjective;
					NewQuest.Objectives.Add (NewObjective);
					NumInObjective = 0;
				}
			}
		}
		if (NumInObjective != 0) {
			NewObjective.SubObjectivesRequiredToComplete = NumInObjective;
			NewQuest.Objectives.Add (NewObjective);
		}

		ActiveQuests.Add(NewQuest);
	}

	/// <summary>
	/// Creates a quest to destroy the target.
	/// </summary>
	/// <param name="Target">The object to destroy</param>
	/// <param name="TargetName">Name of the target, to be used in 'Destroy X'</param>
	public void CreateDestroyMeQuest (GameObject Target, string TargetName) {
		Quest NewQuest;
		Objective NewObjective;

		NewQuest = new Quest("Destroy the prototype robot.");

		NewObjective = new Objective ("Destroy " + TargetName);
		NewObjective.SubObjectives.Add (new SubObjective ("Destroy", Target));
		NewQuest.Objectives.Add (NewObjective);

		ActiveQuests.Add (NewQuest);
	}
}
