using UnityEngine;

public class ExitPoint : MonoBehaviour
{
    private StageManager stageManager;
    private GameDataManager gameDataManager;

    void Awake()
    {
        stageManager = StageManager.Instance;
        gameDataManager = GameDataManager.Instance;
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            gameDataManager.SavePlayerData();
            stageManager.NextStage();
        }
    }
}
