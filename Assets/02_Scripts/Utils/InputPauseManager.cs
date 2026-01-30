using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public static void TogglePause()
    {
        IsPaused = !IsPaused;

        // 시간 멈추기
        Time.timeScale = IsPaused ? 0f : 1f;

        
    }
}
