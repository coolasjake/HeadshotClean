using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseFailsafe : MonoBehaviour {

	public bool InsideSomething = false;
	public bool InsideEnemy = false;
	public Shootable Enemy;
	public Shootable LatestEnemy;

	public int NumberOfStructureCollisions = 0;
	public int NumberOfShooteableCollisions = 0;
	public List<Shootable> IntersectingShooteables = new List<Shootable> ();
	public List<GameObject> IntersectingStructures = new List<GameObject> ();

	public void ExplodedEnemy (Shootable EnemyThatExploded) {
		IntersectingShooteables.Remove (EnemyThatExploded);
		NumberOfShooteableCollisions -= 1;
		CalculateState ();
	}

	private void CalculateState () {
		if (NumberOfShooteableCollisions == 1 && IntersectingShooteables.Count == 1) {
			Enemy = IntersectingShooteables [0];
			InsideEnemy = true;
		} else {
			Enemy = null;
			InsideEnemy = false;
		}

		if (IntersectingShooteables.Count + IntersectingStructures.Count > 0)
			InsideSomething = true;
		else
			InsideSomething = false;
		/*
		if (NumberOfStructureCollisions + NumberOfShooteableCollisions > 0)
			InsideSomething = true;
		else
			InsideSomething = false;
		*/
	}

	void OnTriggerEnter (Collider col) {
		if (col.isTrigger)
			return;
		//Debug.Log ("Phase Collided With: " + col.gameObject.name + ", on Layer: " + col.gameObject.layer);
		Shootable EnemyComponent = col.gameObject.GetComponentInParent<Shootable> ();
		if (EnemyComponent) {
			NumberOfShooteableCollisions += 1;
			InsideEnemy = true;
			IntersectingShooteables.Add (EnemyComponent);
		} else if (col.gameObject.layer != 10) {
			NumberOfStructureCollisions += 1;
			InsideSomething = true;
			IntersectingStructures.Add (col.gameObject);
		}
		CalculateState ();
	}

	void OnTriggerExit (Collider col) {
		if (col.isTrigger)
			return;
		if (col.gameObject.GetComponentInParent<Shootable> ()) {
			if (IntersectingShooteables.Remove (col.gameObject.GetComponentInParent<Shootable> ()))
				NumberOfShooteableCollisions -= 1;
		} else if (col.gameObject.layer != 10) {
			if (IntersectingStructures.Remove (col.gameObject))
				NumberOfStructureCollisions -= 1;
		}
		CalculateState ();
	}
}
