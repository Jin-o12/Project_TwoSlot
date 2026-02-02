using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickUpTrigger : MonoBehaviour
{
    [Header("Item (WeaponItem)")]
    public WeaponItem item;  // ✅ 프리팹마다 다른 WeaponItem 에셋 넣기

    [Header("UI")]
    public bool showFPrompt = true;
    public string promptFormat = "F : 줍기 ({0})";

    [Header("옵션")]
    public bool destroyOnPickup = true;

    bool _playerIn;
    PlayerItemTrigger _inv;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerIn = true;

        _inv = other.GetComponent<PlayerItemTrigger>()
               ?? other.GetComponentInParent<PlayerItemTrigger>();

        if (showFPrompt && FPromptUI.I && _inv != null)
            FPromptUI.I.Show(transform);
        
        // 인벤토리가 있다면 UI 표시
        //if (_inv != null)
            //PickupUI.Instance?.Show(string.Format(promptFormat, itemId));

    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerIn = false;
        _inv = null;

        if (showFPrompt && FPromptUI.I && FPromptUI.I.IsShowing(transform))
            FPromptUI.I.Hide();

        // UI 숨기기
        //PickupUI.Instance?.Hide();
    }

    void Update()
    {
        if (!_playerIn || _inv == null) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (item == null)
            {
                Debug.LogWarning($"[Pickup] '{name}'에 item(WeaponItem)이 할당되지 않았어!");
                return;
            }

            // ✅ 인벤에 아이템 추가
            _inv.AddOrReplaceSelected(item);

            if (showFPrompt && FPromptUI.I && FPromptUI.I.IsShowing(transform))
                FPromptUI.I.Hide();

            // UI 숨기기
            // PickupUI.Instance?.Hide();

            if (destroyOnPickup) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
