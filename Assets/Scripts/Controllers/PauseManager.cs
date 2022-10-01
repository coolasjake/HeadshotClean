using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PauseManager
{
    private static bool _isPaused = false;

    public static bool Paused
    {
        get { return _isPaused; }
    }

    public static bool SetPaused
    {
        set { _isPaused = value; }
    }

    public static void Pause()
    {
        _isPaused = true;
    }

    public static void UnPause()
    {
        _isPaused = false;
    }
}
