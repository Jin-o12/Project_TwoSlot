using UnityEngine;

public class NoiseBomb_Throw : MonoBehaviour
{
    [Header("Explosion Timing")]
    public float fuseTime = 1.2f;
    public bool explodeOnFirstHit = true;

    [Header("Safety (중요)")]
    public float armDelay = 0.2f;         // 던진 직후 0.2초 동안은 충돌해도 안 터짐
    public string ignoreTag = "Player";   // 플레이어랑 부딪히면 무시(선택)

    [Header("Ping")]
    public float radius = 12f;
    public string enemyTag = "ENEMY";
    public GameObject pingPrefab;
    public float pingLifeTime = 1.5f;

    [Header("Optional FX")]
    public GameObject explosionVfx;
    public AudioClip explosionSfx;

    bool _exploded;
    float _spawnTime;

    void Start()
    {
        _spawnTime = Time.time;

        if (fuseTime > 0f)
            Invoke(nameof(Explode), fuseTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!explodeOnFirstHit) return;
        if (_exploded) return;

        // 1) 던진 직후 안전장치 시간 동안은 충돌해도 폭발 금지
        if (Time.time - _spawnTime < armDelay) return;

        // 2) 플레이어와의 충돌이면 무시(원하면 유지)
        if (!string.IsNullOrEmpty(ignoreTag) && collision.collider.CompareTag(ignoreTag))
            return;

        Explode();
    }

    void Explode()
    {
        if (_exploded) return;
        _exploded = true;

        CancelInvoke(nameof(Explode));

        if (explosionVfx != null)
            Instantiate(explosionVfx, transform.position, Quaternion.identity);

        if (explosionSfx != null)
            AudioSource.PlayClipAtPoint(explosionSfx, transform.position);

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (col == null) continue;
            if (!col.CompareTag(enemyTag)) continue;

            Vector3 pingPos = col.bounds.center;

            if (pingPrefab != null)
            {
                var ping = Instantiate(pingPrefab, pingPos, Quaternion.identity);
                if (pingLifeTime > 0f) Destroy(ping, pingLifeTime);
            }

            Debug.Log($"[NoiseBomb] Ping -> {col.name}");
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
