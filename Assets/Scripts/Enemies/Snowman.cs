using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snowman : BaseEnemy {

	void Start() {

	}

	public override void Die ()
	{
		if (!Died) {
			EnemyCounter.BasicEnemiesKilled += 1;
			if (GetComponent<WhiteRobot> ())
				EnemyCounter.WhiteRabbitFound = true;
			EnemyCounter.UpdateScoreboard ();
			Instantiate (Resources.Load<GameObject> ("Prefabs/Lena/Melted Snowman"), transform.position, transform.rotation);
		}
		Died = true;
		Destroy (gameObject);
	}
}
