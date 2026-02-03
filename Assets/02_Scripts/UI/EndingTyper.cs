using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수!

public class EndingTyper : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI textComponent;
    public GameObject buttonGroup; // 버튼들을 묶어놓은 부모 오브젝트

    [Header("Text Content")]
    [TextArea(5, 10)]
    public string fullText;
    public string cursorChar = "■";

    [Header("Typing Settings")]
    public float typingSpeed = 0.08f;
    public float startDelay = 1.0f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] keySoundArray;
    public AudioClip enterSound;

    private void Start()
    {
        // 1. 시작하자마자 버튼 숨기기
        if (buttonGroup != null)
            buttonGroup.SetActive(false);

        textComponent.text = "";
        StartCoroutine(TypeWriterRoutine());
    }

    IEnumerator TypeWriterRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < fullText.Length; i++)
        {
            char c = fullText[i];
            string currentText = fullText.Substring(0, i + 1);
            textComponent.text = currentText + cursorChar;

            if (c != ' ' && c != '\n') PlayRandomKeySound();
            else if (c == '\n' && enterSound != null) audioSource.PlayOneShot(enterSound);

            if (c == '.' || c == '?' || c == '!') yield return new WaitForSeconds(typingSpeed * 3f);
            else yield return new WaitForSeconds(typingSpeed);
        }

        // 타이핑 끝
        StartCoroutine(BlinkCursor());

        // 2. 1초 뒤에 버튼 그룹 켜기
        yield return new WaitForSeconds(1.0f);
        if (buttonGroup != null)
        {
            buttonGroup.SetActive(true);
            // (선택사항) 버튼 나올 때 소리 하나 넣어주면 좋음
            if (enterSound != null) audioSource.PlayOneShot(enterSound);
        }
    }


    void PlayRandomKeySound()
    {
        if (keySoundArray.Length == 0) return;
        int randomIndex = Random.Range(0, keySoundArray.Length);
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(keySoundArray[randomIndex]);
    }

    IEnumerator BlinkCursor()
    {
        bool isVisible = true;
        while (true)
        {
            if (isVisible) textComponent.text = fullText + cursorChar;
            else textComponent.text = fullText;
            isVisible = !isVisible;
            yield return new WaitForSeconds(0.5f);
        }
    }

    // 버튼 기능 함수 추가 (인스펙터에서 연결용)

    public void GoToMainMenu()
    {
        // "MainMenu" 부분에 실제 메인 메뉴 씬 이름 적기
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        UnityEditor.EditorApplication.isPlaying = false;

        Debug.Log("게임 종료!");
        Application.Quit();
    }
}