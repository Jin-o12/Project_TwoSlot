using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
   public int damage = 1;
    public float lifeTime = 3f;

    bool dead;

    void Start() => Destroy(gameObject, lifeTime);

    void OnCollisionEnter(Collision c)
    {
        if (dead) return;
        dead = true;

        // 자신의 자식 파츠(Head/Casing 등)랑 충돌하면 무시
        if (c.transform.IsChildOf(transform))
        {
            dead = false;
            return;
        }

        var dmg = c.collider.GetComponentInParent<IDamageable>();
        dmg?.TakeDamage(damage);

        Destroy(gameObject);
    }
}
