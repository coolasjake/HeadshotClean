using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerHealth P = col.rigidbody.GetComponent<PlayerHealth>();
            if (P)
                P.Kill("Lava");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerHealth P = other.gameObject.GetComponentInParent<PlayerHealth>();
            if (P)
                P.Kill("Lava");
        }
    }
}
