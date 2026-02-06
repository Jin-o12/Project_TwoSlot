using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("컴포넌트")]
    private AudioSource audioSource;
    private Rigidbody rb;        
    private Animator animator;
    private InputManager inputManager;
    private AimAndFlip aimAndFlip;

    [Header("이동")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float acceleration = 40f;
    public float deceleration = 50f;

    private float moveInput;
    private float lockedZ;

    [Header("상태")]
    private bool isDead;
    private bool isMoving;
    private bool isRunning;
    private bool isBackWalkNow;

    [Header("Footsteps")]
    public AudioClip[] walkFootstepClip;
    public AudioClip[] runFootstepClip;
    public float footstepMinSpeed = 0.2f;
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.4f;

    private float footstepTimer;
    private float minPitch = 0.9f;
    private float maxPitch = 1.1f;

    [Header("Animation")]
    public string speedParam = "Speed";
    public string isRunningParam = "IsRunning";
    public string isBackWalkParam = "IsBackWalk";
    public float animSpeedDamp = 0.1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        aimAndFlip = GetComponent<AimAndFlip>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // ⭐⭐⭐⭐⭐ IK 캐릭터 필수
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        InitializePlayer();
    }

    public void InitializePlayer()
    {
        inputManager = InputManager.Instance;

        isRunning = false;
        isDead = false;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        lockedZ = transform.position.z;
        rb.constraints |= RigidbodyConstraints.FreezePositionZ;
    }

    void Update()
    {
        if (GamePauseManager.Paused || isDead) return;

        ReadyMovement();
        PlayFootStepSound();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (GamePauseManager.Paused || isDead) return;
        Move();
    }

    private void ReadyMovement()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        isMoving = Mathf.Abs(moveInput) > 0.01f;
        isBackWalkNow = false;

        // ⭐ AimAndFlip 방향 기준으로 백워크 판정
        if (aimAndFlip != null && isMoving)
        {
            int moveDir = (moveInput > 0f) ? 1 : -1;
            int faceDir = aimAndFlip.Facing;

            isBackWalkNow = (moveDir != faceDir);
        }

        if (animator)
            animator.SetBool(isBackWalkParam, isBackWalkNow);
    }

    private void PlayFootStepSound()
    {
        if (!isMoving) return;
        if (Mathf.Abs(rb.velocity.x) < footstepMinSpeed) return;

        AudioClip[] clips = isRunning ? runFootstepClip : walkFootstepClip;
        if (clips == null || clips.Length == 0) return;

        footstepTimer -= Time.deltaTime;
        if (footstepTimer > 0f) return;

        footstepTimer = isRunning ? runStepInterval : walkStepInterval;

        int index = Random.Range(0, clips.Length);
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clips[index]);
    }

    private void UpdateAnimator()
    {
        if (!animator) return;

        float vx = Mathf.Abs(rb.velocity.x);
        if (vx < 0.05f) vx = 0f;

        float speed01 = Mathf.Clamp01(vx / runSpeed);

        animator.SetFloat(speedParam, speed01, animSpeedDamp, Time.deltaTime);
    }

    public void SetDead(bool dead)
    {
        isDead = dead;

        if (audioSource && audioSource.isPlaying)
            audioSource.Stop();

        if (animator)
            animator.SetFloat(speedParam, 0f);
    }

    private void Move()
    {
        bool wantsRun = Input.GetKey(inputManager.Run);

        isRunning = wantsRun && isMoving && !isBackWalkNow;

        if (animator)
            animator.SetBool(isRunningParam, isRunning);

        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        float targetVelX = moveInput * currentSpeed;

        float accel = Mathf.Abs(targetVelX) > 0.01f ? acceleration : deceleration;

        float newVelX = Mathf.MoveTowards(
            rb.velocity.x,
            targetVelX,
            accel * Time.fixedDeltaTime
        );

        rb.velocity = new Vector3(newVelX, rb.velocity.y, rb.velocity.z);

        // Z 고정
        Vector3 p = rb.position;
        p.z = lockedZ;
        rb.position = p;
    }
}
