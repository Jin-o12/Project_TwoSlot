using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTouchAttack : MonoBehaviour
{
    public int damage = 10;
    public float hitCooldown = 0.5f;
    public LayerMask targetMask; // Player 레이어 넣기

    float lastHitTime;

    void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetMask) == 0) return;
        if (Time.time - lastHitTime < hitCooldown) return;

        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg == null) return;

        lastHitTime = Time.time;
        dmg.TakeDamage(damage);
    }
}
