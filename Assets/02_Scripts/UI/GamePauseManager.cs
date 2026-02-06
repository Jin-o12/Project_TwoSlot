using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;   // UI (Toggle) ����� ���� �߰�
using UnityEngine.Audio; // AudioMixer ����� ���� �߰�

public class GamePauseManager : MonoBehaviour
{
    // �̱��� ����
    public static GamePauseManager Instance { get; private set; }

    [Header("������ ������Ʈ��")]
    public GameObject pausePanel;       // �Ͻ����� UI �г�
    public GameObject blurVolume;       // ���� ����
    public GameObject gameplayUI;       // HUD (ü�¹� ��)

    [Header("ȯ�漳�� UI")]
    public GameObject settingsPanelUI;  // ȯ�漳�� â ������

    // �� [�߰���] ȯ�漳�� ��� ����
    [Header("ȯ�漳�� ��� ����")]
    public AudioMixer audioMixer;       // ����� �ͼ� (MainMixer)
    public Toggle fullscreenToggle;     // ��üȭ�� ���

    public bool IsPaused { get; private set; } = false;

    public static bool Paused => Instance != null && Instance.IsPaused;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (settingsPanelUI != null)
            settingsPanelUI.SetActive(false);
    }

    // �� [�߰���] ������ �� ���� ��üȭ�� ���¸� ��ۿ� �ݿ�
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
            // ����â�� ���������� ����â�� �ݱ�
            if (settingsPanelUI != null && settingsPanelUI.activeSelf)
            {
                CloseSettings();
            }
            // �ƴϸ� �Ͻ����� ���
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

        AudioListener.pause = true; //오디오 재생도 일시정지

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

        AudioListener.pause = false; // 오디오 다시 재생

        // Ŀ�� ��ױ�
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
    // �� [�߰���] ���� �� ��üȭ�� ��� ��
    // ================================================================================

    // �����̴��� ������ �Լ�
    public void SetMasterVolume(float sliderValue)
    {
        if (audioMixer == null) return;

        // �α� ������ ��ȯ (�ڿ������� �Ҹ� ����)
        float volume = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        audioMixer.SetFloat("Master", volume);
    }

    // ��۰� ������ �Լ�
    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
}