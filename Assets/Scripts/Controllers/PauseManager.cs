using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PauseManager
{
    private static bool _isPaused = false;
    public static GameObject pauseMenu;

    public static bool Paused
    {
        get { return _isPaused; }
    }

    public static bool SetPaused
    {
        set { _isPaused = value; }
    }

    public static void TogglePause()
    {
        if (_isPaused)
            UnPause();
        else
            Pause();
    }

    public static void Pause()
    {
        _isPaused = true;

        if (pauseMenu != null)
            pauseMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
    }

    public static void UnPause()
    {
        _isPaused = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
    }
}
