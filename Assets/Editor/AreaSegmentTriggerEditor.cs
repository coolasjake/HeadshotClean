using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AreaSegmentTrigger))]
public class AreaSegmentTriggerEditor : Editor {

	private bool TryingToReset = false;

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();

		AreaSegmentTrigger AST = (AreaSegmentTrigger)target;
		if (GUILayout.Button ("Swap")) {
			List<GameObject> TempUnload = AST.ObjectsToUnload;
			AST.ObjectsToUnload = AST.ObjectsToLoad;
			AST.ObjectsToLoad = TempUnload;
			EditorUtility.SetDirty (AST);
		}
	}
}
