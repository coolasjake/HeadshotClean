using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiringPoint : MonoBehaviour {

	public Light DangerLight;
	public Light AlertLight;
	public LineRenderer EffectLine;
	//public ParticleSystem ParticleEffect;

	// Use this for initialization
	void Start () {
		DangerLight = GetComponentInChildren<Light> ();
		DangerLight.enabled = false;
		if (GetComponentsInChildren<Light> ().Length > 1) {
			AlertLight = GetComponentsInChildren<Light> () [1];
			AlertLight.enabled = false;
		}
		EffectLine = GetComponentInChildren<LineRenderer> ();
		EffectLine.enabled = false;
		//ParticleEffect = GetComponentInChildren<ParticleSystem> ();
	}
}
