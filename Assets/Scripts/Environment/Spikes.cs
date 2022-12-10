using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour {

	private float LastHit = -5;

	void OnTriggerEnter (Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerHealth P = col.gameObject.GetComponent<PlayerHealth>();
            if (P)
                P.Kill("Spikes");
        }
    }
}
