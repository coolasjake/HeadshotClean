using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityCircle : MonoBehaviour {

	//public float Opacity = 0;
	public SpriteRenderer Circle;
	private Color C;

	public void Shift (Vector3 Pos, Quaternion Rot) {
		transform.position = Pos;
		transform.rotation = Rot;
		C = Circle.color;
		Circle.color = new Color (C.r, C.g, C.b, 1);
	}

	// Update is called once per frame
	void Update () {
		C = Circle.color;
		if (C.a > 0)
			Circle.color = new Color (C.r, C.g, C.b, C.a - Time.deltaTime);
	}
}
