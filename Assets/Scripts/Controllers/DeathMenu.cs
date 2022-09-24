using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour {

	void Start () {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.E))
			RestartProgram ();
		else if (Input.GetKeyDown (KeyCode.Escape))
			EndProgram ();
	}

	public void RestartProgram() {
		Movement.PlayerStartPosition = new Vector3 (3, 90.5f, 82);
		SceneManager.LoadScene ("TestCity");
	}

	public void EndProgram() {
		Application.Quit ();
	}
}
