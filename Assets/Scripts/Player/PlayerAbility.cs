using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAbility : MonoBehaviour {
	protected bool Disabled = false;

	private float _resource = 0;
    [Range(1, 1000000)]
	public float MaxResource = 3;
	public float RegenPerSecond = 0.5f;
	public float MinToUse = 1;
    public bool UseResource = true;

    public virtual void Disable()
    {
        Disabled = true;
    }

    public virtual void Enable()
    {
        Disabled = false;
    }

    /// <summary> Get the numeric value of resource. </summary>
    public float Resource { get { return _resource; } }

    /// <summary> Get the fraction of resource out of Max resource (0-1). </summary>
    public float ResourceFraction { get { return _resource / MaxResource; } }

    /// <summary> Set resource to MaxResource's value. </summary>
    public void FillResource()
    {
        _resource = MaxResource;
    }

    /// <summary> Add value to resource, capping at MaxResource. </summary>
    public void AddResource(float value)
    {
        _resource += value;
        _resource = Mathf.Min(Resource, MaxResource);
    }

    /// <summary> Perform a 'tick' of resource regeneration (using RegenPerSecond), capping at MaxResource. </summary>
    public void RegenerateResource()
    {
        _resource += RegenPerSecond * Time.deltaTime;
        _resource = Mathf.Min(_resource, MaxResource);
    }

    /// <summary> Perform a 'tick' of resource regeneration (using RegenPerSecond), multiplied by the value, and capping at MaxResource. </summary>
    public void RegenerateResource(float multiplier)
    {
        _resource += RegenPerSecond * Time.deltaTime * multiplier;
        _resource = Mathf.Min(_resource, MaxResource);
    }

    /// <summary> Checks if resource is above MinToUse, returning false if not, and reducing by given value if true. </summary>
    public virtual bool StartConsumeResource(float value)
    {
        if (_resource < MinToUse)
            return false;
        _resource -= value;
        return true;
    }
    
    /// <summary> Reduce resource by the given value, returning false if resource is ALREADY zero. </summary>
    public virtual bool ConsumeResource(float value)
    {
        if (Resource <= 0)
            return false;
        _resource -= value;
        return true;
    }

    /// <summary> Reduce resource by the given value, down to a minimum defined by the second parameter. </summary>
    public virtual void ConsumeResourceGreedy(float value, float minDebt)
    {
        _resource -= value;
        _resource = Mathf.Max(_resource, minDebt);
    }
}
