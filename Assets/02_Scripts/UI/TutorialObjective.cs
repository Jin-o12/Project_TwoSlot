using UnityEngine;
using TMPro; // TextMeshPro 사용 시 필수
using System.Collections;

public class TutorialObjective : MonoBehaviour
{
    [Header("오디오 연결")]
    public AudioSource audioSource;
    public AudioClip alertClip;
    [Header("UI 연결")]
    public TextMeshProUGUI objectiveText; // 여기에 만든 텍스트 UI 연결

    [Header("설정")]
    public string message = "[목표 갱신] 주요 자산 확보 완료.\n지금 즉시 탈출하십시오.";
    public float startDelay = 1.0f;    // 시작 후 몇 초 뒤에 뜰지
    public float displayTime = 5.0f;   // 몇 초 동안 보여줄지
    public float fadeSpeed = 2.0f;     // 페이드 속도

    void Start()
    {
        if (objectiveText != null)
        {
            // 1. 시작할 때 텍스트 내용을 설정하고 투명하게 만듦
            objectiveText.text = message;
            objectiveText.alpha = 0f;

            // 2. 연출 코루틴 시작
            StartCoroutine(ShowObjectiveRoutine());
            
        }
    }

    IEnumerator ShowObjectiveRoutine()
    {
        // A. 시작 대기
        yield return new WaitForSeconds(startDelay);
        audioSource.PlayOneShot(alertClip);
        // B. 페이드 인 (서서히 나타남)
        while (objectiveText.alpha < 1.0f)
        {
            objectiveText.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        

        // C. 보여주는 시간 유지
        yield return new WaitForSeconds(displayTime);

        // D. 페이드 아웃 (서서히 사라짐)
        while (objectiveText.alpha > 0f)
        {
            objectiveText.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        // (선택 사항) 다 끝나면 오브젝트를 꺼버려서 최적화
        // objectiveText.gameObject.SetActive(false);
    }
}