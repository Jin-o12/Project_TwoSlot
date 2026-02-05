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
    bool wasTracing = false;

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

    [Header("디버그")]
    public bool debugLog = false; // true로 켜면 공격 조건/log 확인 가능

    private Animator animator;
    bool hitAppliedThisSwing = false;

    void Start()
    {
        navi = GetComponent<NavMeshAgent>();
        enemyTr = transform;
        playerTr = GameObject.FindWithTag("Player")?.transform;
        lockZ = gameObject.transform.position.z;

        if (navi != null)
        {
            navi.updateRotation = false;
            navi.updateUpAxis = false;
            navi.stoppingDistance = attackDist;

            // ✅ 적끼리 자동으로 피하는 기능 끄기
            navi.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            // ✅ 필요하면 에이전트 반지름도 줄이기(붙게 만들기)
            navi.radius = 0.15f; // 0.1~0.25 테스트

            // ✅ 기본(추적 시작) 값 세팅
            navi.speed = startTraceSpeed;
            navi.acceleration = startTraceAccel;
        }

        animator = GetComponent<Animator>();

        if (attackBox != null)
        {
            // 공격 판정용 콜라이더는 기본 OFF
            attackBox.enabled = false;
        }

        if (chaseAudio == null)
            chaseAudio = GetComponent<AudioSource>(); // 같은 오브젝트에 AudioSource 달려있으면 자동 연결

        if (chaseAudio != null)
        {
            chaseAudio.playOnAwake = false;
            chaseAudio.loop = chaseLoop;
            chaseAudio.volume = chaseVolume;
        }
    }

    void Update()
    {
        if (animator == null) return;
        if (navi == null) return;
        if (!navi.enabled || !navi.isOnNavMesh) return;

        // ✅ 공격/리코일 중에는 추적/판정 모두 멈추기 (+ 가속 램프 중단)
        if (isAttacking || isRecoiling)
        {
            StopTraceRampOnly(); // 램프만 끊고
            StopAgent();
            animator.SetBool("Trace", false);
            if (lockZAxis) FixZ();
            FaceToPlayer();
            return;
        }

        if (playerTr == null) return;
        Vector3 playerPos = playerTr.position;
        if (lockZAxis) playerPos.z = lockZ;

        // 2.5D면 X만 보는 게 안정적
        float distance = Mathf.Abs(enemyTr.position.x - playerPos.x);

        // ★ 공격 판정: 공격 박스 안에 플레이어가 있는지
        bool inAttackRange = IsPlayerInAttackBox();

        bool isTracingNow = false;

        if (inAttackRange)
        {
            StopTraceRampOnly(); // 공격 들어갈 땐 램프 중단
            StopAgent();
            animator.SetBool("Trace", false);
            isTracingNow = false;

            if (Time.time >= nextAttackTime)
            {
                if (debugLog) Debug.Log("[EnemyCtrl] ATTACK TRIGGER!");
                animator.SetTrigger("Attack");
                nextAttackTime = Time.time + attackCooldown;

                StartCoroutine(AttackLock());               // ✅ 공격 애니가 보이도록 잠깐 멈춤
                StartCoroutine(EnableHitboxTemporarily());  // ✅ 히트박스 잠깐 ON
                StartCoroutine(RecoilBack());               // ✅ 플레이어 반대 방향 리코일
            }
        }
        else if (distance < traceDist)
        {
            // ✅ 추적 "진입 순간"에만 부드러운 가속 시작
            if (!wasTracing)
                StartTraceRamp();

            navi.isStopped = false;
            navi.SetDestination(playerPos);
            animator.SetBool("Trace", true);
            isTracingNow = true;
        }
        else
        {
            StopTraceRampOnly();
            StopAgent();
            animator.SetBool("Trace", false);
            isTracingNow = false;

            // 추적이 끊기면 다음 추적을 위해 시작값으로 복귀(원치 않으면 지워도 됨)
            ResetTraceMove();
        }

        UpdateChaseAudio(isTracingNow);     // 추적 사운드 갱신

        if (lockZAxis) FixZ();
        FaceToPlayer();
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

        // 다음 추적 시작을 위해 시작값으로 복귀
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
            // 공격/리코일 들어가면 램프 중단
            if (isAttacking || isRecoiling) yield break;

            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / rampTime);

            // ✅ SmoothStep(부드럽게 빨라지는 느낌)
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

        StopAgent();
        animator.SetBool("Trace", false);

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
    // 이동/회전 보조
    // =======================
    void StopAgent()
    {
        navi.isStopped = true;
        navi.ResetPath();
    }

    void FixZ()
    {
        Vector3 pos = transform.position;
        pos.z = lockZ;
        transform.position = pos;
    }

    void FaceToPlayer()
    {
        if (playerTr != null && playerTr.position.x > transform.position.x)
            transform.rotation = Quaternion.Euler(0, 90f, 0);
        else
            transform.rotation = Quaternion.Euler(0, -90f, 0);
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
    // 디버그 기즈모
    // =======================
    void OnDrawGizmosSelected()
    {
        if (attackBox == null) return;

        Vector3 center = attackBox.transform.TransformPoint(attackBox.center);
        Vector3 halfExtents = Vector3.Scale(attackBox.size * 0.5f, attackBox.transform.lossyScale);
        Quaternion rot = attackBox.transform.rotation;

        Gizmos.matrix = Matrix4x4.TRS(center, rot, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
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
}
