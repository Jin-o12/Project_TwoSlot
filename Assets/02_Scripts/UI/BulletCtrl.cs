using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    [Header("Life")]
    public float lifeTime = 3f;

    [Header("Hit")]
    public int damage = 10;

    void Start()
    {
        // 일정 시간 후 자동 제거
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 자기 자신(플레이어/총) 무시용
        if (collision.gameObject.CompareTag("Player"))
            return;

        // 데미지 예시 (Enemy 태그 있을 때)
        // if (collision.gameObject.CompareTag("Enemy"))
        // {
        //     Debug.Log("Hit Enemy!");

        //     // 예: 체력 스크립트 있으면 호출 가능
        //     // collision.gameObject.GetComponent<EnemyHP>()?.TakeDamage(damage);
        // }

        // 맞으면 파괴
        Destroy(gameObject);
    }
}
