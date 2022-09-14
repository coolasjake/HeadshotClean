using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPWallExit : MonoBehaviour {

	//public Vector3 TeleportLocation;
	public Vector3 ExitPoint;
    public Vector2 Rotation;
    public bool DeleteVelocity = true;
    public bool ResetGravity = true;

	void OnDrawGizmosSelected () {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere (TeleportLocation, 0.1f);
        Quaternion rot = new Quaternion();
        rot.eulerAngles = rot * Vector3.forward;
        Gizmos.DrawLine(TeleportLocation, TeleportLocation + rot * Vector3.forward);
    }

    public void TeleportPlayer()
    {
        Gravity gravScript = Movement.ThePlayer.GetComponent<Gravity>();
        if (gravScript)
            gravScript.ResetToWorldGravity();
        Movement.ThePlayer.Teleport(TeleportLocation, Rotation.x, Rotation.y, DeleteVelocity);
    }

	public Vector3 TeleportLocation {
		get { return transform.position + (ExitPoint.x * transform.right) + (ExitPoint.y * transform.up) + (ExitPoint.z * transform.forward); }  //+ (transform.up * (2.5f + YDistanceFromCenter)) + (transform.right * XDistanceFromCenter);}
	}
}
