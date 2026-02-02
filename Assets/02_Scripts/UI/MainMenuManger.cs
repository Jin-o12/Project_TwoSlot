using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수!

public class MainMenuManager : MonoBehaviour
{
    [Header("전체화면 토글 연결")]
    public Toggle fullscreenToggle;

    void Start()
    {
        // 1. 기존 볼륨 슬라이더 설정 (있다면 유지)

        // 2. 게임 켜자마자 현재 화면 상태를 토글에 반영 (중요!)
        // (이게 없으면, 이미 전체화면인데 체크가 풀려있거나 하는 버그가 생김)
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }
    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    [Header("이동할 씬 이름을 여기에 적으세요")]
    public string gameSceneName = "GameScene"; // 기본값은 GameScene (인스펙터에서 변경 가능)

    // 게임 시작 버튼 (씬 이동)
    public void OnClickStartGame()
    {
        Debug.Log("게임 시작 버튼 클릭! 이동할 씬: " + gameSceneName);

        // 씬 이름이 비어있지 않다면 이동
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("이동할 씬 이름이 설정되지 않았습니다! GameManager를 확인해주세요.");
        }
    }

    // 게임 종료 버튼
    public void OnClickExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        Debug.Log("게임 종료");
    }
    [Header("설정 창 패널(UI)을 여기에 연결하세요")]
    public GameObject settingsPanel; // 설정 창 오브젝트를 담을 변수

    // 1. 설정 버튼을 눌렀을 때 -> 창 켜기
    public void OnClickOpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // 2. 닫기 버튼을 눌렀을 때 -> 창 끄기
    public void OnClickCloseSettings()
    {
        settingsPanel.SetActive(false);
    }
    [Header("오디오 믹서 연결")]
    public AudioMixer audioMixer; // 아까 만든 MainMixer 넣을 곳

    [Header("슬라이더 연결")]

    public Slider masterVolumeSlider; // 슬라이더 UI 넣을 곳



    // 슬라이더를 움직일 때마다 이 함수가 실행됨
    public void SetMasterVolume(float sliderValue)
    {
        // 슬라이더 값(0.0001 ~ 1)을 데시벨(-80 ~ 0)로 변환하는 공식
        // 로그(Log10)를 써야 자연스럽게 줄어듭니다.
        float volume = Mathf.Log10(sliderValue) * 20;

        // "Master"는 아까 2단계에서 설정한 그 이름입니다!
        audioMixer.SetFloat("Master", volume);
    }
}