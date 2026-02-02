/// <summary>
/// Scene의 이동과 그 흐름을 관리하는 싱글톤
/// </summary>
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;
    public string[] stageList;              // 스테이지 진행 순서대로의 문자열 배열
    private int currentStageIndex = 0;      // 현재 스테이지 번호

    // 싱글톤 null 방지 & 중복 생성 방지

    private void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /* 게임 첫 시작 */
    public void StartGame()
    {
        currentStageIndex = 0;  // Stage1을 0번 인덱스에 두고 시작
        SceneManager.LoadScene(stageList[0]);
    }

    public void NextStage()
    {
        // 빌드 설정 상의 현재 씬 번호와 다음 씬 번호를 저장
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex+1;

        // 마지막 스테이지가 아니라면 다음 씬으로 이동 (다음 인덱스 < 빌드 세팅 내의 씬 갯수)
        if(nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            
        }
        
    }
}
