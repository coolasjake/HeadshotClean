using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Shootable : MonoBehaviour
{
    [Header("Shootable")]
    public List<HitArea> hitAreas = new List<HitArea>();

    /// <summary> DEPRECIATED </summary>
    public virtual void Hit(float damage)
    {

    }

    public virtual void Hit (float damage, string attacker, string areaName) {
        foreach (HitArea area in hitAreas)
        {
            if (area.name == areaName)
            {
                area.Hit(damage, attacker);
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
        public bool handleDamage = true;
        public delegate void HitEvent(float Damage, string Attacker);
        public HitEvent OnHit;
        public delegate void AreaDestroyEvent(string Attacker);
        public AreaDestroyEvent OnDestroy;

        public HitArea ()
        {
            Health = maxHealth;
        }

        public void Hit(float damage, string attacker)
        {
            OnHit?.Invoke(damage, attacker);
            if (handleDamage == false)
                return;
            Health -= damage;
            if (Health <= 0)
                Destroy(attacker);
        }

        public void Destroy(string attacker)
        {
            OnDestroy?.Invoke(attacker);
        }
    }
}
