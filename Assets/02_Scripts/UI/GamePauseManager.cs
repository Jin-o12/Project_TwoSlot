using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;   // UI (Toggle) 사용을 위해 추가
using UnityEngine.Audio; // AudioMixer 사용을 위해 추가

public class GamePauseManager : MonoBehaviour
{
    // 싱글톤 패턴
    public static GamePauseManager Instance { get; private set; }

    [Header("연결할 오브젝트들")]
    public GameObject pausePanel;       // 일시정지 UI 패널
    public GameObject blurVolume;       // 블러 볼륨
    public GameObject gameplayUI;       // HUD (체력바 등)

    [Header("환경설정 UI")]
    public GameObject settingsPanelUI;  // 환경설정 창 프리팹

    // ▼ [추가됨] 환경설정 기능 연결
    [Header("환경설정 기능 연결")]
    public AudioMixer audioMixer;       // 오디오 믹서 (MainMixer)
    public Toggle fullscreenToggle;     // 전체화면 토글

    public bool IsPaused { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (settingsPanelUI != null)
            settingsPanelUI.SetActive(false);
    }

    // ▼ [추가됨] 시작할 때 현재 전체화면 상태를 토글에 반영
    private void Start()
    {
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 설정창이 켜져있으면 설정창만 닫기
            if (settingsPanelUI != null && settingsPanelUI.activeSelf)
            {
                CloseSettings();
            }
            // 아니면 일시정지 토글
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
        pausePanel.SetActive(true);
        if (blurVolume != null) blurVolume.SetActive(true);
        if (gameplayUI != null) gameplayUI.SetActive(false);

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        IsPaused = false;
        pausePanel.SetActive(false);
        if (settingsPanelUI != null) settingsPanelUI.SetActive(false);
        if (blurVolume != null) blurVolume.SetActive(false);
        if (gameplayUI != null) gameplayUI.SetActive(true);

        Time.timeScale = 1f;

        // 커서 잠그기
        // Cursor.visible = false;

    }

    public void OpenSettings()
    {
        if (settingsPanelUI == null) return;
        pausePanel.SetActive(false);
        settingsPanelUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanelUI != null) settingsPanelUI.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    // ================================================================================
    // ▼ [추가됨] 볼륨 및 전체화면 기능 ▼
    // ================================================================================

    // 슬라이더와 연결할 함수
    public void SetMasterVolume(float sliderValue)
    {
        if (audioMixer == null) return;

        // 로그 스케일 변환 (자연스러운 소리 조절)
        float volume = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        audioMixer.SetFloat("Master", volume);
    }

    // 토글과 연결할 함수
    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
}