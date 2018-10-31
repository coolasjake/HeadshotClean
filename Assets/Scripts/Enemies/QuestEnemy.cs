using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestEnemy : MonoBehaviour {

	// Use this for initialization
	void Start () {
		FindObjectOfType<QuestManager> ().CreateDestroyMeQuest (gameObject, "Rapid Fire Robot");
	}
}
