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
            Instance = this;
            // 부모가 있다면 부모 해제 후 보존 (싱글톤이 상속되어 있기 때문에 안전장치 추가)
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        #region 테스트용 코드: 중간 스테이지 부터 실행할 경우 현재 씬에 대한 인텍스 번호로 갱신
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == mainManuScene) return;  // 메인메뉴면 -1 유지
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

    void Update()
    {
        #region 테스트용 코드: 씬 전환 테스트를 위한 임시 코드
        if(Input.GetKeyDown(KeyCode.Return))
        {
            NextStage();
        }
        #endregion
    }

    public void Mainmenu()
    {
        currentStageIndex = -1;                 // 메인메뉴 인덱스로 초기화
        SceneManager.LoadScene(mainManuScene);  // 메인메뉴 이동
    }

    /* 게임 첫 시작 */
    public void StartGame()
    {
        // 스테이지가 하나라도 존재 한다면 로드
        if(stageSceneList.Length > 0)
        {
            SceneManager.LoadScene(stageSceneList[0]);
        }
        else
        {
            Debug.LogWarning("등록된 스테이지가 없습니다.");
        }
    }

    public void NextStage()
    {
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
        SceneManager.LoadScene(gameEndScene);
    }
}
