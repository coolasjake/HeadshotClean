using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementTracker : MonoBehaviour {

	public static bool InAir = false;
	public static int GunKills = 0;
	public static int AirKills = 0;
	public static int AirKillStreak = 0;
	public static int CurrentAirKillStreak = 0;
	public static int StompKills = 0;
	public static int AntiGravityKills = 0;
	public static int DoubleAntiGravityKills = 0;

	public static void TouchedTheGround () {
		InAir = false;
		if (CurrentAirKillStreak > AirKillStreak) {
			AirKillStreak = CurrentAirKillStreak;
			CurrentAirKillStreak = 0;
		}
	}

	public static void UpdateAchievements () {
		FindObjectOfType<AchievementTracker> ().GetComponent<Text> ().text =
			GunKills + " Gun Kills\n" +
			AirKills + " Air Kills\n" +
			AirKillStreak + " Air Kill Streak\n" +
			StompKills + " Stomp Kills\n" +
			AntiGravityKills + " Anti Gravity Kills\n" +
			DoubleAntiGravityKills + " Double Anti Gravity Kills";
	}

	public static void EnemyDied () {
		if (InAir) {
			AirKills += 1;
			CurrentAirKillStreak += 1;
		}
		//UpdateAchievements ();
	}
}
