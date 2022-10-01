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
        if (DangerLight)
		    DangerLight.enabled = false;
        if (AlertLight)
            AlertLight.enabled = false;
        if (EffectLine)
            EffectLine.enabled = false;
	}
}
