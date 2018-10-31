using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrail : MonoBehaviour {

	public float DestroyTime = 0.4f;

	private LineRenderer Line;
	private float StartTime;

	// Use this for initialization
	void Start () {
		Line = GetComponent<LineRenderer> ();
		Destroy(gameObject, DestroyTime);
		StartTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log ("Time: " + Time.time + ", Start: " + StartTime + ", Destroy: " + DestroyTime);
		//Debug.Log ((((Time.time - StartTime) / DestroyTime) - 1) * -1);
		Line.endColor = new Color (Line.endColor.r, Line.endColor.g, Line.endColor.b, (((Time.time - StartTime) / DestroyTime) - 1) * -1);
		//Line.colorGradient. = new Color (Line.endColor.r, Line.endColor.g, Line.endColor.b, (((Time.time - StartTime) / DestroyTime) - 1) * -1);
	}
}


//((Time.time - StartTime / DestroyTime) - 1) * -1