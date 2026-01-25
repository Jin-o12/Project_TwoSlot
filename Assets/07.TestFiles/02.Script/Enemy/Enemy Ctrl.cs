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
    public float traceDist = 50f; //인식 범위,추적 범위
    public float attackDist = 2f;
    public Transform enemyTr; // 에너미 위치
    public Transform playerTr; // 플레이어 위치
    void Start()
    {
        navi = this.gameObject.GetComponent<NavMeshAgent>();
        //animator = GetComponent<Animator>();
        enemyTr = GetComponent<Transform>();
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
    }                 //플레이어 태그로 플레이어와 에너미의 거리 인식

    void Update()
    {
        if (!navi.enabled || !navi.isOnNavMesh || playerTr == null) return;
        float distance = Vector3.Distance(enemyTr.position, playerTr.position);
        
        if(distance <= attackDist)
        {
            navi.isStopped = true;
            navi.ResetPath();
            //animator.SetBool("IsTrace",false);
            //animator.SetBool("IsAttack",true);
        }
        if (distance <= traceDist)
        {
            navi.isStopped = false;
            navi.SetDestination(playerTr.position);
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
    }
}