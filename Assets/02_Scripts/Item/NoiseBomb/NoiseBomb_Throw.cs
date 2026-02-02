using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoiseBomb_Throw : MonoBehaviour
{
    [Header("Explode Timing")]
    public float fuseTime = 1.0f;            // 몇 초 뒤 폭발(소리)
    public bool explodeOnFirstHit = true;    // 닿자마자 폭발(소리)

    [Header("Noise")]
    public float noiseRadius = 15f;          // 이 반경 안의 적만 유인
    public float distractTime = 5f;          // 적이 플레이어 대신 소리 위치 추적하는 시간

    [Header("Sound")]
    public AudioSource audioSource;          // 프리팹에 AudioSource 붙이면 자동 연결
    public AudioClip noiseClip;              // 사용할 소리
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Optional")]
    public GameObject vfxPrefab;             // 있으면 터질 때 이펙트

    bool exploded;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = false;               // 던져서 바닥에 '충돌'시키는 쪽이 자연스러움

        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (fuseTime > 0f) Invoke(nameof(Explode), fuseTime);
    }

    void OnCollisionEnter(Collision c)
    {
        if (!explodeOnFirstHit || exploded) return;
        Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        CancelInvoke(nameof(Explode));

        Vector3 pos = transform.position;

        // 1) 소리 재생
        if (noiseClip)
        {
            if (audioSource) audioSource.PlayOneShot(noiseClip, volume);
            else AudioSource.PlayClipAtPoint(noiseClip, pos, volume);
        }

        // 2) 적 유인 이벤트 발생
        NoiseSystem.Emit(transform.position, noiseRadius, distractTime);

        // 3) 이펙트
        if (vfxPrefab) Instantiate(vfxPrefab, pos, Quaternion.identity);

        // 4) 오브젝트 제거 (소리만 재생하면 바로 지워도 OK)
        Destroy(gameObject, 5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, noiseRadius);
    }
}
