using UnityEngine;

public class ExitPoint : MonoBehaviour
{
    private static StageManager stageManager;

    void Awake()
    {
        stageManager = StageManager.Instance;
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            stageManager.NextStage();
        }
    }
}
