using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotationDebugger : MonoBehaviour
{
    public Transform Gun;

    public void Init(Quaternion cameraLRot)
    {
        Gun.localRotation = cameraLRot;
    }
}
