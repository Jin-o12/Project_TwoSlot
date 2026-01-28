using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCtrl : MonoBehaviour
{
    public NavMeshAgent navi;

    [Header("적 추적 범위와 공격 범위")]
    public float traceDist = 50f;
    public float attackDist = 0.5f;

    [Header("Z축 고정")]
    public float lockZ = 0f;
    public bool lockZAxis = true;

    public Transform enemyTr;
    public Transform playerTr;

    [Header("공격 쿨타임")]
    public float attackCooldown = 2.0f;
    private float nextAttackTime = 0f;

    [Header("공격 히트박스")]
    public BoxCollider attackBox;
    public float hitboxOnTime = 0.2f;

    [Header("리코일(공격시 살짝 넉백 넣기")]
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

        animator = GetComponent<Animator>();

        // 히트박스 기본 OFF
        if (attackBox != null)
            attackBox.enabled = false;
    }

    void Update()
    {
        if (playerTr == null) return;
        if (animator == null) return;
        if (!navi.enabled || !navi.isOnNavMesh) return;

        // 리코일 중에는 추가 동작 막기 
        if (isRecoiling)
        {
            StopAgent();
            if (lockZAxis) FixZ();
            FaceToPlayer();
            return;
        }

        Vector3 playerPos = playerTr.position;
        if (lockZAxis) playerPos.z = lockZ;

        float distance = Vector3.Distance(enemyTr.position, playerPos);

        if(isRecoiling)
        {
            StopAgent();
            animator.SetBool("Trace", false);
            return;
        }

        // 공격 거리면 정지 + 공격
        if (distance <= attackDist)
        {
            StopAgent();
            animator.SetBool("Trace", false);

            if (Time.time >= nextAttackTime)
            {
                animator.SetTrigger("Attack");
                nextAttackTime = Time.time + attackCooldown;

                StartCoroutine(EnableHitboxTemporarily());
                StartCoroutine(RecoilBack());
            }
        }
        // 추적 거리면 추적
        else if (distance <= traceDist)
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

    // ✅ 공격 히트박스 잠깐 켰다가 끄기
    IEnumerator EnableHitboxTemporarily()
    {
        if (attackBox == null) yield break;

        attackBox.enabled = true;
        yield return new WaitForSeconds(hitboxOnTime);
        attackBox.enabled = false;
    }

    // ✅ 공격 시 뒤로 살짝 튕기기
    IEnumerator RecoilBack()
    {
        isRecoiling = true;

        Vector3 start = transform.position;

        // 플레이어 반대 방향으로 뒤로 이동
        float dir = (playerTr.position.x > transform.position.x) ? -1f : 1f;
        Vector3 target = start + new Vector3(dir * recoilDistance, 0f, 0f);

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
    }

    // ✅ 애니메이션 이벤트로도 쓰고 싶을 때 호출 가능
    public void PlayRecoil()
    {
        StartCoroutine(RecoilBack());
    }
}
