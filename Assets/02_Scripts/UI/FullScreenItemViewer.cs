using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))] // AudioSource 컴포넌트가 없으면 자동으로 추가
public class FullScreenItemViewer : MonoBehaviour
{
    [Header("설정")]
    public Sprite documentSprite;

    [Header("UI 연결")]
    public GameObject fullScreenCanvas;
    public Image documentImageComponent;

    // F키 안내 UI (Hierarchy에 있는 오브젝트 연결)
    public GameObject interactionPromptUI;

    [Header("Sound")]
    // [추가] 문서를 열 때 재생할 소리 (종이 넘기는 소리 등)
    public AudioClip openDocumentClip;
    private AudioSource audioSource;

    private bool isPlayerNearby = false;
    private bool isShowing = false;

    void Awake()
    {
        // [추가] AudioSource 컴포넌트 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // 2D 사운드로 설정 (거리 상관없이 잘 들리게)
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (fullScreenCanvas != null) fullScreenCanvas.SetActive(false);

        // 시작할 때 안내 UI가 켜져있다면 끔
        if (interactionPromptUI != null) interactionPromptUI.SetActive(false);
    }

    void Update()
    {
        if (isShowing)
        {
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Escape))
            {
                CloseDocument();
            }
        }
        else if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            if (GamePauseManager.Instance != null && !GamePauseManager.Instance.IsPaused)
            {
                OpenDocument();
            }
        }
    }

    void OpenDocument()
    {
        isShowing = true;

        if (documentImageComponent != null) documentImageComponent.sprite = documentSprite;
        if (fullScreenCanvas != null) fullScreenCanvas.SetActive(true);

        // 문서를 보는 동안에는 'F키 안내'를 숨김 (가리니까)
        if (interactionPromptUI != null) interactionPromptUI.SetActive(false);

        // [추가] 효과음 재생
        if (audioSource != null && openDocumentClip != null)
        {
            // Time.timeScale = 0 상태에서도 소리가 들려야 하므로 PlayOneShot 사용
            // (AudioListener.pause가 true라면 안 들릴 수 있으니 주의)
            audioSource.PlayOneShot(openDocumentClip);
        }

        // 시간 정지 등...
        Time.timeScale = 0f;

        // PauseManager에 알림
        if (GamePauseManager.Instance != null) GamePauseManager.Instance.IsExternalPaused = true;
    }

    void CloseDocument()
    {
        isShowing = false;

        if (fullScreenCanvas != null) fullScreenCanvas.SetActive(false);

        // 문서를 닫았는데, 여전히 제자리에 서있다면 다시 안내를 띄움
        if (isPlayerNearby && interactionPromptUI != null)
            interactionPromptUI.SetActive(true);

        Time.timeScale = 1f;

        if (GamePauseManager.Instance != null) GamePauseManager.Instance.IsExternalPaused = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // 구역에 들어오면 안내 UI 켜기
            if (interactionPromptUI != null) interactionPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            // 구역에서 나가면 안내 UI 끄기
            if (interactionPromptUI != null) interactionPromptUI.SetActive(false);

            if (isShowing) CloseDocument();
        }
    }
}