/// <summary>
/// Scene의 이동과 그 흐름을 관리하는 싱글톤
/// </summary>
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    // StageManager 싱글톤
    public static StageManager Instance;

    [Header("게임 씬 이름 및 목록 저장")]
    public string mainManuScene;
    public string gameEndScene;
    public string[] stageSceneList;                         // 스테이지 리스트 이름
    [SerializeField] private int currentStageIndex = -1;    // 현재 스테이지 번호 (스테이지가 아닐 시 -1) 

    // 싱글톤 null 방지 & 중복 생성 방지
    void Awake()
    {
        if(Instance==null)
        {
            Debug.Log("StageManager: 싱글톤 생성");
            Instance = this;
            // 부모가 있다면 부모 해제 후 보존 (싱글톤이 상속되어 있기 때문에 안전장치 추가)
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("StageManager: 중복 싱글톤 삭제");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        #region 테스트용 코드: 중간 스테이지 부터 실행할 경우 현재 씬에 대한 인텍스 번호로 갱신
        string currentSceneName = SceneManager.GetActiveScene().name;
        for (int i = 0; i < stageSceneList.Length; i++)
        {
            if (stageSceneList[i] == currentSceneName)
            {
                currentStageIndex = i;
                return;
            }
        }
        #endregion
    }

    public void Mainmenu()
    {
        GameDataManager.Instance?.SavePlayerData();

        currentStageIndex = -1;                 // 인덱스 초기화
        SceneManager.LoadScene(mainManuScene);  // 메인메뉴 이동
    }

    /* 게임 첫 시작 */
    public void StartGame()
    {
        // 스테이지가 하나라도 존재 한다면 로드
        if (stageSceneList.Length > 0)
        {
            // 인덱스가 음수면 -1로 초기화, NextStage를 호출하면 0번째 스테이지가 로드
            if (currentStageIndex < 0) currentStageIndex = -1;
            Debug.Log($"StageManager.StartGame() currentStageIndex={currentStageIndex}");
            NextStage();
        }
        else
        {
            Debug.LogWarning("등록된 스테이지가 없습니다.");
        }
    }

    /* 다음 스테이지로 전환 */
    public void NextStage()
    {
        // 씬 넘어가기 전 저장
        GameDataManager.Instance?.SavePlayerData();

        // 다음 스테이지로 가기 위한 인덱스 증가
        int nextIndex = currentStageIndex+1;

        // 마지막 스테이지인지 확인하고, 아니라면 다음 씬으로 이동
        if(nextIndex < stageSceneList.Length)
        {
            currentStageIndex = nextIndex;
            SceneManager.LoadScene(stageSceneList[currentStageIndex]);
            Debug.Log($"{currentStageIndex}번째 스테이지로 이동");
        }
        else
        {
            SceneManager.LoadScene(gameEndScene);
        }
    }

    /* 게임 패배시 종료 씬 전환 */
    public void GameOver()
    {
        GameDataManager.Instance?.SavePlayerData();
        
        SceneManager.LoadScene(gameEndScene);
    }
}
