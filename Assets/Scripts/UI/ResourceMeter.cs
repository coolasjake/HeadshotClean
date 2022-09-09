using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceMeter : MonoBehaviour {

	public Image FillImage;
	public float MaxFill = 0.25f;

	// Use this for initialization
	void Start () {
		GetComponent<Image> ().fillAmount = MaxFill;
		FillImage = GetComponentsInChildren<Image> ()[1];
		FillImage.fillAmount = MaxFill;
	}

	public void ChangeValue (float NormalisedValue) {
		FillImage.fillAmount = NormalisedValue * MaxFill;
	}
}
