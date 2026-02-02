using UnityEngine;

public class CeilingEnemyFireTrigger : MonoBehaviour
{
    [SerializeField] private CeilingEnemyCTRL enemy; // 부모 컨트롤러
    [SerializeField] private string playerTag = "Player";

    private void Awake()
    {
        // 인스펙터로 안 넣어도 부모에서 자동 찾기
        if (enemy == null)
            enemy = GetComponentInParent<CeilingEnemyCTRL>();

        if (enemy == null)
            Debug.LogError("[CeilingEnemyFireTrigger] 부모에서 CeilingEnemyCTRL을 찾지 못했습니다.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemy == null) return;
        if (!other.CompareTag(playerTag)) return;

        enemy.TryFire(); // 닿는 순간 1발
    }

    private void OnTriggerStay(Collider other)
    {
        if (enemy == null) return;
        if (!other.CompareTag(playerTag)) return;

        enemy.TryFire(); // 닿아있는 동안 쿨타임마다 발사
    }
}
