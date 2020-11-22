using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewTrigger : MonoBehaviour
{
    public delegate void GroundedEvents();
    public event GroundedEvents collisionEnter;

    public delegate void UngroundedEvents();
    public event UngroundedEvents collisionExit;

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Trigger Entered: " + other.name);
        collisionEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger Exit: " + other.name);
        collisionExit?.Invoke();
    }
}
