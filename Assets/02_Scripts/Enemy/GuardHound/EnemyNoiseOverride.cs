using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
// 적이 소음에 반응하도록 강제하는 스크립트

public class EnemyNoiseOverride : MonoBehaviour
{
    [Header("Refs")]
    public EnemyCtrl enemyCtrl;     // 기존 EnemyCtrl 드래그
    public NavMeshAgent agent;
    public Animator animator;

    [Header("2.5D Lock")]
    public bool lockZAxis = true;
    public float lockZ = 0f;

    [Header("Anim Param")]
    public string traceBool = "Trace";

    Coroutine noiseCo;

    bool isDistracted = false;   // 지금 소리에 끌린 상태인가?

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!enemyCtrl) enemyCtrl = GetComponent<EnemyCtrl>();

        if (lockZAxis)
            lockZ = transform.position.z;
    }

    void OnEnable()
    {
        NoiseSystem.OnNoise += OnNoise;
    }

    void OnDisable()
    {
        NoiseSystem.OnNoise -= OnNoise;
    }

    // ===============================
    // NoiseSystem에서 호출됨
    // ===============================
    void OnNoise(Vector3 pos, float radius, float time)
    {
        // 거리 체크
        if ((transform.position - pos).sqrMagnitude > radius * radius)
            return;

        if (lockZAxis)
            pos.z = lockZ;

        if (noiseCo != null)
            StopCoroutine(noiseCo);

        noiseCo = StartCoroutine(CoDistract(pos, time));
    }

    // ===============================
    // 소리 추적 코루틴
    // ===============================
    IEnumerator CoDistract(Vector3 noisePos, float time)
    {
        isDistracted = true;

        // ✅ EnemyCtrl 잠시 비활성화
        if (enemyCtrl)
            enemyCtrl.enabled = false;

        // 이동 시작
        if (agent)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(noisePos);
        }

        if (animator && !string.IsNullOrEmpty(traceBool))
            animator.SetBool(traceBool, true);

        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;

            // Z 고정
            if (lockZAxis)
            {
                Vector3 p = transform.position;
                p.z = lockZ;
                transform.position = p;
            }

            // 목적지 계속 유지 (중간에 멈추는 버그 방지)
            if (agent && agent.enabled)
                agent.SetDestination(noisePos);

            FaceTo(noisePos);

            yield return null;
        }

        // ===============================
        // 종료 처리
        // ===============================

        isDistracted = false;

        if (animator && !string.IsNullOrEmpty(traceBool))
            animator.SetBool(traceBool, false);

        // Enemy AI 복구
        if (enemyCtrl)
            enemyCtrl.enabled = true;

        noiseCo = null;
    }

    // ===============================
    // 방향 회전 (2.5D)
    // ===============================
    void FaceTo(Vector3 target)
    {
        float y = (target.x >= transform.position.x) ? 90f : -90f;

        Vector3 e = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(e.x, y, e.z);
    }

    // ===============================
    // 외부 확인용 (필요하면 사용)
    // ===============================
    public bool IsDistracted()
    {
        return isDistracted;
    }
}
