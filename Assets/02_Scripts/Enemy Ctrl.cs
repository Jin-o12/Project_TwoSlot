using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
// 애니메이터 컨트롤러와 내비를 이용해서 플레이어를 추적하고 공격 할 수 있도록 만들어 볼 것.
//근데 공격 애니메이션이 없는 듯 어떻게 만드는지 모름 좀비때는 있던거 썻던거 같음
public class EnemyCtrl : MonoBehaviour
{
    public NavMeshAgent navi; //내비
    //public Animator animator; // 애니메이터
    [Header("적 추적 범위와 공격 범위")]
    public float traceDist = 50f; //인식 범위,추적 범위
    public float attackDist = 2f;
    [Header("Z축 고정")]
    public float lockZ = 0f; //적 Z축 고정 위치
    public bool lockZAxis = true; //Z축 고정 사용 여부 
    public Transform enemyTr; // 에너미 위치
    public Transform playerTr; // 플레이어 위치
    private bool IsAttacking = false;
    void Start()
    {
        navi = this.gameObject.GetComponent<NavMeshAgent>();
        //animator = GetComponent<Animator>();
        enemyTr = GetComponent<Transform>();
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
                              //플레이어 태그로 플레이어와 에너미의 거리 인식
        navi.updateRotation = false;
        navi.updateUpAxis = false;
    }                 

    void Update()
    {
        if (playerTr == null) return;
        if (!navi.enabled || !navi.isOnNavMesh) return;
        //플레이어 목적지 Z축 고정
           Vector3 playerPos = playerTr.position;
        if (lockZAxis) playerPos.z = lockZ;
        //거리계산도 고정된 자표 기준으로 계싼
        float distance = Vector3.Distance(enemyTr.position, playerPos);
        //공격 거리면 정지
        if(distance <= attackDist)
        {
            StopAgent();
            //animator.SetBool("IsTrace",false);
            //animator.SetBool("IsAttack",true);
        }
        else if (distance <= traceDist)
        {
            navi.isStopped = false;
            navi.SetDestination(playerPos);
            //animator.SetBool("IsAttack", false);
            //animator.SetBool("IsTrace", true);
        }
        else
        {
            navi.isStopped = true;
            navi.ResetPath();
            //animator.SetBool("IsAttack", false);
            //animator.SetBool("IsTrace", false);
        }
        //에너미 Z축 고정
        if(lockZAxis) FixZ();
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
        if(playerTr.position.x > transform.position.x)
        transform.rotation = Quaternion.Euler(0, 90f, 0);
        else
        transform.rotation = Quaternion.Euler(0, -90f, 0);
    }
}