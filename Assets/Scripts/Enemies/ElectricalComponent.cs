using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricalComponent : MonoBehaviour {

	void Start () {
		string Heirarchy = transform.parent.name;
		bool Top = false;
		Transform NextParent = transform.parent;
		while (!Top) {
			if (NextParent.parent != null) {
				NextParent = NextParent.parent;
				Heirarchy += " [INSIDE] " + NextParent.name;
			} else
				Top = true;
		}
		Debug.Log (Heirarchy);
		//Debug.Log (transform.parent.gameObject.name + " ] Inside [ " + transform.parent.parent.gameObject.name + " ] Inside [ " + transform.parent.parent.parent.gameObject.name);
		gameObject.SetActive (false);
	}
}
