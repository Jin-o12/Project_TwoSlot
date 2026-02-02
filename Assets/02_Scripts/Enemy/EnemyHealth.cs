using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHp = 100;
    int hp;

    void Awake() => hp = maxHp;

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
