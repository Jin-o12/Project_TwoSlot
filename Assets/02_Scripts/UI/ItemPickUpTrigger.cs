using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickUpTrigger : MonoBehaviour
{
    [Header("Item (WeaponItem)")]
    public WeaponItem item;

    [Header("UI")]
    public bool showFPrompt = true;
    public string promptFormat = "F : 줍기 ({0})";

    [Header("옵션")]
    public bool destroyOnPickup = true;

    [Header("Input Limit")]
    [Tooltip("F 입력 허용 최소 간격(초). 0.5면 1초에 최대 2번.")]
    public float fCooldown = 0.5f;

    bool _playerIn;
    PlayerItemTrigger _inv;

    float _nextAllowedTime = 0f;
    bool _picked; // ✅ 한 번 주웠으면 더 이상 처리 안 함

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_picked) return;
        if (!other.CompareTag("Player")) return;

        _playerIn = true;

        _inv = other.GetComponent<PlayerItemTrigger>()
               ?? other.GetComponentInParent<PlayerItemTrigger>();

        if (showFPrompt && FPromptUI.I && _inv != null)
            FPromptUI.I.Show(transform);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerIn = false;
        _inv = null;

        if (showFPrompt && FPromptUI.I && FPromptUI.I.IsShowing(transform))
            FPromptUI.I.Hide();
    }

    void Update()
    {
        if (_picked) return;
        if (!_playerIn || _inv == null) return;

        // ✅ 쿨다운: 0.5초면 1초에 최대 2번 입력만 허용
        if (Time.time < _nextAllowedTime) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            _nextAllowedTime = Time.time + fCooldown;

            if (item == null)
            {
                Debug.LogWarning($"[Pickup] '{name}'에 item(WeaponItem)이 할당되지 않았어!");
                return;
            }

            // ✅ 중복 방지 락
            _picked = true;

            _inv.AddOrReplaceSelected(item);

            if (showFPrompt && FPromptUI.I && FPromptUI.I.IsShowing(transform))
                FPromptUI.I.Hide();

            // Destroy 전에 트리거/스크립트 비활성화(안전장치)
            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
            enabled = false;

            if (destroyOnPickup) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
