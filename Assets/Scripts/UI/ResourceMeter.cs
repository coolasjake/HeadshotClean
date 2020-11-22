using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceMeter : MonoBehaviour {

	public Image Fill;
	public float MaxFill = 0.25f;

	// Use this for initialization
	void Start () {
		GetComponent<Image> ().fillAmount = MaxFill;
		Fill = GetComponentsInChildren<Image> ()[1];
		Fill.fillAmount = MaxFill;
	}

	public void ChangeValue (float NormalisedValue) {
		Fill.fillAmount = NormalisedValue * MaxFill;
	}
}
