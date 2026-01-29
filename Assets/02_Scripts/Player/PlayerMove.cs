using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Move")]
    private Rigidbody playerRb;                     // Player Rigidbody 컴포넌트
    public InputManager inputKey;                   // 조작 키를 불러오는 인스턴스 (없을 수 있음)

    public float walkSpeed = 5f;                    // 걷는 속도
    public float runSpeed = 10f;                    // 뛰는 속도
    public bool IsRunning = false;                  // 뛰고 있는지 여부
    public bool lockZToStart = true;                // 고정될 Z축

    public float acceleration = 40f;                // 이동시 가속 수치
    public float deceleration = 50f;                // 정지시 감속 수치

    [Header("Facing")]
    public bool faceByMoveInput = true;
    public bool faceByMouse = false;
    public bool faceRight = false;

    [Header("Footsteps (Run Only)")]
    public AudioSource audioSource;                 // 발소리를 재생 할 Audio Source
    public AudioClip runFootstepLoop;               // 달릴 때만 재생할 루프 클립
    public float footstepMinSpeed = 0.2f;           // 이 속도 이상일 때만 발소리

    [Header("Animation")]
    public Animator animator;                       // 플레이어(혹은 모델 자식)에 있는 Animator
    public string speedParam = "Speed";             // 애니메이터의 속도 파라미터
    public float animSpeedDamp = 0.1f;              // 애니 전환 부드럽게

    [Header("Fallback Keys (when no InputManager in scene)")]
    [SerializeField] private KeyCode fallbackRunKey = KeyCode.LeftShift;

    float moveInput;
    bool facingRight = true;
    float lockedZ;

    void Awake()
    {
        // 필요 컴포넌트
        if (!playerRb) playerRb = GetComponent<Rigidbody>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        // InputManager는 "없을 수 있음" (fallback 사용)
        inputKey = InputManager.Instance;

        // 오디오 기본 세팅 (루프 발소리)
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
        }

        // 넘어짐 방지
        if (playerRb != null)
            playerRb.constraints = RigidbodyConstraints.FreezeRotation;

        // 시작 Z 저장 및 rigibdoby 고정
        lockedZ = transform.position.z;
        if (lockZToStart && playerRb != null)
            playerRb.constraints |= RigidbodyConstraints.FreezePositionZ;
    }

    void Update()
    {
        IsMove();

        if (faceByMouse)
            LookAtMouseDir();

        PlayFootstepSound();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        GetMove();
    }

    /* 플레이어의 움직임 입력과 처리 */
    private void IsMove()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // Z 강제 고정(안전장치)
        if (lockZToStart)
        {
            Vector3 pos = transform.position;
            if (!Mathf.Approximately(pos.z, lockedZ))
            {
                pos.z = lockedZ;
                transform.position = pos;
            }
        }
    }

    /* 플레이어 시선의 방향 처리: 마우스 위치 기반 */
    private void LookAtMouseDir()
    {
        Vector3 mPos = Input.mousePosition;
        bool isMouseInScreen = mPos.x >= 0 && mPos.x <= Screen.width &&
                               mPos.y >= 0 && mPos.y <= Screen.height;

        if (isMouseInScreen)
        {
            Vector3 mouseWorld = GetMouseWorldOnZPlane(lockedZ);
            SetFacing(mouseWorld.x >= transform.position.x);
        }
    }

    /* 마우스의 위치에 따라 플레이어의 방향 회전 */
    void SetFacing(bool faceRight)
    {
        facingRight = faceRight;

        // 현재 기본 회전이 Y=-90 기준인 경우: 오른쪽/왼쪽 값은 프로젝트에 맞게 조정
        float y = faceRight ? 90f : -90f;

        Vector3 angle = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(angle.x, y, angle.z);
    }

    // ★ InputManager 없을 때 fallback 키로 달리기 입력 처리
    bool GetRunKey()
    {
        // 혹시 런타임 중 InputManager가 생성될 수도 있으니 매번 재연결 시도
        if (inputKey == null) inputKey = InputManager.Instance;

        return (inputKey != null) ? Input.GetKey(inputKey.Run)
                                  : Input.GetKey(fallbackRunKey);
    }

    /* 움직임에 따른 발소리 재생 */
    void PlayFootstepSound()
    {
        if (playerRb == null || audioSource == null) return;

        bool wantsRun = GetRunKey();
        bool isMovingInput = Mathf.Abs(moveInput) > 0.01f;

        // ★ 매 프레임 갱신 (안 그러면 true가 남아버림)
        IsRunning = wantsRun && isMovingInput;

        float speedX = Mathf.Abs(playerRb.velocity.x);
        bool fastEnough = speedX >= footstepMinSpeed;

        bool shouldPlay = IsRunning && fastEnough;

        if (shouldPlay)
        {
            if (runFootstepLoop != null)
            {
                if (audioSource.clip != runFootstepLoop) audioSource.clip = runFootstepLoop;
                if (!audioSource.isPlaying) audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying) audioSource.Stop();
        }
    }

    Vector3 GetMouseWorldOnZPlane(float zPlane)
    {
        Camera cam = Camera.main;
        if (cam == null) return transform.position; // ★ MainCamera 없을 때 방어

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, zPlane));
        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return transform.position;
    }

    /* 애니메이션 재생 */
    private void UpdateAnimator()
    {
        if (!animator || playerRb == null) return;

        float vx = playerRb.velocity.x;
        if (Mathf.Abs(vx) < 0.05f) vx = 0f;

        float faceDir = facingRight ? 1f : -1f;
        float relativeSpeed = (vx / runSpeed) * faceDir;
        relativeSpeed = Mathf.Clamp(relativeSpeed, -1f, 1f);

        animator.SetFloat(speedParam, relativeSpeed, animSpeedDamp, Time.deltaTime);
    }

    private void GetMove()
    {
        if (playerRb == null) return;

        bool wantsRun = GetRunKey();
        bool isMoving = Mathf.Abs(moveInput) > 0.01f;
        bool isRunning = wantsRun && isMoving;

        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        float targetVelX = moveInput * currentSpeed;

        float accel = Mathf.Abs(targetVelX) > 0.01f ? acceleration : deceleration;
        float newVelX = Mathf.MoveTowards(playerRb.velocity.x, targetVelX, accel * Time.fixedDeltaTime);

        playerRb.velocity = new Vector3(newVelX, playerRb.velocity.y, playerRb.velocity.z);

        if (lockZToStart)
        {
            Vector3 p = playerRb.position;
            p.z = lockedZ;
            playerRb.position = p;
        }
    }
}
