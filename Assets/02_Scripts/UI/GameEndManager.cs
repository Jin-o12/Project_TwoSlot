/// <summary>
/// GameEndScene의 모든 상호작용을 관리함
/// </summary>
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndManager : MonoBehaviour
{
    [Header("필요 컴포넌트")]
    private StageManager stageManager;

    void Awake()
    {
        stageManager = StageManager.Instance;
    }

    /* 메인 화면으로 이동 */
    public void OnClickMainMenu()
    {
        stageManager.Mainmenu();
    }

    /* 게임 종료 버튼 */
    public void OnClickExitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                    Application.Quit();
        #endif
        Debug.Log("게임이 종료 되었습니다");
    }
}
