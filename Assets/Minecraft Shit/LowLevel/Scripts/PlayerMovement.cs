using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform head;

    public Rigidbody RB;

    public float speed = 0.1f;
    private float newSpeed = 0.1f;
    public bool accelerateSpeed = true;

    // Start is called before the first frame update
    void Start()
    {
        newSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        float xMotion = Input.GetAxis("Horizontal");
        float zMotion = Input.GetAxis("Vertical");

        if (accelerateSpeed && (xMotion != 0 || zMotion != 0))
            newSpeed = newSpeed * 1.01f;
        else
            newSpeed = speed;

        RB.velocity = Vector3.zero;
        RB.velocity += head.forward * zMotion * newSpeed;
        RB.velocity += head.right * xMotion * newSpeed;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(new Vector3(0, mouseX, 0));
        head.Rotate(new Vector3(-mouseY, 0, 0));

        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            if (Physics.Raycast(head.position, head.forward, out hit))
            {
                Debug.Log("Hit");
                GameObject GO = new GameObject();
                GO.transform.position = hit.point;
            }
        }

    }
}
