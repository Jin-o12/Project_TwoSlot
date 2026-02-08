using System.Collections;
using UnityEngine;

public class GunFlashEmission : MonoBehaviour
{
    [Header("Bind")]
    [Tooltip("비워두면 부모에서 GunFire를 자동으로 찾습니다.")]
    public GunFire gunFire;

    [Tooltip("비워두면 이 오브젝트(자식 포함)의 모든 Renderer를 자동으로 수집합니다.")]
    public Renderer[] renderers;

    [Header("Emission Flash")]
    public Color flashColor = Color.yellow * 30f;
    public float flashDuration = 0.05f;
    public bool useFade = true;
    public float fadeSpeed = 45f;

    [Header("Light Flash (Optional)")]
    [Tooltip("비워두면 자식/부모에서 Light를 자동 탐색합니다.")]
    public Light flashLight;

    [Tooltip("라이트 켜지는 시간(초). Emission보다 짧게 추천")]
    public float lightDuration = 0.04f;

    [Tooltip("라이트 강도 배수(원래 intensity 기준)")]
    public float lightIntensityMul = 5f;

    [Tooltip("라이트 Range 배수(원래 range 기준)")]
    public float lightRangeMul = 0.5f;

    MaterialPropertyBlock mpb;
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    Coroutine routine;
    bool subscribed;

    float baseLightIntensity;
    float baseLightRange;
    bool baseLightEnabled;

    void Awake()
    {
        if (gunFire == null) gunFire = GetComponentInParent<GunFire>();

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        if (flashLight == null)
        {
            // 총 오브젝트 근처에 있는 라이트를 자동으로 찾기
            flashLight = GetComponentInChildren<Light>(true);
            if (flashLight == null) flashLight = GetComponentInParent<Light>(true);
        }

        mpb = new MaterialPropertyBlock();
        ApplyEmission(Color.black);

        CacheLightBase();
        SetLightFlash(0f); // 시작 시 라이트 플래시 OFF
    }

    void OnEnable()
    {
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
        if (routine != null) StopCoroutine(routine);
        routine = null;

        ApplyEmission(Color.black);
        RestoreLightBase();
    }

    void Subscribe()
    {
        if (subscribed) return;
        if (gunFire == null) return;

        gunFire.OnFired += Flash;
        subscribed = true;
    }

    void Unsubscribe()
    {
        if (!subscribed) return;
        if (gunFire == null) return;

        gunFire.OnFired -= Flash;
        subscribed = false;
    }

    public void Flash()
    {
        if (!isActiveAndEnabled) return;
        if (renderers == null || renderers.Length == 0) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        // Emission + Light를 같이 켬
        if (useFade)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * fadeSpeed;
                float k = Mathf.Clamp01(t);
                ApplyEmission(Color.Lerp(Color.black, flashColor, k));
                yield return null;
            }
        }
        else
        {
            ApplyEmission(flashColor);
        }

        // 라이트 ON
        SetLightFlash(1f);

        // 유지
        float hold = Mathf.Max(flashDuration, 0f);
        if (hold > 0f) yield return new WaitForSeconds(hold);

        // 라이트 OFF (Emission보다 짧게 꺼지는 느낌)
        if (lightDuration > 0f)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, lightDuration - hold));
        }
        SetLightFlash(0f);

        // Emission OFF
        if (useFade)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * fadeSpeed;
                float k = Mathf.Clamp01(t);
                ApplyEmission(Color.Lerp(flashColor, Color.black, k));
                yield return null;
            }
        }
        ApplyEmission(Color.black);
    }

    void ApplyEmission(Color emission)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (!r) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionColorId, emission);
            r.SetPropertyBlock(mpb);
        }
    }

    void CacheLightBase()
    {
        if (flashLight == null) return;
        baseLightIntensity = flashLight.intensity;
        baseLightRange = flashLight.range;
        baseLightEnabled = flashLight.enabled;
    }

    void RestoreLightBase()
    {
        if (flashLight == null) return;
        flashLight.intensity = baseLightIntensity;
        flashLight.range = baseLightRange;
        flashLight.enabled = baseLightEnabled;
    }

    // amount: 0(OFF) ~ 1(ON)
    void SetLightFlash(float amount)
    {
        if (flashLight == null) return;

        if (amount <= 0f)
        {
            // 끔
            flashLight.intensity = baseLightIntensity;
            flashLight.range = baseLightRange;
            flashLight.enabled = baseLightEnabled; // 원래 상태로
            return;
        }

        // 켬
        flashLight.enabled = true;
        flashLight.intensity = baseLightIntensity * lightIntensityMul;
        flashLight.range = baseLightRange * lightRangeMul;
        flashLight.color = Color.yellow;
    }
}
