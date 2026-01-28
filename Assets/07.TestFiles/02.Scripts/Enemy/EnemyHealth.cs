using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHp = 10;
    int hp;

    void Awake() => hp = maxHp;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0) Destroy(gameObject);
    }
}
