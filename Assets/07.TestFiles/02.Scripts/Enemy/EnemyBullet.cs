using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 100;
    public float lifeTime = 3f;
    public LayerMask hitMask;   // Player + Wall 등
    public GameObject hitVfxPrefab;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 마스크 밖이면 무시
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        // 데미지
        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
            dmg.TakeDamage(damage);

        // 이펙트
        if (hitVfxPrefab != null)
            Instantiate(hitVfxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
    
}
