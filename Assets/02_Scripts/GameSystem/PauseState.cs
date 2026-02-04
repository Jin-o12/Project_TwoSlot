using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseState : MonoBehaviour
{
    public static bool IsPaused =>
        (GamePauseManager.Instance != null && GamePauseManager.Instance.IsPaused)
        || Time.timeScale == 0f;
}
