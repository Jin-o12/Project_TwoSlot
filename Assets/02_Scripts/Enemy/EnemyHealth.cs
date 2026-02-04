using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHp = 100;
    int hp;
    public AudioSource audioSource;
    public AudioClip dieClip;

    void Awake() => hp = maxHp;

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0) Die();
    }

    void Die()
    {
        GameDataManager.Instance.AddScore(150);
        Destroy(gameObject);
        AudioSource.PlayClipAtPoint(dieClip, transform.position, 1f);
    }
}
