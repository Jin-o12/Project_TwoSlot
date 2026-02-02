using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // �� �̵��� ���� �ʼ�!

public class MainMenuManager : MonoBehaviour
{
    [Header("전체화면 버튼의 눌림 여부")]
    public Toggle fullscreenToggle;

    void Start()
    {
        // fullscreenToggle에 값이 있다면 (토글이 눌러져있다면) 전체화면
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }
    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    [Header("�̵��� �� �̸��� ���⿡ ��������")]
    public string gameSceneName = "GameScene"; // �⺻���� GameScene (�ν����Ϳ��� ���� ����)

    // ���� ���� ��ư (�� �̵�)
    public void OnClickStartGame()
    {
        Debug.Log("���� ���� ��ư Ŭ��! �̵��� ��: " + gameSceneName);

        // �� �̸��� ������� �ʴٸ� �̵�
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("�̵��� �� �̸��� �������� �ʾҽ��ϴ�! GameManager�� Ȯ�����ּ���.");
        }
    }

    // ���� ���� ��ư
    public void OnClickExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        Debug.Log("���� ����");
    }
    [Header("���� â �г�(UI)�� ���⿡ �����ϼ���")]
    public GameObject settingsPanel; // ���� â ������Ʈ�� ���� ����

    // 1. ���� ��ư�� ������ �� -> â �ѱ�
    public void OnClickOpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // 2. �ݱ� ��ư�� ������ �� -> â ����
    public void OnClickCloseSettings()
    {
        settingsPanel.SetActive(false);
    }
    [Header("����� �ͼ� ����")]
    public AudioMixer audioMixer; // �Ʊ� ���� MainMixer ���� ��

    [Header("�����̴� ����")]

    public Slider masterVolumeSlider; // �����̴� UI ���� ��



    // �����̴��� ������ ������ �� �Լ��� �����
    public void SetMasterVolume(float sliderValue)
    {
        // �����̴� ��(0.0001 ~ 1)�� ���ú�(-80 ~ 0)�� ��ȯ�ϴ� ����
        // �α�(Log10)�� ��� �ڿ������� �پ��ϴ�.
        float volume = Mathf.Log10(sliderValue) * 20;

        // "Master"�� �Ʊ� 2�ܰ迡�� ������ �� �̸��Դϴ�!
        audioMixer.SetFloat("Master", volume);
    }
}