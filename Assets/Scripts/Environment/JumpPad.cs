using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour {

    public float Height = 11f;
	public float JumpSpeed = 15f;

	public List<Transform> EffectRings = new List<Transform>();
	private List<Renderer> RingRenderers = new List<Renderer> ();

	private float LoopTime = 6f;
	private Color NormalColour;

	// Use this for initialization
	void Start () {
		NormalColour = EffectRings [0].GetComponent<Renderer> ().material.color;
		foreach (Transform ER in EffectRings)
			RingRenderers.Add (ER.GetComponent<Renderer> ());
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < EffectRings.Count; ++i) {
			float RelativeTime = (Time.time + (i * (LoopTime / EffectRings.Count))) % 6;
			EffectRings [i].localPosition = new Vector3 (0, (RelativeTime / LoopTime) * 3f - 0.5f, 0);
			float RelativeScale = ((LoopTime - RelativeTime) / LoopTime) * 0.1f + 0.89f;
			EffectRings [i].localScale = new Vector3 (RelativeScale, 0.15f, RelativeScale);
			RingRenderers[i].material.color = new Color (NormalColour.r, NormalColour.g, NormalColour.b, ((LoopTime - RelativeTime) / LoopTime));
		}
	}

	void OnTriggerEnter (Collider col) {
		PlayerMovement Player = col.gameObject.GetComponentInParent<PlayerMovement> ();
		if (Player) {
            JumpSpeed = Mathf.Sqrt(2 * Physics.gravity.magnitude * Height);
            Player.GetComponent<Rigidbody> ().velocity = transform.up * JumpSpeed;
		}
	}
}
