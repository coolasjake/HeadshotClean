using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestDisplayItem {
	public bool Completed = false;
	public string Description;
}

public class Quest : QuestDisplayItem {
	public List<Objective> Objectives = new List<Objective> ();
	public bool HideFutureObjectives = false;
	//public bool Completed = false;
	public int CurrentObjective = 0;
	public bool Active = false;

	public Quest (string Name) {
		Description = Name;
	}

	public void StartQuest () {
		if (Objectives.Count == 0) {
			Debug.Log ("Zero Objectives in Quest: " + Description);
			return;
		}
		Objectives [0].StartObjective ();
		Active = true;
		Debug.Log (Description + " Objectives: " + Objectives.Count);
	}

	public bool CheckCompleted () {
		Debug.Log (Description + " Number of Objectives?!?!? " + Objectives.Count + " Current Objective: " + CurrentObjective);
		if (Objectives [CurrentObjective].CheckCompleted ()) {
			if (CurrentObjective + 1 >= Objectives.Count) {
				Completed = true;
				Debug.Log (Description + " Current Objective = " + CurrentObjective + " / " + Objectives.Count);
				return true;
			} else if (Objectives.Count > CurrentObjective + 1) {
				CurrentObjective += 1;
				Objectives [CurrentObjective].StartObjective ();
			}
		}
		return false;
	}

	public void Print () {
		if (Active)
			Debug.Log ("Quest: " + Description + ", " + Objectives.Count + " Objectives" + " ACTIVE");
		else
			Debug.Log ("Quest: " + Description + ", " + Objectives.Count + " Objectives");
		foreach (Objective O in Objectives) {
			if (O.Active || !HideFutureObjectives)
				O.Print ();
		}
	}
}

/// <summary>
/// A Quest can only have one Objective active at a time. When it is completed, the next one is activated, or the Quest is also completed.
/// </summary>
public class Objective : QuestDisplayItem {
	public List<SubObjective> SubObjectives = new List<SubObjective>();
	//public bool Completed = false;
	public bool Active = false;
	/// <summary>
	/// The number of sub objectives required to complete this objective, e.g. 'Ambush ONE of the THREE enemies' would have 3 SOs, but this variable would be 1.
	/// </summary>
	public int SubObjectivesRequiredToComplete = 1;
	public int CurrentNumberCompleted = 0;

	public Objective (string Name) {
		Description = Name;
	}

	public void StartObjective () {
		Active = true;
		foreach (SubObjective SO in SubObjectives) {
			SO.StartSubObjective ();
		}
	}

	public bool CheckCompleted () {
		foreach (SubObjective SO in SubObjectives) {
			if (SO.CheckCompleted ())
				CurrentNumberCompleted += 1;
		}

		if (CurrentNumberCompleted >= SubObjectivesRequiredToComplete) {
			Completed = true;
			return true;
		}
		return false;
	}

	public void Print() {
		if (Completed)
			Debug.Log ("    - " + Description + " COMPLETE");
		else
			Debug.Log ("    - " + Description);
	}
}

/// <summary>
/// An Objective can have multiple SubObjectives active simultaneously, e.g. 'Find the 3 special enemies'.
/// </summary>
public class SubObjective : QuestDisplayItem {
	public QuestPointer Pointer;

	bool AttatchedToArea = false;

	private GameObject TargetObject;
	private Vector3 AreaPosition;
	private Vector3 AreaSize;
	private Vector3 AreaRotation;

	/// <summary>
	/// Create a sub objective and attach it to a destructible object.
	/// </summary>
	/// <param name="Target">The object the pointer is attached to.</param>
	public SubObjective (string Name, GameObject Target) {
		Description = Name;
		AttatchedToArea = false;
		TargetObject = Target;
	}

	/// <summary>
	/// Create a sub objective for an area.
	/// </summary>
	/// <param name="Position">The position for the bounding box on this objective.</param>
	/// <param name="BoxSize">The size of the bounding box.</param>
	public SubObjective (string Name, Vector3 Position, Vector3 BoxSize, Vector3 Rotation) {
		Description = Name;
		AttatchedToArea = true;

		AreaPosition = Position;
		AreaSize = BoxSize;
		AreaRotation = Rotation;
	}

	public void StartSubObjective () {
		if (AttatchedToArea) {
			Quaternion Rot = new Quaternion ();
			Rot.eulerAngles = AreaRotation;
			GameObject QuestAreaTriggerObject = GameObject.Instantiate (new GameObject (), AreaPosition, Rot);
			QuestAreaTriggerObject.layer = 17; //Player Only
			QuestAreaTriggerObject.name = Description;
			QuestAreaTriggerObject.AddComponent<QuestAreaTrigger> ();
			BoxCollider BC = QuestAreaTriggerObject.AddComponent<BoxCollider> ();
			BC.size = AreaSize;
			BC.isTrigger = true;
			//Description = Description;

			GameObject QuestIndicatorObject = GameObject.Instantiate (Resources.Load<GameObject> ("Prefabs/QuestPointer"), GameObject.FindObjectOfType<Canvas>().transform);
			QuestIndicatorObject.GetComponent<QuestPointer> ().Target = QuestAreaTriggerObject.transform;
			QuestIndicatorObject.GetComponent<QuestPointer> ().Message = Description;
		} else {
			if (TargetObject != null) {
				GameObject QuestIndicatorObject = GameObject.Instantiate (Resources.Load<GameObject> ("Prefabs/QuestPointer"), GameObject.FindObjectOfType<Canvas> ().transform);
				QuestIndicatorObject.GetComponent<QuestPointer> ().Target = TargetObject.transform;
				QuestIndicatorObject.GetComponent<QuestPointer> ().Message = Description;
			} else {
				Pointer = null;
				GameObject.FindObjectOfType<QuestManager> ().CheckQuests ();
			}
		}
	}

	public bool CheckCompleted () {
		if (Pointer == null && Completed == false) {
			Completed = true;
			return true;
		}
		return false;
	}
}