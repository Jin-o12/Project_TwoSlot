using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 3f;
    public float speed = 10.0f;
    
    [Header("Hit (Enemy Layer)")]
    public string enemyLayerName = "ENEMY";
    public GameObject hitVfxPrefab;     // 피격 파티클 프리팹(선택)
    public AudioClip hitSfx;            // 피격 사운드(선택)
    public float hitSfxVolume = 1f;

    bool dead;
    int enemyLayer;

    void Awake()
    {
        Destroy(gameObject, lifeTime);

        // Trigger 방식 추천
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
    }
    

    void OnTriggerEnter(Collider other)
    {
        if (dead) return;

        // 자기 자신/자식과 충돌 무시(필요시)
        if (other.transform.IsChildOf(transform)) return;
        // ENEMY 레이어가 아니면 무시
        if (other.gameObject.layer != enemyLayer) return;

        dead = true;

        var dmg = other.GetComponentInParent<IDamageable>();
        dmg?.TakeDamage(damage);

        // 피격 이펙트
        if (hitVfxPrefab)
        {
            var go = Instantiate(hitVfxPrefab, other.ClosestPoint(transform.position), Quaternion.identity);
            var ps = go.GetComponent<ParticleSystem>();
            if (ps) ps.Play();
            Destroy(go, 0.5f);
        }

        // 피격 사운드
        if (hitSfx)
            AudioSource.PlayClipAtPoint(hitSfx, transform.position, hitSfxVolume);
            
        Destroy(gameObject);
    }
}
