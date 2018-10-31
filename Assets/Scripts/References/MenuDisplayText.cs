using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuDisplayText : MonoBehaviour {

	public string text {
		get {return GetComponent<Text> ().text;}
		set {GetComponent<Text> ().text = value;}
	}
}