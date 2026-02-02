/// <summary>
/// 플레이어의 인벤토리에 관한 기능을 총괄함.
/// 아이템 습득, 떨어트리기, 사용하기 위한 선택 과정을 모두 관리
/// </summary>
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemTrigger : MonoBehaviour
{
    private InputManager inputManager;

    [Header("아이템 슬롯 2칸 (WeaponItem)")]
    public WeaponItem slot1;                        // 첫번째 아이템이 들어갈 번수
    public WeaponItem slot2;                        // 두번째 아이템이 들어갈 변수

    [Header("슬롯 아이콘 UI (Image)")]
    public Image slotIcon;                          // 아이템 슬롯 이미지

    [Header("아이템 슬롯(1/2)")]
    [Range(1, 2)]
    public int selectedSlot;                        // 선택 된 슬롯

    [Header("드롭 위치(선택)")]
    public Transform dropPoint;                     // 아이템이 떨어질 위치

    [Header("현재 활성 모드")]
    public ActiveMode activeMode;                   // 현재 활성화 된 아이템 모드

    [Header("아이템 선택중(하이라이트만)")]
    public bool selectingItem = false;              // 아이템을 선택 중인지에 대한 여부

    public enum ActiveMode { Gun, Item }            // 아이템 활성화 여부: 총 or 아이템(선택 슬롯)

    // ====== 편의 프로퍼티 ======
    public bool IsFull => slot1 != null && slot2 != null;
    public bool Has(WeaponItem item) => item != null && (slot1 == item || slot2 == item);
    public WeaponItem GetSelectedItem() => selectedSlot == 1 ? slot1 : slot2;

    // 총을 쏴도 되는지(총 스크립트에서 이걸로 막으면 “통합” 완성)
    public bool CanFireGun => activeMode == ActiveMode.Gun;

    private void Awake()
    {
        inputManager = InputManager.Instance;
    }
    void Start()
    {
        RefreshUI();
    }

    /* 인벤토리 관련 기능 초기화 */
    public void Initialized()
    {
        activeMode = ActiveMode.Gun;
        selectedSlot = 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(inputManager.Slot1Key)) HandleSlotKey(1);
        else if (Input.GetKeyDown(inputManager.Slot2Key)) HandleSlotKey(2);
    }

    void HandleSlotKey(int slotIndex)
    {
        // ✅ 아이템 모드에서 같은 슬롯 키를 누르면 총 모드로 토글 복귀
        if (activeMode == ActiveMode.Item && !selectingItem && selectedSlot == slotIndex)
        {
            SwitchToGun();
            return;
        }

        // 1) 아직 아이템 선택중이 아니면: 선택 모드(하이라이트) 진입
        if (!selectingItem)
        {
            selectingItem = true;
            selectedSlot = slotIndex;
            Debug.Log($"[ItemSelect] 슬롯 {selectedSlot} 선택됨 (한 번 더 누르면 전환/토글)");
            return;
        }

        // 2) 선택중인데 다른 슬롯 키면: 하이라이트 이동
        if (selectedSlot != slotIndex)
        {
            selectedSlot = slotIndex;
            Debug.Log($"[ItemSelect] 슬롯 {selectedSlot}로 이동 (한 번 더 누르면 전환/토글)");
            return;
        }

        // 3) 같은 키를 한 번 더 눌렀다 = 확정(아이템 모드로 전환)
        if (GetSelectedItem() == null)
        {
            Debug.Log("[ItemSelect] 슬롯이 비어있어서 전환 불가");
            return;
        }

        activeMode = ActiveMode.Item;
        selectingItem = false;
        Debug.Log($"[ItemSelect] ✅ 아이템 모드로 전환! (슬롯 {selectedSlot})");
    }

    // 아이템 사용 후 자동으로 총로 돌아가고 싶을 때 호출
    public void SwitchToGun()
    {
        activeMode = ActiveMode.Gun;
        selectingItem = false;
        Debug.Log("[ItemSelect] 총 모드로 복귀");
    }

    // ================== 인벤/드롭 로직 ==================

    public void AddOrReplaceSelected(WeaponItem newItem)
    {
        if (newItem == null) return;

        if (slot1 == null)
        {
            slot1 = newItem;
            RefreshUI();
            return;
        }

        if (slot2 == null)
        {
            slot2 = newItem;
            RefreshUI();
            return;
        }

        // 둘 다 차있으면 선택 슬롯 교체(기존 것은 드랍)
        if (selectedSlot == 1)
        {
            Drop(slot1);
            slot1 = newItem;
        }
        else
        {
            Drop(slot2);
            slot2 = newItem;
        }

        RefreshUI();
    }

    // 소비(사용) 시 슬롯 비우기
    public bool TryConsume(WeaponItem item)
    {
        if (item == null) return false;

        if (slot1 == item)
        {
            slot1 = null;
            RefreshUI();
            return true;
        }

        if (slot2 == item)
        {
            slot2 = null;
            RefreshUI();
            return true;
        }

        return false;
    }

    void Drop(WeaponItem item)
    {
        if (item == null) return;

        if (item.pickupPrefab == null)
        {
            Debug.LogWarning($"[Drop] '{item.name}' pickupPrefab이 없어서 드랍하지 않음.");
            return;
        }

        Vector3 pos = dropPoint
            ? dropPoint.position
            : (transform.position + transform.forward * 0.8f + Vector3.up * 0.2f);

        Instantiate(item.pickupPrefab, pos, Quaternion.identity);
    }

    void RefreshUI()
    {
        SetSlotIcon(slotIcon, slot1);
        SetSlotIcon(slotIcon, slot2);
    }

    void SetSlotIcon(Image img, WeaponItem item)
    {
        if (img == null) return;

        if (item == null || item.icon == null)
        {
            img.enabled = false;
            img.sprite = null;
            return;
        }

        img.enabled = true;
        img.sprite = item.icon;
    }
}
