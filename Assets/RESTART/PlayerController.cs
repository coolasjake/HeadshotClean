using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private bool paused = false;

    private Transform head;
    private Rigidbody RB;

    public float jumpForce = 10;
    public float mouseSense = 1;
    public float moveSpeed = 1;
    public float maxSpeed = 3;
    public float frictionForce = 1;

    public bool grounded = true;

    public void BecomeGounded() { grounded = true; }

    public void BecomeUnGounded() { grounded = false; }

    public NewTrigger footTrigger;

    // Start is called before the first frame update
    void Start()
    {
        head = GetComponentInChildren<Camera>().transform;
        RB = GetComponent<Rigidbody>();

        LockCursor();

        SetupTrigger();
    }

    private void SetupTrigger()
    {
        footTrigger.collisionEnter += BecomeGounded;
        footTrigger.collisionExit += BecomeUnGounded;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnLockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        MouseMovement();
        PlayerMovement();

    }

    private void CheckPause()
    {
        //Tilde is just for debug.
        if (Input.GetButtonDown("Pause") || Input.GetKeyDown(KeyCode.Tilde))
        {
            if (paused)
            {
                paused = false;
                LockCursor();
            }
            else
            {
                paused = true;
                UnLockCursor();
            }
        }
    }

    private void MouseMovement()
    {
        float my = Input.GetAxis("Mouse Y");
        float mx = Input.GetAxis("Mouse X");

        Quaternion rot = head.transform.rotation;
        rot.eulerAngles = new Vector3(rot.eulerAngles.x - my, rot.eulerAngles.y, rot.eulerAngles.z);
        head.transform.rotation = rot;


        rot = transform.rotation;
        rot.eulerAngles = new Vector3(rot.eulerAngles.x, rot.eulerAngles.y + mx, rot.eulerAngles.z);
        transform.rotation = rot;
    }

    private void PlayerMovement()
    {
        //Get input.
        //Vector3 delta = new Vector3(Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal"));
        Vector3 delta = new Vector3();
        if (Input.GetKey(KeyCode.A))
            delta.x -= 1;
        if (Input.GetKey(KeyCode.D))
            delta.x += 1; //(1, 0, 0)
        if (Input.GetKey(KeyCode.W))
            delta.z += 1;
        if (Input.GetKey(KeyCode.S))
            delta.z -= 1;


        //Check for Jump.
        if (Input.GetButtonDown("Jump"))
            delta.y += 1;

        //Convert input to relative motion for player transform.
        Vector3 movement = Vector3.zero; //(0, 0, 0)
        movement += transform.right * Input.GetAxis("Horizontal") * moveSpeed; //(3, 0, 0)
        movement += transform.forward * Input.GetAxis("Vertical") * moveSpeed;

        if (Input.GetButtonDown("Jump"))
            movement += transform.up * jumpForce;

        //Save the velocity at the start of the frame.
        Vector3 oldVelocity = RB.velocity; //(0, 0, 0)
        //Calculate and store the new velocity.
        Vector3 newVelocity = RB.velocity + (movement * Time.deltaTime); //(0.1, 0, 0)

        //If the local non-vertical (lateral) velocity of the player is above the max speed, do not allow any increases in speed due to input.

        //Create a copy of the values without the vertical component.
        Vector3 LateralVelocityOld = new Vector3(oldVelocity.x, 0, oldVelocity.z); //(0, 0, 0)
        Vector3 LateralVelocityNew = new Vector3(newVelocity.x, 0, newVelocity.z); //(0.1, 0, 0)

        //If the new speed is too fast.
        if (LateralVelocityNew.magnitude > maxSpeed)
        {
            //If the new movement would speed up the player, reduce the magnitude so it doesn't.
            if (LateralVelocityNew.magnitude > LateralVelocityOld.magnitude)
            {
                //If the player was not at max speed yet, set them to the max speed, otherwise revert to the old speed (but with direction changes).
                if (LateralVelocityOld.magnitude < maxSpeed)
                    LateralVelocityNew = LateralVelocityNew.normalized * maxSpeed;
                else
                    LateralVelocityNew = LateralVelocityNew.normalized * LateralVelocityOld.magnitude;
            }

            //FRICTION
            //If the new lateral velocity is still greater than the max speed, reduce it by the relevant amount until it is AT the max speed.
            if (LateralVelocityNew.magnitude > maxSpeed)
            {
                LateralVelocityNew = LateralVelocityNew.normalized * Mathf.Max(maxSpeed, LateralVelocityNew.magnitude - frictionForce);
            }
        }

        //Add the vertical component back.
        newVelocity = LateralVelocityNew + new Vector3(0, newVelocity.y, 0);

        //Calculate the difference between the current and new velocity.
        Vector3 FinalVelocityChange = newVelocity - RB.velocity;
        
        //Super-friction for when the player is trying to stop moving.
        if (movement.magnitude < 0.01f && grounded)
        {
            Vector3 NewVelocity = RB.velocity;

            //Jump to zero velocity when below max speed and on the ground to give more control and prevent gliding.
            if (RB.velocity.magnitude < maxSpeed / 2)
                RB.velocity = new Vector3();
            else
            {
                //Apply a 'friction' force to the player.
                NewVelocity = NewVelocity.normalized * Mathf.Max(0, NewVelocity.magnitude - (frictionForce * Time.deltaTime));
                RB.velocity = NewVelocity;
            }
        }
        else
        {
            //Move the player the chosen direction.
            RB.velocity += FinalVelocityChange;
        }
    }
}
