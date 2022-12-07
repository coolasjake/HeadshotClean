using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
        //public Vector3 TeleportLocation;
    public Vector3 ExitPoint;
    public Vector2 Rotation;
    public bool DeleteVelocity = true;
    public bool ResetGravity = true;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(TeleportLocation, 0.1f);
        Quaternion rot = new Quaternion();
        rot.eulerAngles = rot * Vector3.forward;
        Gizmos.DrawLine(TeleportLocation, TeleportLocation + rot * Vector3.forward);
    }

    public Vector3 TeleportLocation
    {
        get { return transform.position + (ExitPoint.x * transform.right) + (ExitPoint.y * transform.up) + (ExitPoint.z * transform.forward); }  //+ (transform.up * (2.5f + YDistanceFromCenter)) + (transform.right * XDistanceFromCenter);}
    }
}
