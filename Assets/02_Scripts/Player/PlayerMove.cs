/// <summary>
/// 플레이어의 물리 기반 움직임을 구현하고 애니메이션을 재생합니다.
/// </summary>
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("컴포넌트&인스턴스")]
    private AudioSource audioSource;
    private Rigidbody rb;
    private Animator animator;
    private InputManager inputManager;              // 조작 키를 가지고 있는 인스턴스 스크립트
    private AimAndFlip aimAndFlip;                  // 캐릭터를 뒤집는 스크립트

    [Header("이동")]
    public float walkSpeed = 5f;                    // 걷는 속도
    public float runSpeed = 10f;                    // 뛰는 속도

    public float acceleration = 40f;                // 가속 정도 수치
    public float deceleration = 50f;                // 감속 정도 수치

    public float moveInput;                         // Horizontal 이동 키보드 입력
    public float lockedZ;                           // Z축 고정

    [Header("상태 bool 변수")]
    private bool isDead;                            // 죽었다면 참
    private bool isMoving;                          // 움직이고 있다면 참
    private bool isRunning;                         // 뛰고 있다면 참
    private bool isBackWalkNow;                     // 뒷걸음질 치고 있다면 참

    [Header("Facing")]
    private bool faceByMoveInput;
    private bool faceByMouse;

    [Header("Footsteps")]
    public AudioClip[] walkFootstepClip;            // 걸을 때 발소리 사운드 클립
    public AudioClip[] runFootstepClip;             // 달릴 때 발소리 사운드 클립

    // ▼▼▼ [추가된 부분] 볼륨 조절 변수 ▼▼▼
    [Range(0f, 1f)] public float walkVolume = 0.5f; // 걷기 볼륨 (0~1)
    [Range(0f, 1f)] public float runVolume = 1.0f;  // 달리기 볼륨 (0~1)
    // ▲▲▲ [추가된 부분] ▲▲▲

    public float footstepMinSpeed = 0.2f;           // 이 속도 이상일 때만 발소리
    private float minPitch;                         // 다양한 크기의 발소리 연출을 위한 최소 피치
    private float maxPitch;                         // 사운드 클립의 최대 피치
    public float walkStepInterval = 0.5f;           // 걷는 발소리 재생 간격
    public float runStepInterval = 0.4f;            // 뛰는 발소리 재생 간격
    private float footstepTimer = 0f;               // 발소리 재생 간격을 재는 타이머

    [Header("Animation")]
    public string speedParam = "Speed";             // 속도 파라미터 이름
    public string isRunningParam = "IsRunning";     // 달리기 확인 파라미터 이름
    public string isBackWalkParam = "IsBackWalk";   // 뒤로 가기 확인 파라미터 이름
    public float animSpeedDamp = 0.1f;              // 애니메이션 전환을 부드럽게하기 위해 값을 점진적으로 증가시키는 정도


    void Awake()
    {
        // 필요 컴포넌트
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!aimAndFlip) aimAndFlip = GetComponent<AimAndFlip>();

        // 오디오 기본 세팅
        audioSource.playOnAwake = false;
        // PlayOneShot으로 재생하므로 루프는 사용하지 않음
        audioSource.loop = false;
        minPitch = 0.9f;
        maxPitch = 1.1f;
    }

    void Start()
    {
        InitializePlayer();
    }

    /* 플레이어 생성 시 모든 상태 초기화 */
    public void InitializePlayer()
    {
        inputManager = InputManager.Instance;                        // 키 입력 싱글톤 초기화

        isRunning = false;                                           // 상태 변수 초기화
        isDead = false;
        faceByMoveInput = true;
        faceByMouse = false;

        rb.constraints = RigidbodyConstraints.FreezeRotation;        // 넘어짐 방지 회전 고정

        lockedZ = transform.position.z;                              // 시작 Z 저장

        rb.constraints |= RigidbodyConstraints.FreezePositionZ;      // 시작 시 z축 고정
    }

    void Update()
    {
        if (PauseState.IsPaused || isDead) return;   // 게임 일시정지 상태이거나 죽었을 시 입력 무시

        ReadyMovement();
        PlayFootStepSound();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (PauseState.IsPaused || isDead) return;

        GetMove();
    }

    /* FixedUpdated에서 움직임을 수행하기 전 움직임에 대한 확인 및 준비를 수행 */
    private void ReadyMovement()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        isMoving = Mathf.Abs(moveInput) > 0.01f;
        isBackWalkNow = false;

        if (aimAndFlip != null && isMoving)
        {
            int moveDir = (moveInput > 0f) ? 1 : -1;
            int faceDir = aimAndFlip.Facing;
            // 움직이는 방향과 마우스 방향이 다르면 뒷걸음 판정
            isBackWalkNow = (moveDir != faceDir);
        }

        // 방향 처리
        if (faceByMouse)
        {
            Vector3 mouseWorld = GetMouseWorldOnZPlane(lockedZ);
            SetFacing(mouseWorld.x >= transform.position.x);
        }
        else if (faceByMoveInput && moveInput != 0)
        {
            if (!isBackWalkNow)
                SetFacing(moveInput > 0);
        }

        // 애니메이션
        if (animator) animator.SetBool(isBackWalkParam, isBackWalkNow);
    }

    /* 상태를 받아 해당하는 소리를 재생 */
    private void PlayFootStepSound()
    {
        // 이동 중이 아니면 소리 재생하지 않음
        if (!isMoving) return;

        // 속도가 충분히 빠를 때만 발소리 재생
        if (rb == null || Mathf.Abs(rb.velocity.x) < footstepMinSpeed) return;

        // 달림 여부에 따라 재생할 클립 배열 선택
        AudioClip[] clips = isRunning ? runFootstepClip : walkFootstepClip;

        // ▼▼▼ [수정된 부분] 상태에 따른 볼륨 선택 ▼▼▼
        float currentVolume = isRunning ? runVolume : walkVolume;

        // null 또는 비었을 시 리턴
        if (clips == null || clips.Length == 0) return;

        // 타이머로 프레임마다 재생되는 것을 방지
        footstepTimer -= Time.deltaTime;
        if (footstepTimer > 0f) return;

        // 현재 속도에 맞는 간격 설정
        footstepTimer = isRunning ? runStepInterval : walkStepInterval;

        // 랜덤한 클립 선택 및 피치 조절
        int index = Random.Range(0, clips.Length);
        audioSource.pitch = Random.Range(minPitch, maxPitch);

        // ▼▼▼ [수정된 부분] PlayOneShot에 볼륨 값 전달 ▼▼▼
        audioSource.PlayOneShot(clips[index], currentVolume);
    }

    void SetFacing(bool faceRight)
    {
        // 지금 오브젝트(SkelMesh_Bodyguard_01)를 그냥 회전시켜서 방향 전환
        // 현재 기본 회전이 Y=-90 이므로: 오른쪽 보기 = -90 / 왼쪽 보기 = +90
        float y = faceRight ? 90f : -90f;

        Vector3 e = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(e.x, y, e.z);
    }

    Vector3 GetMouseWorldOnZPlane(float zPlane)
    {
        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, zPlane));
        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return transform.position;
    }

    void UpdateAnimator()
    {
        if (!animator) return;

        // 플레이어의 x축 속도
        float vx = Mathf.Abs(rb.velocity.x);

        // 미세 흔들림 제거
        if (vx < 0.05f) vx = 0f;

        // 뛰는 속도에 비례해 현재 속도가 어느 정도인지 계산
        float speed01 = Mathf.Clamp01(vx / runSpeed);
        animator.SetFloat(speedParam, speed01, animSpeedDamp, Time.deltaTime);
    }

    public void SetDead(bool dead)
    {
        isDead = dead;

        // 죽을 때 발소리 끄기
        if (audioSource && audioSource.isPlaying) audioSource.Stop();

        // 죽으면 애니 파라미터가 Walk로 끌어올리는 걸 방지하려면
        // (선택) Speed를 0으로 한번 고정
        if (animator) animator.SetFloat(speedParam, 0f);
    }

    /* 직접적인 움직임 실행 */
    private void GetMove()
    {
        bool wantsRun = Input.GetKey(inputManager.Run);
        // bool canRun = stamina > minStaminaToRun;

        // 달리려고 하고, 움직이고 있으며 뒤로가지 않는다면 달림
        isRunning = wantsRun && isMoving && !isBackWalkNow;
        // 저장형 상태로 반영하여 다른 메서드에서 사용하게 함
        if (animator) animator.SetBool(isRunningParam, isRunning);

        // 달리고 있는지 아닌지에 따라 플레이어의 현재 속도 설정
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        float targetVelX = moveInput * currentSpeed;

        // 가속/감속 선택 (입력 있으면 acceleration, 없으면 deceleration)
        float accel = Mathf.Abs(targetVelX) > 0.01f ? acceleration : deceleration;

        // 목표 속도까지 일정 속도로 접근 (Lerp보다 목표치 도달이 확실함)
        float newVelX = Mathf.MoveTowards(rb.velocity.x, targetVelX, accel * Time.fixedDeltaTime);

        rb.velocity = new Vector3(newVelX, rb.velocity.y, rb.velocity.z);

        // 물리 단계에서도 Z 고정(더 단단하게)
        Vector3 p = rb.position;
        p.z = lockedZ;
        rb.position = p;
    }
}