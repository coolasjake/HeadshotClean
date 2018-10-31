using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	void Start () {
		AchievementTracker.UpdateAchievements ();
	}

	public void RestartProgram() {
		SceneManager.LoadScene ("TestCity");
	}

	public void EndProgram() {
		Application.Quit ();
	}
}
