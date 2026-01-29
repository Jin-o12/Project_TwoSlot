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
    public float attackLockTime = 0.8f;
    private bool isAttacking = false;

    [Header("공격 히트박스")]
    public BoxCollider attackBox;
    public float hitboxOnTime = 0.2f;

    [Header("플레이어 레이어(필수)")]
    public LayerMask playerLayer;

    [Header("리코일(공격시 살짝 넉백 넣기)")]
    public float recoilDistance = 0.6f;
    public float recoilDuration = 0.12f;
    private bool isRecoiling = false;

    private Animator animator;

    void Start()
    {
        navi = GetComponent<NavMeshAgent>();
        enemyTr = transform;
        playerTr = GameObject.FindWithTag("Player")?.transform;

        navi.updateRotation = false;
        navi.updateUpAxis = false;

        navi.stoppingDistance = attackDist;

        animator = GetComponent<Animator>();

        if (attackBox != null)
            attackBox.enabled = false;
            // ✅ 적끼리 자동으로 피하는(간격 벌리는) 기능 끄기
navi.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

// ✅ 필요하면 에이전트 반지름도 줄이기(붙게 만들기)
navi.radius = 0.15f;   // 기본이 크면 서로 더 멀어짐. (0.1~0.25 사이로 테스트)

    }

    void Update()
    {
        if (playerTr == null) return;
        if (animator == null) return;
        if (!navi.enabled || !navi.isOnNavMesh) return;

        // ✅ 공격 중(또는 공격 직후 잠깐)은 추적/판정 모두 멈추기
        if (isAttacking)
        {
            StopAgent();
            animator.SetBool("Trace", false);
            if (lockZAxis) FixZ();
            FaceToPlayer();
            return;
        }

        // ✅ 리코일 중에도 멈추기
        if (isRecoiling)
        {
            StopAgent();
            animator.SetBool("Trace", false);
            if (lockZAxis) FixZ();
            FaceToPlayer();
            return;
        }

        Vector3 playerPos = playerTr.position;
        if (lockZAxis) playerPos.z = lockZ;

        // 2.5D면 X만 보는 게 안정적
        float distance = Mathf.Abs(enemyTr.position.x - playerPos.x);

        // ★ 공격 판정: 공격 박스 안에 플레이어가 있는지
        bool inAttackRange = IsPlayerInAttackBox();

        if (inAttackRange)
        {
            StopAgent();
            animator.SetBool("Trace", false);

            if (Time.time >= nextAttackTime)
            {
                animator.SetTrigger("Attack");
                nextAttackTime = Time.time + attackCooldown;

                StartCoroutine(AttackLock());               // ✅ 공격 후 0.8초 멈춤
                StartCoroutine(EnableHitboxTemporarily());
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

    // ★ 플레이어가 공격 박스 안에 있는지 확인
    bool IsPlayerInAttackBox()
    {
        if (attackBox == null) return false;

        // bounds 기반(현재 너 코드 유지). 만약 계속 안 잡히면 TransformPoint 버전으로 바꾸면 더 안정적임.
        Vector3 center = attackBox.bounds.center;
        Vector3 halfExtents = attackBox.bounds.extents;

        return Physics.CheckBox(center, halfExtents, attackBox.transform.rotation, playerLayer, QueryTriggerInteraction.Collide);
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
        Gizmos.matrix = attackBox.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(attackBox.center, attackBox.size);
    }
}
