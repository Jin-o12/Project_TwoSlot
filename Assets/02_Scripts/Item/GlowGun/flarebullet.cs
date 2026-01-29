using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FlareBullet : MonoBehaviour
{
    [Header("Lifetime")]
    public float flareTimer = 9f;      // 타는 시간
    public float destroyDelay = 1f;    // 꺼진 뒤 삭제 여유

    [Header("Flicker")]
    public Vector2 burnIntensityRange = new Vector2(2f, 6f);
    public Vector2 flyIntensityRange  = new Vector2(1.2f, 2.2f); // 날아갈 때는 약하게
    public bool flickerStrongerAfterHit = true;

    [Header("Fade Out")]
    public float smooth = 2.4f;
    public float particleFadeSpeed = 5f;

    [Header("Audio")]
    public AudioClip flareBurningSound;

    Light _flareLight;
    AudioSource _audio;
    ParticleSystem _ps;
    ParticleSystemRenderer _psr;

    bool _burning = true;
    bool _hasHitSomething = false;

    void Awake()
    {
        _flareLight = GetComponentInChildren<Light>();
        _audio = GetComponent<AudioSource>();
        _ps = GetComponentInChildren<ParticleSystem>();
        if (_ps != null) _psr = _ps.GetComponent<ParticleSystemRenderer>();
    }

    void Start()
    {
        if (flareBurningSound != null)
            _audio.PlayOneShot(flareBurningSound);

        // flareTimer 뒤에 "꺼짐" 시작
        Invoke(nameof(BeginFadeOut), flareTimer);

        // 전체 삭제 시간
        Destroy(gameObject, flareTimer + destroyDelay);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 첫 충돌 체크 (바닥/벽 닿았는지)
        if (!_hasHitSomething)
            _hasHitSomething = true;
    }

    void Update()
    {
        if (_flareLight == null) return;

        if (_burning)
        {
            // 타는 동안 깜빡임
            Vector2 range = burnIntensityRange;

            // 충돌 전에는 덜 깜빡이게
            if (!_hasHitSomething)
                range = flyIntensityRange;
            else if (!flickerStrongerAfterHit)
                range = flyIntensityRange;

            _flareLight.intensity = Random.Range(range.x, range.y);
        }
        else
        {
            // 꺼지면서 감쇠
            _flareLight.intensity = Mathf.Lerp(_flareLight.intensity, 0f, Time.deltaTime * smooth);
            _flareLight.range     = Mathf.Lerp(_flareLight.range,     0f, Time.deltaTime * smooth);

            if (_audio != null)
                _audio.volume = Mathf.Lerp(_audio.volume, 0f, Time.deltaTime * smooth);

            // 파티클 크기 줄이기 (있는 경우만)
            if (_psr != null)
                _psr.maxParticleSize = Mathf.Lerp(_psr.maxParticleSize, 0f, Time.deltaTime * particleFadeSpeed);

            // (선택) 파티클 방출도 줄이고 싶으면 아래를 추가할 수 있어:
            // if (_ps != null)
            // {
            //     var em = _ps.emission;
            //     em.rateOverTime = Mathf.Lerp(em.rateOverTime.constant, 0f, Time.deltaTime * particleFadeSpeed);
            // }
        }
    }

    void BeginFadeOut()
    {
        _burning = false;
    }

    void OnDestroy()
    {
        CancelInvoke();
    }
}
