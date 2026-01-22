using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
  [Header("Move")]
    public float walkSpeed = 6f;
    public float runSpeed = 10f;
    public KeyCode runKey = KeyCode.LeftShift;

    public float acceleration = 40f;
    public float deceleration = 50f;

    [Header("Facing")]
    public bool faceByMoveInput = true;
    public bool faceByMouse = false;

    [Header("Z Lock (2.5D)")]
    public bool lockZToStart = true;

    [Header("Stamina")]
    public float maxStamina = 5f;             // 최대 스태미나(초 단위처럼 쓰기 좋음)
    public float staminaDrainPerSec = 1.2f;   // 달릴 때 초당 소모
    public float staminaRegenPerSec = 0.9f;   // 안 달릴 때 초당 회복
    public float minStaminaToRun = 0.2f;      // 이 값보다 낮으면 달리기 불가(깜빡임 방지)
    public float stamina;                     // 현재 스태미나(읽기용)

    [Header("Footsteps (Run Only)")]
    public AudioSource audioSource;
    public AudioClip runFootstepLoop;         // 달릴 때만 재생할 루프 클립
    public float footstepMinSpeed = 0.2f;     // 이 속도 이상일 때만 발소리
    

    Rigidbody rb;

    float moveInput;
    bool facingRight = true;
    float lockedZ;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // 오디오 기본 세팅 (루프 발소리)
        audioSource.playOnAwake = false;
        audioSource.loop = true;

        // 넘어짐 방지
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // 시작 Z 저장
        lockedZ = transform.position.z;

        if (lockZToStart)
            rb.constraints |= RigidbodyConstraints.FreezePositionZ;

        stamina = maxStamina;
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

        HandleStamina();
        HandleRunFootsteps();
    }

    void FixedUpdate()
    {
        bool wantsRun = Input.GetKey(runKey);
        bool canRun = stamina > minStaminaToRun;

        bool isMoving = Mathf.Abs(moveInput) > 0.01f;
        bool isRunning = wantsRun && canRun && isMoving;

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        float targetVelX = moveInput * currentSpeed;
        float diff = targetVelX - rb.velocity.x;

        float rate = Mathf.Abs(targetVelX) > 0.01f ? acceleration : deceleration;

        rb.AddForce(new Vector3(diff * rate, 0f, 0f), ForceMode.Acceleration);

        // 물리 단계에서도 Z 고정(더 단단하게)
        if (lockZToStart)
        {
            Vector3 p = rb.position;
            p.z = lockedZ;
            rb.position = p;
        }
    }

    void HandleStamina()
    {
        bool isMoving = Mathf.Abs(moveInput) > 0.01f;
        bool wantsRun = Input.GetKey(runKey);

        // "달리기 상태" 판정은 FixedUpdate와 동일한 규칙으로 맞춰줌
        bool isRunning = wantsRun && isMoving && stamina > minStaminaToRun;

        if (isRunning)
        {
            stamina -= staminaDrainPerSec * Time.deltaTime;
        }
        else
        {
            stamina += staminaRegenPerSec * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }

    void HandleRunFootsteps()
    {
        // 달릴 때만 발소리: (Shift 누름) + (움직임 있음) + (스태미나 충분) + (실제 속도도 어느 정도)
        bool wantsRun = Input.GetKey(runKey);
        bool isMovingInput = Mathf.Abs(moveInput) > 0.01f;
        bool hasStamina = stamina > minStaminaToRun;

        float speedX = Mathf.Abs(rb.velocity.x);
        bool fastEnough = speedX >= footstepMinSpeed;

        bool shouldPlay = wantsRun && isMovingInput && hasStamina && fastEnough;

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
        if (facingRight == faceRight) return;
        facingRight = faceRight;

        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
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
}
