using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public abstract class Shootable : MonoBehaviour
{
    [Header("Shootable")]
    public List<HitArea> hitAreas = new List<HitArea>();
    protected string _lastHitBy = "Game";
    protected string _lastDamagedBy = "Game";

    protected void ResetHitAreas()
    {
        foreach (HitArea area in hitAreas)
        {
            area.ResetHealth();
        }
    }

    /// <summary> DEPRECIATED </summary>
    public virtual void Hit(float damage)
    {

    }

    public virtual void Hit (float damage, string attacker, string areaName)
    {
        _lastHitBy = attacker;
        foreach (HitArea area in hitAreas)
        {
            if (area.name == areaName)
            {
                _lastDamagedBy = attacker;
                area.Hit(damage, attacker);
                return;
            }
        }
    }

    [System.Serializable]
    public class HitArea
    {
        [Tooltip("This must match the name of the gameObject that the collider is on to work.")]
        public string name = "Default";
        public float maxHealth = 50f;
        public float Health { get; private set; } = 0;
        public UnityEvent OnHit;
        public UnityEvent OnDestroy;

        public void ResetHealth ()
        {
            Health = maxHealth;
        }

        public void Hit(float damage, string attacker)
        {
            print(name + " hit by " + attacker + " for " + damage + ". Health now = " + Health);
            OnHit.Invoke();
            Health -= damage;
            if (Health <= 0)
                Destroy(attacker);
        }

        public void Destroy(string attacker)
        {
            print(name + " destroyed by " + attacker);
            OnDestroy.Invoke();
        }
    }
}
