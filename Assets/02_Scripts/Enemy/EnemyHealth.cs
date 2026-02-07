using UnityEngine;
using UnityEngine.Audio; // 믹서 사용 시 필요

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHp = 100;
    int hp;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioMixerGroup sfxMixerGroup; // ✅ 믹서 그룹 연결용 변수 (인스펙터에서 연결 필요)

    public AudioClip[] hitClips;
    public AudioClip dieClip;

    [Header("VFX (Hit Effect)")]
    public GameObject hitVfxPrefab;
    public Vector3 hitVfxOffset = Vector3.zero;
    public bool useEnemyRotationForHitVfx = false;
    public bool hitVfxUseContactPoint = true;
    public float hitVfxMinInterval = 0.05f;

    [Header("VFX (Death Effect)")]
    public GameObject deathVfxPrefab;
    public Vector3 deathVfxOffset = Vector3.zero;
    public bool useEnemyRotationForVfx = false;

    private bool isDying = false;
    private float hitVfxCooldown = 0f;

    void Awake()
    {
        hp = maxHp;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 시작 시 오디오 소스에 믹서 그룹 적용
        if (audioSource != null && sfxMixerGroup != null)
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    void Update()
    {
        if (hitVfxCooldown > 0f)
            hitVfxCooldown -= Time.deltaTime;
    }

    public void TakeDamage(int dmg)
    {
        TakeDamage(dmg, null);
    }

    public void TakeDamage(int dmg, Vector3? hitPointWS)
    {
        if (isDying) return;

        hp -= dmg;
        PlayRandomHitSound();
        SpawnHitVfx(hitPointWS);

        if (hp <= 0)
            DieOnce();
    }

    void PlayRandomHitSound()
    {
        if (isDying) return;
        if (hitClips == null || hitClips.Length == 0) return;

        AudioClip clip = hitClips[Random.Range(0, hitClips.Length)];
        if (clip == null) return;

        if (audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    void DieOnce()
    {
        if (isDying) return;
        isDying = true;

        if (GameDataManager.Instance != null)
            GameDataManager.Instance.AddScore(10);

        SpawnDeathVfx();

        if (dieClip != null)
        {
            // ❌ 기존: PlayClipAtPoint (무조건 3D라 멀게 들림, 믹서 적용 안됨)
            // AudioSource.PlayClipAtPoint(dieClip, transform.position, 1f);

            // ✅ 수정: 커스텀 함수 사용 (2D로 빵빵하게 들림 + 믹서 적용)
            PlayDeathSound(dieClip, transform.position);
        }

        DisableColliders();
        Destroy(gameObject);
    }

    // ⭐ [핵심 해결책] 사망 사운드 전용 커스텀 함수
    void PlayDeathSound(AudioClip clip, Vector3 position)
    {
        GameObject tempGO = new GameObject("TempDeathAudio");
        tempGO.transform.position = position;

        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;

        // 여기가 핵심입니다! 
        // 0으로 하면 2D(거리에 상관없이 크게 들림)
        // 1로 하면 3D(멀면 작게 들림) -> 기존 문제는 이게 1이었기 때문
        aSource.spatialBlend = 0f;

        // 믹서 그룹 적용 (볼륨 조절을 위해)
        if (sfxMixerGroup != null)
            aSource.outputAudioMixerGroup = sfxMixerGroup;

        aSource.Play();

        // 클립 길이만큼 대기 후 삭제
        Destroy(tempGO, clip.length);
    }

    // ... (아래 SpawnHitVfx, SpawnDeathVfx, DisableColliders, GetParticleLifetime 등은 기존과 동일) ...

    void SpawnHitVfx(Vector3? hitPointWS)
    {
        if (isDying) return;
        if (hitVfxPrefab == null) return;
        if (hitVfxMinInterval > 0f && hitVfxCooldown > 0f) return;
        hitVfxCooldown = hitVfxMinInterval;

        Vector3 pos;
        if (hitVfxUseContactPoint && hitPointWS.HasValue)
            pos = hitPointWS.Value + hitVfxOffset;
        else
            pos = transform.position + hitVfxOffset;

        Quaternion rot = useEnemyRotationForHitVfx ? transform.rotation : Quaternion.identity;
        GameObject vfxObj = Instantiate(hitVfxPrefab, pos, rot);
        float lifeTime = GetParticleLifetime(vfxObj);
        Destroy(vfxObj, lifeTime);
    }

    void SpawnDeathVfx()
    {
        if (deathVfxPrefab == null) return;
        Vector3 pos = transform.position + deathVfxOffset;
        Quaternion rot = useEnemyRotationForVfx ? transform.rotation : Quaternion.identity;
        GameObject vfxObj = Instantiate(deathVfxPrefab, pos, rot);
        float lifeTime = GetParticleLifetime(vfxObj);
        Destroy(vfxObj, lifeTime);
    }

    void DisableColliders()
    {
        var cols = GetComponentsInChildren<Collider>(true);
        foreach (var c in cols) c.enabled = false;
    }

    float GetParticleLifetime(GameObject vfxObj)
    {
        float max = 2f;
        var systems = vfxObj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in systems)
        {
            var main = ps.main;
            float startLifetimeMax = 0f;
            if (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                startLifetimeMax = main.startLifetime.constantMax;
            else if (main.startLifetime.mode == ParticleSystemCurveMode.Constant)
                startLifetimeMax = main.startLifetime.constant;
            else
                startLifetimeMax = 2f;

            float t = main.duration + startLifetimeMax + 0.5f;
            if (t > max) max = t;
        }
        return max;
    }
}