using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportWall : MonoBehaviour {

	public TPWallExit Exit;

	void OnCollisionEnter (Collision col) {
		Movement P = col.gameObject.GetComponentInParent<Movement> ();
        if (P)
            Exit.TeleportPlayer();
	}

    void OnTriggerEnter(Collider other)
    {
        Movement P = other.gameObject.GetComponentInParent<Movement>();
        if (P)
            Exit.TeleportPlayer();
    }
}
