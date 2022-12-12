using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    void OnCollisionStay(Collision col)//Enter(Collision col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerHealth P = col.gameObject.GetComponentInParent<PlayerHealth>();
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
