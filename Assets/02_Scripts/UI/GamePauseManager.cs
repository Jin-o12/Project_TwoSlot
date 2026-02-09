using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;    // UI (Toggle) 제어를 위해 추가
using UnityEngine.Audio; // AudioMixer 제어를 위해 추가

public class GamePauseManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GamePauseManager Instance { get; private set; }

    [Header("게임플레이 오브젝트")]
    public GameObject pausePanel;       // 일시정지 UI 패널
    public GameObject blurVolume;       // 블러 효과 볼륨
    public GameObject gameplayUI;       // HUD (체력바 등)

    [Header("환경설정 UI")]
    public GameObject settingsPanelUI;  // 환경설정 창 패널

    [Header("환경설정 기능 연결")]
    public AudioMixer audioMixer;       // 오디오 믹서 (MainMixer)
    public Toggle fullscreenToggle;     // 전체화면 토글

    // 현재 일시정지 상태인지 확인하는 변수
    public bool IsPaused { get; private set; } = false;

    // 외부(문서 보기 등)에서 일시정지를 걸었는지 확인하는 변수 [추가됨]
    // 이 변수가 true면 ESC를 눌러도 일시정지 메뉴가 뜨지 않습니다.
    public bool IsExternalPaused { get; set; } = false;

    // 어디서든 GamePauseManager.Paused로 접근 가능하게 하는 프로퍼티
    public static bool Paused => Instance != null && Instance.IsPaused;

    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 시작 시 환경설정 창은 꺼둠
        if (settingsPanelUI != null)
            settingsPanelUI.SetActive(false);
    }

    private void Start()
    {
        // 시작 시 전체화면 상태에 따라 토글 값 설정
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }

    void Update()
    {
        // [중요 수정] 문서나 지도를 보고 있는 상태(IsExternalPaused)라면,
        // ESC 키 입력을 무시하여 일시정지 메뉴가 겹쳐 뜨는 것을 방지합니다.
        if (IsExternalPaused) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 1. 환경설정 창이 켜져있으면 -> 환경설정만 닫고 일시정지 메뉴로 돌아감
            if (settingsPanelUI != null && settingsPanelUI.activeSelf)
            {
                CloseSettings();
            }
            // 2. 아니면 -> 게임을 일시정지하거나 재개함
            else
            {
                if (IsPaused) ResumeGame();
                else PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        IsPaused = true;

        // UI 제어
        if (pausePanel != null) pausePanel.SetActive(true);
        if (blurVolume != null) blurVolume.SetActive(true);
        if (gameplayUI != null) gameplayUI.SetActive(false);

        // 시간 정지
        Time.timeScale = 0f;

        // 오디오 정지
        AudioListener.pause = true;

        // 마우스 커서 보이기 (메뉴 조작을 위해)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        IsPaused = false;

        // UI 제어
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanelUI != null) settingsPanelUI.SetActive(false);
        if (blurVolume != null) blurVolume.SetActive(false);
        if (gameplayUI != null) gameplayUI.SetActive(true);

        // 시간 재개
        Time.timeScale = 1f;

        // 오디오 재개
        AudioListener.pause = false;

        // 마우스 커서 숨기기 (FPS 게임이라면 다시 잠금)
        // 상황에 따라 주석 해제 필요
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenSettings()
    {
        if (settingsPanelUI == null) return;
        if (pausePanel != null) pausePanel.SetActive(false);
        settingsPanelUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanelUI != null) settingsPanelUI.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; // 종료 전 시간 복구 (안전장치)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // 씬 이동 전 시간 복구
        SceneManager.LoadScene("TitleScene");
    }

    // ================================================================================
    // 환경설정 관련 기능
    // ================================================================================

    public void SetMasterVolume(float sliderValue)
    {
        if (audioMixer == null) return;
        // 로그 스케일 변환 (자연스러운 볼륨 조절)
        float volume = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        audioMixer.SetFloat("Master", volume);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
}