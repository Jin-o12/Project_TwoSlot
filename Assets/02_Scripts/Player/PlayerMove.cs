using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Move")]
    private Rigidbody playerRb;                     // Player Rigubbody 컴포넌트
    public InputManager inputKey;                   // 조작 키를 불러오는 인스턴스

    public float walkSpeed = 5f;                    // 걷는 속도
    public float runSpeed = 10f;                    // 뛰는 속도
    public bool IsRunning = false;                  // 뛰고 있는지에 대한 여부
    public bool lockZToStart = true;                // 고정될 Z축

    public float acceleration = 40f;                // 이동시 가속 수치
    public float deceleration = 50f;                // 정지시 감속 수치

    [Header("Facing")]
    public bool faceByMoveInput = true;             // 
    public bool faceByMouse = false;                // 마우스 방향을 바라 보고 있는지에 대한 여부
    public bool faceRight = false;                  // 현재 바라보고 있는 방향이 오른쪽인지에 대한 여부

    [Header("Footsteps (Run Only)")]
    public AudioSource audioSource;                 // 발소리를 재생 할 Audio Source
    public AudioClip runFootstepLoop;               // 달릴 때만 재생할 루프 클립
    public float footstepMinSpeed = 0.2f;           // 이 속도 이상일 때만 발소리

    [Header("Animation")]
    public Animator animator;                       // 플레이어(혹은 모델 자식)에 있는 Animator
    public string speedParam = "Speed";             // 애니메이터의 속도 파라미터
    //public string isRunningParam = "IsRunning";   // 애니메이터의 달리기 여부 파라이터
    public float animSpeedDamp = 0.1f;              // 애니 전환 부드럽게

    float moveInput;
    bool facingRight = true;
    float lockedZ;

    void Awake()
    {
        // 필요 컴포넌트
        if (!playerRb)      playerRb = GetComponent<Rigidbody>();
        if (!audioSource)   audioSource = GetComponent<AudioSource>();
        if (!animator)      animator = GetComponentInChildren<Animator>();

        /// 조작 키들을 모두 InputManager에 이동, 싱글톤 생성하여 일괄적인 편집이 용이하게 하였습니다.
        /// inputKey에 인스턴스를 저장하였기 때문에, 'inputKey.변수명'의 형태로 호출할 수 있고 변수명은 /02.Script/System/InputSystem/에서 참고 하실 수 있습니다.
        inputKey = InputManager.Instance;

        // 오디오 기본 세팅 (루프 발소리)
        audioSource.playOnAwake = false;
        audioSource.loop = true;

        // 넘어짐 방지
        playerRb.constraints = RigidbodyConstraints.FreezeRotation;

        // 시작 Z 저장 및 rigibdoby 고정
        lockedZ = transform.position.z;
        if (lockZToStart)
            playerRb.constraints |= RigidbodyConstraints.FreezePositionZ;
    }

    void Update()
    {
        IsMove();
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

    /* 플레이어 시선의 방향 처리: 마우스의 좌표가 플레이어를 기준으로 어느 방향에 있는지 찾아 회전 함수 호출 */
    private void LookAtMouseDir()
    {
        // 마우스가 스크린 내에 존재하는지와 어디 존재하는지에 대해 기록
        Vector3 mPos = Input.mousePosition;
        bool isMouseInScreen = mPos.x >= 0 && mPos.x <= Screen.width &&
                            mPos.y >= 0 && mPos.y <= Screen.height;

        // 2. 마우스 위치에 의해서 SetFacing을 호출함
        if (isMouseInScreen)
        {
            Vector3 mouseWorld = GetMouseWorldOnZPlane(lockedZ);
            
            // 마우스가 플레이어보다 오른쪽에 있으면 true, 왼쪽에 있으면 false 전달
            SetFacing(mouseWorld.x >= transform.position.x);
        }
    }

    /* 마우스의 위치에 따라 플레이어의 방향 회전 */
    void SetFacing(bool faceRight)
    {
        facingRight = faceRight;

        // 지금 오브젝트(SkelMesh_Bodyguard_01)를 그냥 회전시켜서 방향 전환
        // 현재 기본 회전이 Y=-90 이므로: 오른쪽 보기 = -90/ 왼쪽 보기 = +90
        float y = faceRight ? 90f : -90f;

        Vector3 angle = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(angle.x, y, angle.z);
    }

    /* 움직임에 따른 발소리 재생 */
    void PlayFootstepSound()
    {
        /// 이후 음원 파일 추가 후 걸을 때에도 발소리 재생되게 바꿀 것
        
        // 달릴 때만 발소리: (Shift 누름) + (움직임 있음) + (스태미나 충분) + (실제 속도도 어느 정도)
        if(Input.GetKey(inputKey.Run))
        {
            IsRunning = true;
        }

        bool isMovingInput = Mathf.Abs(moveInput) > 0.01f;
        // bool hasStamina = stamina > minStaminaToRun;

        float speedX = Mathf.Abs(playerRb.velocity.x);
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

    Vector3 GetMouseWorldOnZPlane(float zPlane)
    {
        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, zPlane));
        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return transform.position;
    }

    /* 애니메이션 재생 */
    private void UpdateAnimator()
    {
        if (!animator) return;

        // 플레이어의 x축 속도
        float vx = playerRb.velocity.x;

        // 미세 흔들림 제거
        if (Mathf.Abs(vx) < 0.05f) vx = 0f;

        // 바라보는 방향 판정 (왼쪽: -1, 오른쪽: 1)
        float faceDir = facingRight ? 1f : -1f;

        // 뛰는 속도에 비례해 현재 속도가 어느 정도인지 계산하고, 방향을 곱하여 지정
        float relativeSpeed = (vx / runSpeed) * faceDir;

        // 값 제한
        relativeSpeed = Mathf.Clamp(relativeSpeed, -1f, 1f);

        animator.SetFloat(speedParam, relativeSpeed, animSpeedDamp, Time.deltaTime);
    }

    private void GetMove()
    {
        bool wantsRun = Input.GetKey(inputKey.Run);
        // bool canRun = stamina > minStaminaToRun;

        bool isMoving = Mathf.Abs(moveInput) > 0.01f;
        bool isRunning = wantsRun && isMoving;

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        float targetVelX = moveInput * currentSpeed;

        // 가속/감속 선택 (입력 있으면 acceleration, 없으면 deceleration)
        float accel = Mathf.Abs(targetVelX) > 0.01f ? acceleration : deceleration;

        // 목표 속도까지 일정 속도로 접근 (Lerp보다 목표치 도달이 확실함)
        float newVelX = Mathf.MoveTowards(playerRb.velocity.x, targetVelX, accel * Time.fixedDeltaTime);

        playerRb.velocity = new Vector3(newVelX, playerRb.velocity.y, playerRb.velocity.z);      

        // 물리 단계에서도 Z 고정(더 단단하게)
        if (lockZToStart)
        {
            Vector3 p = playerRb.position;
            p.z = lockedZ;
            playerRb.position = p;
        }
    }
}
