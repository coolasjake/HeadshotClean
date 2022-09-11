using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportWall : MonoBehaviour {

	public TPWallExit Exit;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter (Collision col) {
		Movement P = col.gameObject.GetComponentInParent<Movement> ();
		if (P)
			P.Teleport (Exit.TeleportLocation);
	}

    void OnTriggerEnter(Collider other)
    {
        Movement P = other.gameObject.GetComponentInParent<Movement>();
        if (P)
            P.Teleport(Exit.TeleportLocation);
    }
}
