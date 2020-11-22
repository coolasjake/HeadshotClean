using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCounter : MonoBehaviour {

	public static int MaxBasicEnemies = 0;
	public static int BasicEnemiesKilled = 0;
	public static int MaxFollowingEnemies = 0;
	public static int FollowingEnemiesKilled = 0;
	public static int CuriousEnemiesKilled = 0;
	public static int HitsTaken = 0;
	public static bool WhiteRabbitFound = false;

	public static void UpdateScoreboard () {
		string WR;
		if (EnemyCounter.WhiteRabbitFound)
			WR = "\nWhite Robot Found!";
		else
			WR = "\nWhite Robot Not Found";

        UIManager.stat.LoadOrGetUI("AchievementsDebug").GetComponentInChildren<EnemyCounter> ().GetComponent<Text> ().text =
			"Enemies Killed: " + BasicEnemiesKilled + "/" + MaxBasicEnemies +
			"\nFollowing Enemies Killed: " + FollowingEnemiesKilled + "/" + MaxFollowingEnemies +
			"\nNumber That Were Only Curious: " + CuriousEnemiesKilled +
			WR +
			"\nHits Taken: " + HitsTaken;
	}
}
