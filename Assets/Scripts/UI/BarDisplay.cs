using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarDisplay : MonoBehaviour {

	public Image PositiveBar;
	public Image NegativeBar;

	// Use this for initialization
	void Start () {
		PositiveBar = GetComponentsInChildren<Image> ()[1];
		PositiveBar.fillAmount = 0;
		NegativeBar = GetComponentsInChildren<Image> ()[2];
		NegativeBar.fillAmount = 0;
	}

	public void ChangeValue (float NormalisedValue) {
		PositiveBar.fillAmount = NormalisedValue;
		NegativeBar.fillAmount = NormalisedValue * -1;
	}
}
