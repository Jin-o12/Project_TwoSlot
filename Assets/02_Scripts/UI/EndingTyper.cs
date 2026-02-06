using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // �� �̵��� ���� �ʼ�!

public class EndingTyper : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI textComponent;
    public GameObject buttonGroup; // ��ư���� ������� �θ� ������Ʈ

    [Header("Text Content")]
    [TextArea(5, 10)]
    public string fullText;
    public string cursorChar = "��";

    [Header("Typing Settings")]
    public float typingSpeed = 0.08f;
    public float startDelay = 1.0f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] keySoundArray;
    public AudioClip enterSound;

    private void Start()
    {
        // 1. �������ڸ��� ��ư �����
        if (buttonGroup != null)
            buttonGroup.SetActive(false);

        textComponent.text = "";
        StartCoroutine(TypeWriterRoutine());

        int score = GameDataManager.Instance.score;
        fullText = $"계약 종료...\n\n\n\n[SYSTEM MESSAGE]\n\n\n사유 : 자산 파손 위험, 작업 수행능력 미달, 계약 미이행.\n\n청구 목록 : 시신 수습 비용, 계약 미이행 환수금\n\n\n정산 후 최종 합계 : {score} - 700 = {score-700}$\n\n\n귀하의 노고에 진심으로 감사드립니다. 	-한빛 시스템즈-";
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

        // Ÿ���� ��
        StartCoroutine(BlinkCursor());

        // 2. 1�� �ڿ� ��ư �׷� �ѱ�
        yield return new WaitForSeconds(1.0f);
        if (buttonGroup != null)
        {
            buttonGroup.SetActive(true);
            // (���û���) ��ư ���� �� �Ҹ� �ϳ� �־��ָ� ����
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

    // ��ư ��� �Լ� �߰� (�ν����Ϳ��� �����)

    public void GoToMainMenu()
    {
        // "MainMenu" �κп� ���� ���� �޴� �� �̸� ����
        SceneManager.LoadScene("MainMenu_Scene");
    }

    public void QuitGame()
    {
        // 1. 유니티 에디터에서 실행 중일 때
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            
        // 2. 실제 빌드된 파일에서 실행 중일 때
        #else
            Application.Quit();
        #endif

        Debug.Log("���� ����!");
    }
}