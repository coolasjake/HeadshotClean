using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugToggle : MonoBehaviour
{
    public static bool doPrint = false;
    public static bool doLog = false;
    public static bool doMessage = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            doPrint = !doPrint;

        if (Input.GetKeyDown(KeyCode.L))
            doLog = !doLog;

        if (Input.GetKeyDown(KeyCode.M))
            doMessage = !doMessage;
    }
}
