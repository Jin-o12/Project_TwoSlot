using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCtrl : MonoBehaviour
{
    public NavMeshAgent navi;

    [Header("적 추적 범위와 공격 범위")]
    public float traceDist = 30f;
    public float attackDist = 0.5f; // stoppingDistance 참고용

    [Header("Z축 고정")]
    public float lockZ = 0f;
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

    void Start()
    {
        navi = GetComponent<NavMeshAgent>();
        enemyTr = transform;
        playerTr = GameObject.FindWithTag("Player")?.transform;

        if (navi != null)
        {
            navi.updateRotation = false;
            navi.updateUpAxis = false;
            navi.stoppingDistance = attackDist;

            // ✅ 적끼리 자동으로 피하는 기능 끄기
            navi.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            // ✅ 필요하면 에이전트 반지름도 줄이기(붙게 만들기)
            navi.radius = 0.15f; // 0.1~0.25 테스트
        }

        animator = GetComponent<Animator>();

        if (attackBox != null)
        {
            // 공격 판정용 콜라이더는 기본 OFF
            attackBox.enabled = false;
        }
    }

    void Update()
    {
        
        if (animator == null) return;
        if (navi == null) return;
        if (!navi.enabled || !navi.isOnNavMesh) return;

        //  공격/리코일 중에는 추적/판정 모두 멈추기
        if (isAttacking||isRecoiling)
        {
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

        // if (debugLog)
        // {
        //     Debug.Log($"[EnemyCtrl] distX={distance:F2}, inAttackRange={inAttackRange}, time={Time.time:F2}, next={nextAttackTime:F2}");
        // }

        if (inAttackRange)
        {
            StopAgent();
            animator.SetBool("Trace", false);

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
            navi.isStopped = false;
            navi.SetDestination(playerPos);
            animator.SetBool("Trace", true);
        }
        else
        {
            StopAgent();
            animator.SetBool("Trace", false);
        }

        if (lockZAxis) FixZ();
        FaceToPlayer();
    }

    // ✅ 공격 애니가 보이도록 잠깐 멈추기
    IEnumerator AttackLock()
    {
        isAttacking = true;

        StopAgent();
        animator.SetBool("Trace", false);

        yield return new WaitForSeconds(attackLockTime);

        isAttacking = false;
    }

    // ★ 플레이어가 공격 박스 안에 있는지 확인 (안정 버전)
    bool IsPlayerInAttackBox()
    {
        if (attackBox == null) return false;

        // BoxCollider의 로컬 center -> 월드 center
        Vector3 center = attackBox.transform.TransformPoint(attackBox.center);

        // 로컬 size 반영 + 월드 스케일 적용
        Vector3 halfExtents = Vector3.Scale(attackBox.size * 0.5f, attackBox.transform.lossyScale);

        Quaternion rot = attackBox.transform.rotation;

        return Physics.CheckBox(center, halfExtents, rot, playerLayer, QueryTriggerInteraction.Collide);
    }

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
        if (playerTr.position.x > transform.position.x)
            transform.rotation = Quaternion.Euler(0, 90f, 0);
        else
            transform.rotation = Quaternion.Euler(0, -90f, 0);
    }
    void FaceToTarget(Vector3 targetPos)
    {
        if (targetPos.x > transform.position.x)
            transform.rotation = Quaternion.Euler(0, 90f, 0);
        else
            transform.rotation = Quaternion.Euler(0, -90f, 0);
    }

    IEnumerator EnableHitboxTemporarily()
    {
        if (attackBox == null) yield break;

        attackBox.enabled = true;
        yield return new WaitForSeconds(hitboxOnTime);
        attackBox.enabled = false;
    }

    // ✅ 플레이어 반대 방향으로 "항상" 튕기기 (부호 꼬임 방지: 벡터 기반)
    IEnumerator RecoilBack()
    {
        isRecoiling = true;

        Vector3 start = transform.position;

        // 플레이어로부터 멀어지는 방향(플레이어 -> 적)
        Vector3 away = (transform.position - playerTr.position);

        // 2.5D: X축만 사용
        away.y = 0f;
        away.z = 0f;

        // 겹쳤을 때(0벡터) 안전 처리: 적의 오른쪽 기준 반대로
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

        // NavMeshAgent 위치 동기화(꼬임 방지)
        if (navi != null && navi.enabled && navi.isOnNavMesh)
            navi.Warp(transform.position);
    }

    void OnDrawGizmosSelected()
    {
        if (attackBox == null) return;

        // CheckBox와 동일한 방식으로 기즈모 표시
        Vector3 center = attackBox.transform.TransformPoint(attackBox.center);
        Vector3 halfExtents = Vector3.Scale(attackBox.size * 0.5f, attackBox.transform.lossyScale);
        Quaternion rot = attackBox.transform.rotation;

        Gizmos.matrix = Matrix4x4.TRS(center, rot, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
    }
}
