using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class FlareBullet : MonoBehaviour
{
    [Header("Lifetime")]
    public float flareTimer = 9f;          // 타는 시간
    public float destroyDelay = 1f;        // 꺼진 뒤 삭제 여유

    [Header("Flicker While Burning")]
    public Vector2 burnIntensityRange = new Vector2(2f, 6f);

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

    void Awake()
    {
        _flareLight = GetComponentInChildren<Light>(); // 혹시 자식에 있을 수도 있어서
        _audio = GetComponent<AudioSource>();
        _ps = GetComponentInChildren<ParticleSystem>();
        if (_ps != null) _psr = _ps.GetComponent<ParticleSystemRenderer>();
    }

    void Start()
    {
        if (flareBurningSound != null)
            _audio.PlayOneShot(flareBurningSound);

        StartCoroutine(BurnThenFade());
        Destroy(gameObject, flareTimer + destroyDelay);
    }

    void Update()
    {
        if (_flareLight == null) return;

        if (_burning)
        {
            // 타는 동안 살짝 깜빡
            _flareLight.intensity = Random.Range(burnIntensityRange.x, burnIntensityRange.y);
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
        }
    }

    IEnumerator BurnThenFade()
    {
        _burning = true;
        yield return new WaitForSeconds(flareTimer);
        _burning = false;
    }
}
