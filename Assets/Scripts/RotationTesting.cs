using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTesting : MonoBehaviour
{
    public Camera camera;
    public Transform body;
    public float sense = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CameraMove();


        if (Input.GetKeyDown(KeyCode.F))
        {
            RotateToFacing();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            RotateToNormal();
        }
    }

    private void CameraMove()
    {
        float bodyAngle = body.localRotation.eulerAngles.y;
        //Rotate player
        bodyAngle += Input.GetAxis("Mouse X") * sense;
        Quaternion NewBodRot = new Quaternion();
        NewBodRot.eulerAngles = new Vector3(0, bodyAngle, 0);
        body.localRotation = NewBodRot;

        
        float cameraAngle = camera.transform.localRotation.eulerAngles.x;
        cameraAngle = ClampAngleTo180(cameraAngle);

        cameraAngle -= Input.GetAxis("Mouse Y") * sense;
        cameraAngle = Mathf.Clamp(cameraAngle, -90, 90);

        Quaternion newRot = new Quaternion();
        newRot.eulerAngles = new Vector3(cameraAngle, 0, 0);
        camera.transform.localRotation = newRot;
    }

    /// <summary>
    /// Clamps the angle so that it is within the range [-180, 180], while maintaining the relative direction of the angle.
    /// </summary>
    public static float ClampAngleTo180(float angle)
    {
        while (angle > 180)
            angle -= 360;

        while (angle < -180)
            angle += 360;

        return angle;
    }

    private void RotateToFacing()
    {
        transform.localRotation = Quaternion.LookRotation(camera.transform.up, -camera.transform.forward);
    }

    private void RotateToNormal()
    {

    }
}
