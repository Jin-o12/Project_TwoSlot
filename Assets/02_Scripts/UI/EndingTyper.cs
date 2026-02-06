using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndingTyper : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI textComponent;
    public GameObject buttonGroup;

    [Header("Text Content")]
    [TextArea(5, 10)]
    public string fullText;
    public string cursorChar = "█";

    [Header("Typing Settings")]
    public float typingSpeed = 0.08f;
    public float startDelay = 1.0f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] keySoundArray;
    public AudioClip enterSound;

    // 코루틴 제어용 변수
    private Coroutine typingCoroutine;
    private bool isTypingFinished = false;

    private void Start()
    {
        // 1. 버튼 숨기기
        if (buttonGroup != null)
            buttonGroup.SetActive(false);

        textComponent.text = "";

        // ⭐ [중요] 텍스트 내용을 '가장 먼저' 완성시킵니다.
        // 이 부분이 아래에 있으면 스킵할 때 내용이 비어있게 됩니다.
        if (GameDataManager.Instance != null)
        {
            int score = GameDataManager.Instance.score;
            fullText = $"계약 종료...\n\n\n\n[SYSTEM MESSAGE]\n\n\n사유 : 자산 파손 위험, 작업 수행능력 미달, 계약 미이행.\n\n청구 목록 : 시신 수습 비용, 계약 미이행 환수금\n\n\n정산 후 최종 합계 : {score} - 700 = {score - 700}$\n\n\n귀하의 노고에 진심으로 감사드립니다. \t-한빛 시스템즈-";
        }

        // 2. 텍스트가 완성된 후 코루틴 시작
        typingCoroutine = StartCoroutine(TypeWriterRoutine());
    }

    private void Update()
    {
        // 타이핑이 아직 안 끝났는데 마우스 왼쪽 클릭을 했다면?
        if (!isTypingFinished && Input.GetMouseButtonDown(0))
        {
            SkipTyping();
        }
    }

    // ⭐ 연출 스킵 함수
    void SkipTyping()
    {
        // 1. 진행 중이던 타이핑 멈춤
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        // 2. 상태 완료 처리
        isTypingFinished = true;

        // 3. 텍스트 전체 출력
        textComponent.text = fullText + cursorChar;

        // 4. 버튼 즉시 표시
        if (buttonGroup != null) buttonGroup.SetActive(true);

        // 5. 커서 깜빡임 효과 시작
        StartCoroutine(BlinkCursor());

        // 6. 스킵 효과음 (선택사항)
        if (enterSound != null) audioSource.PlayOneShot(enterSound);
    }

    IEnumerator TypeWriterRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < fullText.Length; i++)
        {
            char c = fullText[i];
            // Substring 대신 StringBuilder 등을 쓰면 더 좋지만 기존 로직 유지
            textComponent.text = fullText.Substring(0, i + 1) + cursorChar;

            if (c != ' ' && c != '\n') PlayRandomKeySound();
            else if (c == '\n' && enterSound != null) audioSource.PlayOneShot(enterSound);

            if (c == '.' || c == '?' || c == '!') yield return new WaitForSeconds(typingSpeed * 3f);
            else yield return new WaitForSeconds(typingSpeed);
        }

        // 정상적으로 타이핑이 다 끝났을 때의 처리
        if (!isTypingFinished)
        {
            isTypingFinished = true;
            StartCoroutine(BlinkCursor());

            yield return new WaitForSeconds(1.0f);
            if (buttonGroup != null)
            {
                buttonGroup.SetActive(true);
                if (enterSound != null) audioSource.PlayOneShot(enterSound);
            }
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

    // 버튼 연결 함수들
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu_Scene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        Debug.Log("게임 종료!");
    }
}