using UnityEngine;

public class CeilingEnemyFireTrigger : MonoBehaviour
{
    [SerializeField] private CeilingEnemyCTRL enemy; // 부모 컨트롤러
    [SerializeField] private string playerTag;

    private void Awake()
    {
        // 인스펙터로 안 넣어도 부모에서 자동 찾기
        if (enemy == null)
            enemy = GetComponentInParent<CeilingEnemyCTRL>();
    
        playerTag = "Player";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemy == null) return;
        if (!other.CompareTag(playerTag)) return;

        enemy.TryFire();
        Debug.Log("[CeilingTrigger] hit = " + other.name);
    }

    private void OnTriggerStay(Collider other)
    {
        if (enemy == null) return;
        if (!other.CompareTag(playerTag)) return;

        enemy.TryFire(); // 닿아있는 동안 쿨타임마다 발사
    }
}
