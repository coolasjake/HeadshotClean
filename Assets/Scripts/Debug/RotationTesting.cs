using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTesting : MonoBehaviour
{
    public Camera camera;
    public Transform body;
    public LineRenderer lineCamU;
    public LineRenderer lineCamB;
    public LineRenderer lineOne;
    public LineRenderer lineTwo;
    public LineRenderer lineThree;
    public LineRenderer lineFour;
    public float sense = 1f;
    
    float bodyDiff = 0;
    float camDiff = 0;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        CameraMove();


        if (Input.GetKeyDown(KeyCode.R))
        {
            PointFeetAtFacing(false);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            RotateBodyByAngle();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            lineThree.transform.position = body.position;
            lineThree.SetPosition(1, Vector3.Cross(Vector3.up, camera.transform.right) * 2f);

            lineFour.transform.position = body.position;
            lineFour.SetPosition(1, Vector3.up * 2f);

            //StartCoroutine(LerpRotate(Quaternion.LookRotation(Vector3.Cross(Vector3.up, camera.transform.right), Vector3.up)));
            StartCoroutine(LerpRotate(Quaternion.LookRotation(Vector3.zero, Vector3.up)));
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.RotateAround(transform.position, camera.transform.up, -1 * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.RotateAround(transform.position, camera.transform.up, 1 * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            PointFeetAtGround(false);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            PointFeetAtGround(true);
        }

        lineCamB.transform.position = body.position;
        lineCamB.SetPosition(1, -camera.transform.forward * 3f);

        lineCamU.transform.position = body.position;
        lineCamU.SetPosition(1, camera.transform.up * 3f);
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

    private void PointFeetAtFacing(bool rotateBody)
    {
        Vector3 newDir = camera.transform.forward;

        float rotation = Vector3.SignedAngle(-transform.up, newDir, camera.transform.right);
        print("First angle: " + rotation);
        transform.RotateAround(transform.position, camera.transform.right, rotation);

        /*
        rotation = Vector3.SignedAngle(-transform.up, newDir, body.forward);
        print("Second angle: " + rotation);
        transform.Rotate(body.forward, rotation);
        */

        bodyDiff = Vector3.SignedAngle(body.forward, newDir, body.up);
        camDiff = Vector3.SignedAngle(camera.transform.forward, newDir, camera.transform.right);

        if (rotateBody)
            RotateBodyByAngle();
        RotateCameraByAngle();
    }

    private void PointFeetAtFacingOLD(bool rotateBody)
    {
        Vector3 newDir = camera.transform.forward;

        lineOne.transform.position = body.position;
        lineOne.SetPosition(1, camera.transform.up * 2f);

        lineTwo.transform.position = body.position;
        lineTwo.SetPosition(1, -camera.transform.forward * 2f);

        /*
        float rotation = Vector3.SignedAngle(-transform.up, newDir, camera.transform.right);
        print("First angle: " + rotation);
        transform.rotation *= Quaternion.AngleAxis(rotation, camera.transform.right);

        rotation = Vector3.SignedAngle(-transform.up, newDir, body.forward);
        print("Second angle: " + rotation);
        transform.Rotate(body.forward, rotation);
        */
        transform.rotation = Quaternion.LookRotation(camera.transform.up, -camera.transform.forward);

        lineThree.transform.position = body.position;
        lineThree.SetPosition(1, transform.forward * 2f);

        lineFour.transform.position = body.position;
        lineFour.SetPosition(1, transform.up * 2f);

        bodyDiff = Vector3.SignedAngle(body.forward, newDir, body.up);
        camDiff = Vector3.SignedAngle(camera.transform.forward, newDir, camera.transform.right);

        if (rotateBody)
            RotateBodyByAngle();
        RotateCameraByAngle();
    }

    private void PointFeetAtGround(bool rotateBody)
    {
        Vector3 oldCamForward = camera.transform.forward;

        transform.rotation = Quaternion.LookRotation(Vector3.zero, Vector3.up);

        /*
        float rotation = Vector3.SignedAngle(-transform.up, Vector3.down, body.forward);
        print("First angle: " + rotation);
        transform.RotateAround(transform.position, body.forward, rotation);
        
        rotation = Vector3.SignedAngle(-transform.up, Vector3.down, body.right);
        print("Second angle: " + rotation);
        transform.RotateAround(transform.position, body.right, rotation);
        */

        //Up and target -> new
        //Up and new = desired body forwards

        Vector3 desiredBodyRight = Vector3.Cross(body.up, oldCamForward);

        bodyDiff = Vector3.SignedAngle(body.right, desiredBodyRight, body.up);
        camDiff = Vector3.SignedAngle(camera.transform.forward, oldCamForward, camera.transform.right);

        if (rotateBody)
            RotateBodyByAngle();
        RotateCameraByAngle();
    }

    private void RotateBodyByAngle()
    {
        float bodyAngle = body.localRotation.eulerAngles.y;
        bodyAngle += bodyDiff;
        Quaternion newRot = new Quaternion();
        newRot.eulerAngles = new Vector3(0, bodyAngle, 0);
        body.localRotation = newRot;
    }

    private void RotateCameraByAngle()
    {
        float cameraAngle = camera.transform.localRotation.eulerAngles.x;
        cameraAngle += camDiff;
        cameraAngle = ClampAngleTo180(cameraAngle);
        Quaternion newRot = new Quaternion();
        newRot.eulerAngles = new Vector3(cameraAngle, 0, 0);
        camera.transform.localRotation = newRot;
    }

    public IEnumerator LerpRotate(Quaternion target)
    {
        Quaternion startingRotation = transform.rotation;
        float startTime = Time.time;
        float duration = 5f;
        while (transform.rotation != target && Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            transform.rotation = Quaternion.Lerp(startingRotation, target, t);
            yield return new WaitForEndOfFrame();
        }
    }
}
