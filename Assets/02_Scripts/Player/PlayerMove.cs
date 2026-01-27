using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Move")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public bool IsRunning = false;
    public InputManager inputManager;
    private KeyCode runKey = KeyCode.LeftShift;

    public float acceleration = 40f;
    public float deceleration = 50f;

    [Header("Facing")]
    public bool faceByMoveInput = true;
    public bool faceByMouse = false;

    [Header("Z Lock (2.5D)")]
    public bool lockZToStart = true;

    [Header("Footsteps (Run Only)")]
    public AudioSource audioSource;
    public AudioClip runFootstepLoop;         // 달릴 때만 재생할 루프 클립
    public float footstepMinSpeed = 0.2f;     // 이 속도 이상일 때만 발소리

    [Header("Animation")]
    public Animator animator;          // 플레이어(혹은 모델 자식)에 있는 Animator
    public string speedParam = "Speed";
    public string isRunningParam = "IsRunning";
    public float animSpeedDamp = 0.1f; // 애니 전환 부드럽게

    Rigidbody rb;

    float moveInput;
    bool facingRight = true;
    float lockedZ;

    void Awake()
    {
        // 필요 컴포넌트
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // 입력키 

        // 오디오 기본 세팅 (루프 발소리)
        audioSource.playOnAwake = false;
        audioSource.loop = true;

        // 넘어짐 방지
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // 시작 Z 저장
        lockedZ = transform.position.z;

        if (lockZToStart)
            rb.constraints |= RigidbodyConstraints.FreezePositionZ;

        // stamina = maxStamina;
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // 방향 처리
        if (faceByMouse)
        {
            Vector3 mouseWorld = GetMouseWorldOnZPlane(lockedZ);
            SetFacing(mouseWorld.x >= transform.position.x);
        }
        else if (faceByMoveInput && moveInput != 0)
        {
            SetFacing(moveInput > 0);
        }

        // Z 강제 고정(안전장치)
        if (lockZToStart)
        {
            Vector3 p = transform.position;
            if (!Mathf.Approximately(p.z, lockedZ))
            {
                p.z = lockedZ;
                transform.position = p;
            }
        }

        // HandleStamina();
        HandleRunFootsteps();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        bool wantsRun = Input.GetKey(runKey);
        // bool canRun = stamina > minStaminaToRun;

        bool isMoving = Mathf.Abs(moveInput) > 0.01f;
        bool isRunning = wantsRun && isMoving;

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        float targetVelX = moveInput * currentSpeed;

        // 가속/감속 선택 (입력 있으면 acceleration, 없으면 deceleration)
        float accel = Mathf.Abs(targetVelX) > 0.01f ? acceleration : deceleration;

        // 목표 속도까지 일정 속도로 접근 (Lerp보다 목표치 도달이 확실함)
        float newVelX = Mathf.MoveTowards(rb.velocity.x, targetVelX, accel * Time.fixedDeltaTime);

        rb.velocity = new Vector3(newVelX, rb.velocity.y, rb.velocity.z);      

        // 물리 단계에서도 Z 고정(더 단단하게)
        if (lockZToStart)
        {
            Vector3 p = rb.position;
            p.z = lockedZ;
            rb.position = p;
        }
    }

    void HandleRunFootsteps()
    {
        // 달릴 때만 발소리: (Shift 누름) + (움직임 있음) + (스태미나 충분) + (실제 속도도 어느 정도)
        if(Input.GetKey(runKey))
        {
            IsRunning = true;
        }

        bool isMovingInput = Mathf.Abs(moveInput) > 0.01f;
        // bool hasStamina = stamina > minStaminaToRun;

        float speedX = Mathf.Abs(rb.velocity.x);
        bool fastEnough = speedX >= footstepMinSpeed;

        bool shouldPlay = IsRunning && isMovingInput && fastEnough;

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

    void SetFacing(bool faceRight)
    {
    facingRight = faceRight;

    // 지금 오브젝트(SkelMesh_Bodyguard_01)를 그냥 회전시켜서 방향 전환
    // 현재 기본 회전이 Y=-90 이므로:
    // 오른쪽 보기 = -90
    // 왼쪽 보기 = +90
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
        // 애니메이터가 존재하지 않을 시 애니메이션 출력 하지 않음
        if (!animator) return;

        // 플레이어의 x축 속도
        float vx = Mathf.Abs(rb.velocity.x);

        // 미세 흔들림 제거
        if (vx < 0.05f) vx = 0f;

        // 뛰는 속도에 비례해 현재 속도가 어느 정도인지 계산
        float speed01 = Mathf.Clamp01(vx / runSpeed);
        animator.SetFloat(speedParam, speed01, animSpeedDamp, Time.deltaTime);
    }
}
