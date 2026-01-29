using UnityEngine;

public class PlaneEnemyDmgTrigger : MonoBehaviour
{
    public int hpInit = 100;
    public int hp;
    public bool isDie = false;

    private Animator animator;

    [Header("References")]
    public BoxCollider boxCol; // 인스펙터로 연결 가능

    void Awake()
    {
        hp = hpInit;

        // Animator 안전하게
        animator = GetComponentInChildren<Animator>();

        // boxCol이 인스펙터에서 비어있으면 자식에서 찾기
        if (boxCol == null)
            boxCol = GetComponentInChildren<BoxCollider>();

        if (boxCol == null)
            Debug.LogError("[PlaneEnemyDmgTrigger] BoxCollider를 찾지 못했습니다. 자식에 HitTrigger가 있는지 확인하세요.");
    }
}
