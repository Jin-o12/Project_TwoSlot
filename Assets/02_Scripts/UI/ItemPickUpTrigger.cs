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

        if (showFPrompt && FPromptUI.I && _inv != null && !IsSameAsEquipped())
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

        // 손에 든 아이템과 동일하면: 줍기 막고, UI도 숨김
        if (IsSameAsEquipped())
        {
            if (showFPrompt && FPromptUI.I && FPromptUI.I.IsShowing(transform))
                FPromptUI.I.Hide();
            return;
        }

        // 동일 아이템이 아닌 경우: UI가 꺼져있으면 다시 켜기(선택)
        if (showFPrompt && FPromptUI.I && !FPromptUI.I.IsShowing(transform))
            FPromptUI.I.Show(transform);


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
    bool IsSameAsEquipped()
    {
        if (_inv == null) return false;
        if (_inv.EquippedItem == null) return false;
        if (item == null) return false;

        // 1) 같은 ScriptableObject 참조면 같은 아이템
        if (_inv.EquippedItem == item) return true;

        // 2) (선택) ID가 있으면 ID 비교로도 막기
        // return _inv.EquippedItem.itemId == item.itemId;

        return false;
    }
}
