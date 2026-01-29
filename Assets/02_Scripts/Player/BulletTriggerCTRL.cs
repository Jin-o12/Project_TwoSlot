using UnityEngine;

public class CeilingEnemyTrigger : MonoBehaviour
{
    private CeilingEnemyCTRL enemy;

    void Start()
    {
        enemy = GetComponentInParent<CeilingEnemyCTRL>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("트리거 감지됨: " + other.name);

        if (!other.CompareTag("Player")) return;
        if (enemy == null) return;

        enemy.TryFire();
    }
}
