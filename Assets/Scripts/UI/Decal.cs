using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decal : MonoBehaviour {

	public float StartTime;
	public float Duration = 0.8f;
	public float MaxSize = 1;

	// Use this for initialization
	void Start () {
		StartTime = Time.time;
		MaxSize = transform.lossyScale.x;
	}
	
	// Update is called once per frame
	void Update () {
		float newSize = (StartTime - Time.time + Duration) / Duration * MaxSize;
		transform.localScale = new Vector3 (newSize, newSize, newSize);
		//GetComponent<Renderer> ().material = new Color (1, 1, 1, (StartTime - Time.time + Duration) / Duration);
		if (Time.time - StartTime > Duration)
			Destroy (gameObject);
	}
}
