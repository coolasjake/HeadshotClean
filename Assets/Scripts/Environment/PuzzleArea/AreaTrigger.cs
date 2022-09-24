using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AreaTrigger : MonoBehaviour
{
    public UnityEvent effect;

    private void OnTriggerEnter(Collider other)
    {
        effect.Invoke();
    }

    
}
