using UnityEngine;

public class CeilingEnemyCTRL : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Bullet")]
    public float bulletSpeed = 20f;

    [Header("Attack")]
    public float attackCooldown = 5f;
    private float nextAttackTime = 0f;

    public void TryFire()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        FireBullet();
    }

    void FireBullet()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogError("[Enemy] bulletPrefab/firePoint 연결 안됨!");
            return;
        }

        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("[Enemy] 총알 프리팹에 Rigidbody 없음!");
            return;
        }

        // ✅ 방향: firePoint 기준 "아래"
        Vector3 downDir = firePoint.up;

        // ✅ 프리팹이 이상해도 날아가게 기본값 강제
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // ✅ 아래로 발사
        rb.velocity = downDir.normalized * bulletSpeed;

        // ✅ 총알 비주얼도 아래 방향을 보게(필요 없으면 지워도 됨)
        bullet.transform.rotation = Quaternion.LookRotation(downDir, Vector3.forward);

        // 디버그 (원하면 유지)
        // Debug.Log($"[Bullet] downDir={downDir}, vel={rb.velocity}");
    }
}
