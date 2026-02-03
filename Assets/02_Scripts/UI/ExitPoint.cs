using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitPoint : MonoBehaviour
{
    private static StageManager stageManager;

    void Awake()
    {
        stageManager = StageManager.Instance;
    }

    public void OnColliderEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            stageManager.GameOver();
        }
    }
}
