using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCtrl : MonoBehaviour
{
    public NavMeshAgent navi;

    [Header("데미지")]
    public int damage = 25;

    [Header("적 추적 범위와 공격 범위")]
    public float traceDist = 7f;
    public float attackDist = 0.5f; // stoppingDistance 참고용

    [Header("추적 사운드")]
    public AudioSource chaseAudio;          // 추적 사운드 재생용 AudioSource
    public AudioClip chaseLoopClip;         // 추적 중 반복 재생할 클립
    public bool chaseLoop = true;           // 보통 true 추천(반복)
    public float chaseVolume = 1f;          // 볼륨
    private bool wasTracing = false;        // ✅ 추적 진입/이탈 감지 플래그

    [Header("추적 가속(부드럽게 빨라지기)")]
    public float startTraceSpeed = 1.2f;   // 추적 시작 속도
    public float chaseSpeed = 3.5f;        // 추적 최고 속도
    public float startTraceAccel = 4f;     // 시작 가속
    public float chaseAccel = 20f;         // 추적 가속
    public float rampTime = 0.7f;          // 몇 초에 걸쳐 빨라질지
    private Coroutine traceRampCo;

    [Header("Z축 고정")]
    public float lockZ = 9f;
    public bool lockZAxis = true;

    public Transform enemyTr;
    public Transform playerTr;

    [Header("공격 쿨타임")]
    public float attackCooldown = 2.0f;
    private float nextAttackTime = 0f;

    [Header("공격 애니 보이게 잠깐 멈춤")]
    public float attackLockTime = 1.2f;
    private bool isAttacking = false;

    [Header("공격 히트박스")]
    public BoxCollider attackBox;
    public float hitboxOnTime = 0.2f;

    [Header("플레이어 레이어(필수)")]
    public LayerMask playerLayer;

    [Header("리코일(공격시 살짝 넉백 넣기)")]
    public float recoilDistance = 2.0f;
    public float recoilDuration = 0.25f;
    private bool isRecoiling = false;

    [Header("패트롤(좌우 왕복)")]
    public bool usePatrol = true;
    public float patrolRange = 2.5f;       // 시작 위치 기준 좌/우 거리
    public float patrolSpeed = 2.5f;       // 패트롤 이동 속도
    public float patrolAccel = 40f;        // 패트롤 가속(느림 해결)
    public float patrolWaitTime = 0.2f;    // 끝점에서 잠깐 멈춤
    public float patrolArriveEps = 0.15f;  // 도착 판정 오차

    [Header("방향(뒤집힘 보정)")]
    public bool invertFacing = false;      // 모델 방향이 반대면 true

    [Header("히트(피격)")]
    public float hitStunTime = 0.35f;      // 피격 경직 시간
    private bool isHitted = false;         // 피격 중 플래그
    private Coroutine hitCo;

    private Vector3 patrolA;
    private Vector3 patrolB;
    private Vector3 patrolTarget;
    private bool patrolInited = false;
    private float patrolNextSwitchTime = 0f;

    [Header("디버그")]
    public bool debugLog = false;

    private Animator animator;
    private bool hitAppliedThisSwing = false;

    // Animator hashes
    static readonly int HashTrace  = Animator.StringToHash("Trace");
    static readonly int HashPatrol = Animator.StringToHash("Patrol");
    static readonly int HashAttack = Animator.StringToHash("Attack");
    static readonly int HashHit    = Animator.StringToHash("Hit");   // Trigger
    static readonly int HashDie    = Animator.StringToHash("Die");   // (있으면 사용, 없어도 문제 없음)

    void Start()
    {
        navi = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        enemyTr = transform;
        playerTr = GameObject.FindWithTag("Player")?.transform;

        lockZ = transform.position.z;

        InitNavAgent();
        InitAttackHitbox();
        InitChaseAudio();
        InitPatrolPoints();
    }

    void Update()
    {
        if (!CanUpdate()) return;

        // ✅ 피격 중이면 최우선으로 행동 중단
        if (isHitted)
        {
            StopTracing(true);   // ✅ 핵심: wasTracing=false까지 포함
            if (lockZAxis) FixZ();
            return;
        }

        // ✅ 공격/리코일 중에도 추적/패트롤 중단
        if (HandleBusyState())
            return;

        Vector3 playerPos = GetPlayerPosLockedZ();
        float distanceX = Mathf.Abs(enemyTr.position.x - playerPos.x);

        bool inAttackRange = IsPlayerInAttackBox();
        bool tracingNow;

        if (inAttackRange)
        {
            HandleAttackState(playerPos);
            tracingNow = false;
        }
        else if (distanceX < traceDist)
        {
            HandleTraceState(playerPos);
            tracingNow = true;
        }
        else
        {
            HandlePatrolOrIdleState();
            tracingNow = false;
        }

        UpdateChaseAudio(tracingNow);

        if (lockZAxis) FixZ();
    }

    // =======================
    // ✅ 외부에서 호출할 피격 함수(복붙용)
    // =======================
    public void PlayHit(float stunTime = -1f)
    {
        if (animator == null) return;
        if (hitCo != null) StopCoroutine(hitCo);
        hitCo = StartCoroutine(HitRoutine(stunTime));
    }

    IEnumerator HitRoutine(float stunTime = -1f)
    {
        isHitted = true;

        // ✅ 피격 진입 시 “추적 상태”를 강제로 끊어줌 (가속 램프 재시작 보장)
        StopTracing(true);

        // ✅ 트리거 꼬임 정리: 공격이 남아있으면 Hit 끝나고 Attack으로 튈 수 있음
        animator.ResetTrigger(HashAttack);
        animator.ResetTrigger(HashHit);   // ✅ 연속 피격 시 트리거 꼬임 방지(추천)
        // animator.ResetTrigger(HashDie); // ✅ Die 트리거도 쓰고 있다면(선택)

        animator.SetTrigger(HashHit);

        float t = (stunTime >= 0f) ? stunTime : hitStunTime;
        yield return new WaitForSeconds(t);

        isHitted = false;
        hitCo = null;

        // ✅ 다음 추적 진입을 진입으로 인식시키기
        wasTracing = false;

        // ✅ 피격 후 속도 튐 방지(기본값으로)
        ResetTraceMove();
    }

    // =======================
    // Init
    // =======================
    void InitNavAgent()
    {
        if (navi == null) return;

        navi.updateRotation = false;
        navi.updateUpAxis = false;
        navi.stoppingDistance = attackDist;

        navi.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        navi.radius = 0.15f;
        navi.autoBraking = false;

        navi.speed = startTraceSpeed;
        navi.acceleration = startTraceAccel;
    }

    void InitAttackHitbox()
    {
        if (attackBox != null)
            attackBox.enabled = false;
    }

    void InitChaseAudio()
    {
        if (chaseAudio == null)
            chaseAudio = GetComponent<AudioSource>();

        if (chaseAudio != null)
        {
            chaseAudio.playOnAwake = false;
            chaseAudio.loop = chaseLoop;
            chaseAudio.volume = chaseVolume;
        }
    }

    void InitPatrolPoints()
    {
        if (!usePatrol) return;

        Vector3 p = transform.position;
        if (lockZAxis) p.z = lockZ;

        patrolA = p; patrolA.x -= patrolRange;
        patrolB = p; patrolB.x += patrolRange;

        patrolTarget = patrolB;
        patrolInited = true;
        patrolNextSwitchTime = 0f;
    }

    // =======================
    // Update helpers
    // =======================
    bool CanUpdate()
    {
        if (animator == null) return false;
        if (navi == null) return false;
        if (!navi.enabled || !navi.isOnNavMesh) return false;
        if (playerTr == null) return false;
        return true;
    }

    bool HandleBusyState()
    {
        if (!(isAttacking || isRecoiling)) return false;

        StopTracing(true); // ✅ 공격/리코일 중에도 추적 상태 리셋
        FaceByTargetX(playerTr.position.x);

        if (lockZAxis) FixZ();
        return true;
    }

    Vector3 GetPlayerPosLockedZ()
    {
        Vector3 p = playerTr.position;
        if (lockZAxis) p.z = lockZ;
        return p;
    }

    void SetAnim(bool trace, bool patrol)
    {
        animator.SetBool(HashTrace, trace);
        animator.SetBool(HashPatrol, patrol);
    }

    // =======================
    // ✅ 추적/가속/오디오/애니/정지 한방 정리
    // =======================
    void StopTracing(bool stopAudio = true)
    {
        StopTraceRampOnly();

        // ✅ 핵심: 다음 추적 진입을 “진입”으로 인식시키기
        wasTracing = false;

        if (stopAudio && chaseAudio != null && chaseAudio.isPlaying)
            chaseAudio.Stop();

        SetAnim(trace: false, patrol: false);
        StopAgentHard();
    }

    // =======================
    // Facing
    // =======================
    void FaceByTargetX(float targetX)
    {
        float dirX = targetX - transform.position.x;
        if (invertFacing) dirX = -dirX;

        if (dirX >= 0f)
            transform.rotation = Quaternion.Euler(0, 90f, 0);
        else
            transform.rotation = Quaternion.Euler(0, -90f, 0);
    }

    // =======================
    // States
    // =======================
    void HandleAttackState(Vector3 playerPos)
    {
        StopTracing(false); // 상태 정리(오디오는 UpdateChaseAudio가 끄니 여기선 false도 OK)

        FaceByTargetX(playerPos.x);

        if (Time.time < nextAttackTime) return;

        if (debugLog) Debug.Log("[EnemyCtrl] ATTACK TRIGGER!");

        // ✅ 트리거 꼬임 방지: 같은 프레임/짧은 간격의 중복 입력 정리
        animator.ResetTrigger(HashAttack);
        // Hit는 “맞으면 끊기는 게 정상”이라 Attack 넣을 때 ResetHit는 보통 안 함
        // animator.ResetTrigger(HashHit); // 필요하면(공격 입력이 Hit랑 충돌하는 이상한 케이스만)

        animator.SetTrigger(HashAttack);
        nextAttackTime = Time.time + attackCooldown;

        StartCoroutine(AttackLock());
        StartCoroutine(EnableHitboxTemporarily());
        StartCoroutine(RecoilBack());
    }

    void HandleTraceState(Vector3 playerPos)
    {
        // ✅ 추적 “진입” 순간 램프 시작
        if (!wasTracing)
            StartTraceRamp();

        navi.isStopped = false;
        navi.stoppingDistance = attackDist;
        navi.SetDestination(playerPos);

        SetAnim(trace: true, patrol: false);
        FaceByTargetX(playerPos.x);
    }

    void HandlePatrolOrIdleState()
    {
        StopTraceRampOnly();
        wasTracing = false;

        SetAnim(trace: false, patrol: usePatrol);
        ResetTraceMove();

        if (usePatrol)
            PatrolMove();
        else
            StopAgentHard();
    }

    // =======================
    // Patrol
    // =======================
    void PatrolMove()
    {
        if (navi == null) return;
        if (!patrolInited) InitPatrolPoints();
        if (!navi.enabled || !navi.isOnNavMesh) return;

        navi.speed = patrolSpeed;
        navi.acceleration = patrolAccel;
        navi.stoppingDistance = 0f;

        float distX = Mathf.Abs(transform.position.x - patrolTarget.x);

        if (distX <= patrolArriveEps)
        {
            if (Time.time < patrolNextSwitchTime)
            {
                StopAgentHard();
                return;
            }

            patrolTarget = (Mathf.Abs(patrolTarget.x - patrolA.x) < 0.001f) ? patrolB : patrolA;
            patrolNextSwitchTime = Time.time + patrolWaitTime;
        }

        Vector3 dest = patrolTarget;
        if (lockZAxis) dest.z = lockZ;

        navi.isStopped = false;
        navi.SetDestination(dest);

        FaceByTargetX(dest.x);
    }

    // =======================
    // ✅ 부드러운 추적 가속 램프
    // =======================
    void StartTraceRamp()
    {
        if (navi == null) return;

        if (traceRampCo != null) StopCoroutine(traceRampCo);
        traceRampCo = StartCoroutine(RampChase());
    }

    void StopTraceRampOnly()
    {
        if (traceRampCo != null)
        {
            StopCoroutine(traceRampCo);
            traceRampCo = null;
        }
    }

    void ResetTraceMove()
    {
        if (navi == null) return;

        navi.speed = startTraceSpeed;
        navi.acceleration = startTraceAccel;
    }

    IEnumerator RampChase()
    {
        float t = 0f;

        float s0 = startTraceSpeed;
        float s1 = chaseSpeed;

        float a0 = startTraceAccel;
        float a1 = chaseAccel;

        navi.speed = s0;
        navi.acceleration = a0;

        while (t < rampTime)
        {
            if (isAttacking || isRecoiling || isHitted) yield break;

            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / rampTime);

            float smooth = r * r * (3f - 2f * r);

            navi.speed = Mathf.Lerp(s0, s1, smooth);
            navi.acceleration = Mathf.Lerp(a0, a1, smooth);

            yield return null;
        }

        navi.speed = s1;
        navi.acceleration = a1;
        traceRampCo = null;
    }

    // =======================
    // 공격 애니/판정
    // =======================
    IEnumerator AttackLock()
    {
        isAttacking = true;

        StopAgentHard();
        SetAnim(trace: false, patrol: false);

        yield return new WaitForSeconds(attackLockTime);

        isAttacking = false;
    }

    bool IsPlayerInAttackBox()
    {
        if (attackBox == null) return false;

        Vector3 center = attackBox.transform.TransformPoint(attackBox.center);
        Vector3 halfExtents = Vector3.Scale(attackBox.size * 0.5f, attackBox.transform.lossyScale);
        Quaternion rot = attackBox.transform.rotation;

        return Physics.CheckBox(center, halfExtents, rot, playerLayer, QueryTriggerInteraction.Collide);
    }

    void ApplyDamageByOverlapBox()
    {
        if (attackBox == null) return;

        Vector3 center = attackBox.transform.TransformPoint(attackBox.center);
        Vector3 halfExtents = Vector3.Scale(attackBox.size * 0.5f, attackBox.transform.lossyScale);
        Quaternion rot = attackBox.transform.rotation;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, rot, playerLayer, QueryTriggerInteraction.Collide);

        for (int i = 0; i < hits.Length; i++)
        {
            var dmg = hits[i].GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damage);
                hitAppliedThisSwing = true;
                return;
            }
        }
    }

    IEnumerator EnableHitboxTemporarily()
    {
        if (attackBox == null) yield break;

        hitAppliedThisSwing = false;
        attackBox.enabled = true;

        float t = 0f;
        while (t < hitboxOnTime)
        {
            t += Time.deltaTime;

            if (!hitAppliedThisSwing)
                ApplyDamageByOverlapBox();

            yield return null;
        }

        attackBox.enabled = false;
    }

    // =======================
    // 이동/정지 보조
    // =======================
    void StopAgentHard()
    {
        if (navi == null) return;
        if (!navi.enabled) return;
        if (!navi.isOnNavMesh) return;

        navi.isStopped = true;
        navi.ResetPath();
        navi.velocity = Vector3.zero;
    }

    void FixZ()
    {
        Vector3 pos = transform.position;
        pos.z = lockZ;
        transform.position = pos;
    }

    // =======================
    // 리코일
    // =======================
    IEnumerator RecoilBack()
    {
        isRecoiling = true;

        Vector3 start = transform.position;

        Vector3 away = (transform.position - playerTr.position);
        away.y = 0f;
        away.z = 0f;

        if (away.sqrMagnitude < 0.0001f)
        {
            away = -transform.right;
            away.y = 0f;
            away.z = 0f;
        }

        Vector3 target = start + away.normalized * recoilDistance;

        if (lockZAxis)
        {
            start.z = lockZ;
            target.z = lockZ;
        }

        float t = 0f;
        while (t < recoilDuration)
        {
            t += Time.deltaTime;
            float ratio = t / recoilDuration;
            transform.position = Vector3.Lerp(start, target, ratio);

            if (lockZAxis) FixZ();
            yield return null;
        }

        isRecoiling = false;

        if (navi != null && navi.enabled && navi.isOnNavMesh)
            navi.Warp(transform.position);
    }

    // =======================
    // 추적 사운드
    // =======================
    void UpdateChaseAudio(bool isTracingNow)
    {
        if (chaseAudio == null || chaseLoopClip == null) return;

        if (isTracingNow && !wasTracing)
        {
            chaseAudio.clip = chaseLoopClip;
            chaseAudio.loop = chaseLoop;
            chaseAudio.volume = chaseVolume;

            if (!chaseAudio.isPlaying)
                chaseAudio.Play();
        }
        else if (!isTracingNow && wasTracing)
        {
            if (chaseAudio.isPlaying)
                chaseAudio.Stop();
        }

        wasTracing = isTracingNow;
    }

    // =======================
    // 디버그 기즈모
    // =======================
    void OnDrawGizmosSelected()
    {
        if (attackBox != null)
        {
            Vector3 center = attackBox.transform.TransformPoint(attackBox.center);
            Vector3 halfExtents = Vector3.Scale(attackBox.size * 0.5f, attackBox.transform.lossyScale);
            Quaternion rot = attackBox.transform.rotation;

            Gizmos.matrix = Matrix4x4.TRS(center, rot, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        }

        if (usePatrol)
        {
            Vector3 p = transform.position;
            float z = Application.isPlaying ? lockZ : transform.position.z;

            Vector3 a = p; a.x -= patrolRange; if (lockZAxis) a.z = z;
            Vector3 b = p; b.x += patrolRange; if (lockZAxis) b.z = z;

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawLine(a, b);
        }
    }
}
