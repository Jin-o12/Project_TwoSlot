using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 3f;
    public float speed = 10.0f;

    bool dead;

    void Awake()
    {
        Destroy(gameObject, lifeTime);

        // Trigger 방식 추천
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }
    

    void OnTriggerEnter(Collider other)
    {
        if (dead) return;

        // 자기 자신/자식과 충돌 무시(필요시)
        if (other.transform.IsChildOf(transform)) return;

        dead = true;

        var dmg = other.GetComponentInParent<IDamageable>();
        dmg?.TakeDamage(damage);

        Destroy(gameObject);
    }
}
