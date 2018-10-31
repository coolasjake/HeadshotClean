using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPWallExit : MonoBehaviour {

	//public Vector3 TeleportLocation;
	public Vector3 ExitPoint;

	void OnDrawGizmosSelected () {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere (TeleportLocation, 0.1f);
	}

	public Vector3 TeleportLocation {
		get { return transform.position + (ExitPoint.x * transform.right) + (ExitPoint.y * transform.up) + (ExitPoint.z * transform.forward); }  //+ (transform.up * (2.5f + YDistanceFromCenter)) + (transform.right * XDistanceFromCenter);}
	}
}
