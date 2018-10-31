using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PatrolEnemy))]
public class PatrolEnemyEditor : Editor {

	private bool TryingToReset = false;

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();

		PatrolEnemy PE = (PatrolEnemy)target;
		if (GUILayout.Button ("Add Current Location to Path: NO Pause")) {
			PE.PatrolPoints.Add (PE.transform.position);
			PE.PausePoints.Add (false);
			EditorUtility.SetDirty (PE);
		} else if (GUILayout.Button ("Add Current Location to Path: WITH Pause")) {
			PE.PatrolPoints.Add (PE.transform.position);
			PE.PausePoints.Add (true);
			EditorUtility.SetDirty (PE);
		} 

		if (TryingToReset) {
			if (GUILayout.Button ("Confirm Reset")) {
				PE.PatrolPoints = new List<Vector3> ();
				PE.PausePoints = new List<bool> ();
				EditorUtility.SetDirty (PE);
				TryingToReset = false;
			} else if (GUILayout.Button ("Cancel"))
				TryingToReset = false;
		} else {
			if (GUILayout.Button ("Reset Patrol Path"))
				TryingToReset = true;
		}
	}
}
