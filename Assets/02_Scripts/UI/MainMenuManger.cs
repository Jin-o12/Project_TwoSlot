/// <summary>
/// MainmenuScene의 모든 상호작용을 관리함
/// </summary>
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System; // Scene을 관리하기 위해 필수

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject settingsPanel;        // 게임 설정 팝업 UI

    [Header("게임 설정")]
    public Toggle fullscreenToggle;         // 전체화면 토글 버튼
    public AudioMixer audioMixer;           // 오디오 믹서
    public Slider masterVolumeSlider;       // 마스터 볼륨 조절 슬라이더

    void Start()
    {
        // fullscreenToggle에 값이 있다면 (토글이 눌러져있다면) 전체화면
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }

    /* 게임 시작 버튼 */
    public void OnClickStartGame()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("StageManager.Instance가 존재하지 않습니다.");
        }
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

    /* 설정 창 팝업 띄우기 */
    public void OnClickOpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    /* 설정 창 팝업 끄기 */
    public void OnClickCloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    /* 마스터 볼륨 슬라이더 */
    public void SetMasterVolume(float sliderValue)
    {
        // 오디오 믹서의 0dB ~ -80dB 값을 0~1 사이 값으로 바꿔줌
        float volume = Mathf.Log10(sliderValue) * 20;

        // "Master" 오디오 믹서 값을 volume으로 설정
        audioMixer.SetFloat("Master", volume);
    }

    /* 게임 설정: 전체 화면 */
    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
}