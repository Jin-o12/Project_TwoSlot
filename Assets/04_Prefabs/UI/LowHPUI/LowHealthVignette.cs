using UnityEngine;
using UnityEngine.UI; // �̹��� �����
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LowHealthEffect : MonoBehaviour
{
    [Header("Settings - Post Processing")]
    public Volume globalVolume;
    public Color vignetteColor = Color.red;

    [Header("Settings - UI Image")]
    public Image bloodOverlayImage; 
    [Range(0f, 1f)] public float maxImageAlpha = 0.8f; // �� �̹����� ���� ���� �� ������

    [Header("Debug / Test")]
    //public bool isTestMode = true;
    [Range(0, 100)] public float testCurrentHealth = 100f;
    public float testMaxHealth = 100f;

    private Vignette _vignette;
    private float _targetVignetteIntensity;
    private float _targetImageAlpha;
    private float _pulseSpeed;
    private float _currentHealthPercent = 1f;

    void Start()
    {
        // ����� �ʱ�ȭ
        if (globalVolume.profile.TryGet(out Vignette vig))
        {
            _vignette = vig;
            _vignette.active = true;
            _vignette.color.value = vignetteColor;
            _vignette.intensity.value = 0f;
        }

        // �̹��� �ʱ�ȭ
        if (bloodOverlayImage != null)
        {
            Color c = bloodOverlayImage.color;
            c.a = 0f;
            bloodOverlayImage.color = c;
        }
    }

    void Update()
    {
        // --- �׽�Ʈ ��� ---
        /*if (isTestMode)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                testCurrentHealth = Mathf.Max(0, testCurrentHealth - 10f);
                UpdateHealthState(testCurrentHealth, testMaxHealth);
                Debug.Log($"[Test] ü��: {testCurrentHealth}%");
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                testCurrentHealth = Mathf.Min(testMaxHealth, testCurrentHealth + 10f);
                UpdateHealthState(testCurrentHealth, testMaxHealth);
                Debug.Log($"[Test] ü��: {testCurrentHealth}%");
            }
        }*/
        // --------------------

        if (_vignette == null) return;

        // ü�� 50% �̻��̸� ȿ�� ����
        if (_currentHealthPercent > 0.5f)
        {
            // ����� ����
            _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, 0f, Time.deltaTime * 2f);

            // �̹��� ����
            if (bloodOverlayImage != null)
            {
                Color c = bloodOverlayImage.color;
                c.a = Mathf.Lerp(c.a, 0f, Time.deltaTime * 2f);
                bloodOverlayImage.color = c;
            }
            return;
        }

        // ���� �ڵ� (Pulse) ���
        // 0.0 ~ 1.0 ���̸� �Դٰ��� ��
        float pulse = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;

        // 1. ����� ���� (�ﷷ�Ÿ�)
        float currentVig = Mathf.Lerp(_targetVignetteIntensity * 0.7f, _targetVignetteIntensity, pulse);
        _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, currentVig, Time.deltaTime * 5f);

        // 2. PNG �̹��� ���� (�ﷷ�Ÿ�)
        if (bloodOverlayImage != null)
        {
            // �̹����� ���� �ڵ��� ���� �������� ���ϰ� �� (�� ������)
            float currentAlpha = Mathf.Lerp(_targetImageAlpha * 0.6f, _targetImageAlpha, pulse);

            Color c = bloodOverlayImage.color;
            c.a = Mathf.Lerp(c.a, currentAlpha, Time.deltaTime * 5f);
            bloodOverlayImage.color = c;
        }
        
    }

    public void UpdateHealthState(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0) return;
        _currentHealthPercent = currentHealth / maxHealth;

        if (_currentHealthPercent <= 0.25f) // 25% ���� (����!)
        {
            _targetVignetteIntensity = 0.45f;
            _targetImageAlpha = maxImageAlpha * 0.4f; // �̹����� ���� ���ϰ�
            _pulseSpeed = 12f;
        }
        else if (_currentHealthPercent <= 0.5f) // 50% ���� (���)
        {
            _targetVignetteIntensity = 0.35f;
            _targetImageAlpha = maxImageAlpha * 0.1f; // �̹����� ��¦�� ����
            _pulseSpeed = 6f;
        }
        else // ����
        {
            _targetVignetteIntensity = 0f;
            _targetImageAlpha = 0f;
            _pulseSpeed = 0f;
        }
    }
}