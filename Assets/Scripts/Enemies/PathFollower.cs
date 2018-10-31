using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour {

	public GameObject PathObject;
	public float MaxSpeed = 5f;
	[Range(0.01f, 0.2f)]
	public float LerpValue = 0.04f;

	private List<Vector3> Path = new List<Vector3> ();
	private int CurrentNode = 0;
	private Rigidbody RB;

	// Use this for initialization
	void Start () {
		RB = GetComponent<Rigidbody> ();

		//Transform the Node objects into a list of Vector3, and set the current node to be the closest one.
		Node[] Nodes = PathObject.GetComponentsInChildren<Node> ();
		float MinimumDistance = float.PositiveInfinity;
		Vector3 NearestNode = Nodes[0].transform.position;
		foreach (Node N in Nodes) {
			Path.Add (N.transform.position);
			float Dist = Vector3.Distance (N.transform.position, transform.position);
			if (Dist < MinimumDistance) {
				MinimumDistance = Dist;
				NearestNode = N.transform.position;
			}
		}
		CurrentNode = Path.IndexOf (NearestNode);
	}

	// Update is called once per frame
	void Update () {
		Vector3 DesiredPos = Path [CurrentNode];

		Vector3 DesiredVelocity = Vector3.MoveTowards (transform.position, DesiredPos, 1) - transform.position;
		DesiredVelocity = DesiredVelocity.normalized * MaxSpeed;

		//float LerpValue = 0.04f; //(MaxSpeed * 100) / Vector3.Distance (transform.position, DesiredPos);
		if (LerpValue > 0.2f)
			LerpValue = 0.2f;
		if (LerpValue < 0.01f)
			LerpValue = 0.01f;

		RB.velocity = Vector3.Lerp (RB.velocity, DesiredVelocity, LerpValue);

		transform.rotation = Quaternion.LookRotation (RB.velocity.normalized, transform.up);
		Quaternion OnlyYRotation = new Quaternion ();
		OnlyYRotation.eulerAngles = new Vector3 (0, transform.rotation.eulerAngles.y, 0);
		transform.rotation = OnlyYRotation;
	}

	void OnTriggerEnter (Collider col) {
		if (col.GetComponent<Node> ()) {
			CurrentNode += 1;
			if (CurrentNode >= Path.Count)
				CurrentNode = 0;
		}

	}
}
