using UnityEngine;

public class CeilingEnemyCTRL : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    public float attackCooldown = 5f;
    private float nextAttackTime = 0f;

    void Start()
    {
        Debug.Log("[Enemy] Start OK : " + gameObject.name);
    }

    public void TryFire()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        Debug.Log("[Enemy] 발사 시도!");
        FireBullet();
    }

    void FireBullet()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogError("[Enemy] bulletPrefab/firePoint 연결 안됨!");
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Debug.Log("[Enemy] 총알 생성됨: " + bullet.name);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = Vector3.down * bulletSpeed;
        else Debug.LogError("[Enemy] 총알 프리팹에 Rigidbody 없음!");
    }
}
