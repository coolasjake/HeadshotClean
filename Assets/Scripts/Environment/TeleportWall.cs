using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportWall : MonoBehaviour {

	public TPWallExit Exit;

	void OnCollisionEnter (Collision col) {
		PlayerMovement P = col.gameObject.GetComponentInParent<PlayerMovement> ();
        if (P)
            Exit.TeleportPlayer();
	}

    void OnTriggerEnter(Collider other)
    {
        PlayerMovement P = other.gameObject.GetComponentInParent<PlayerMovement>();
        if (P)
            Exit.TeleportPlayer();
    }
}
