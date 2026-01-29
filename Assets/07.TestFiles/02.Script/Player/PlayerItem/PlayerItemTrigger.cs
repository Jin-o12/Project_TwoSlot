using UnityEngine;

public class PlayerItemTrigger : MonoBehaviour
{
    [Header("아이템 슬롯 2칸")]
    public string slot1 = "";
    public string slot2 = "";

    [Header("아이템 슬롯(1/2)")]
    [Range(1, 2)]
    public int selectedSlot = 1;

    [System.Serializable]
    public class ItemDropEntry
    {
        public string itemId;
        public GameObject prefab;
    }

    [Header("드롭 테이블 (itemId -> prefab)")]
    public ItemDropEntry[] dropTable;

    [Header("드롭 위치(선택)")]
    public Transform dropPoint;

    // ====== 여기부터 핵심: 총/아이템 전환 상태 ======
    public enum ActiveMode { Gun, Item } // 총 or 아이템(선택 슬롯)
    [Header("현재 활성 모드")]
    public ActiveMode activeMode = ActiveMode.Gun;

    [Header("아이템 선택중(하이라이트만)")]
    public bool selectingItem = false;

    [Header("키")]
    public KeyCode slot1Key = KeyCode.Q;
    public KeyCode slot2Key = KeyCode.E;

    public bool IsFull => !string.IsNullOrEmpty(slot1) && !string.IsNullOrEmpty(slot2);
    public bool Has(string itemId) => slot1 == itemId || slot2 == itemId;

    public string GetSelectedItem() => selectedSlot == 1 ? slot1 : slot2;

    // 총을 쏴도 되는지(총 스크립트에서 이거로 막아주면 “통합” 완성)
    public bool CanFireGun => activeMode == ActiveMode.Gun;

    void Update()
    {
        // Q: 슬롯1 선택/확정
        if (Input.GetKeyDown(slot1Key))
            HandleSlotKey(1);

        // E: 슬롯2 선택/확정
        if (Input.GetKeyDown(slot2Key))
            HandleSlotKey(2);
    }

    void HandleSlotKey(int slotIndex)
    {
        // 1) 아직 아이템 선택중이 아니면: 선택 모드로 들어가고 슬롯만 하이라이트
        if (!selectingItem)
        {
            selectingItem = true;
            selectedSlot = slotIndex;
            Debug.Log($"[ItemSelect] 슬롯 {selectedSlot} 선택됨 (한 번 더 누르면 전환)");
            return;
        }

        // 2) 이미 선택중일 때
        if (selectedSlot != slotIndex)
        {
            // 다른 슬롯으로 하이라이트 이동
            selectedSlot = slotIndex;
            Debug.Log($"[ItemSelect] 슬롯 {selectedSlot}로 이동 (한 번 더 누르면 전환)");
            return;
        }

        // 3) 같은 키를 한 번 더 눌렀다 = 확정(아이템 모드로 전환)
        if (string.IsNullOrEmpty(GetSelectedItem()))
        {
            // 빈 슬롯이면 전환하지 않고, 그냥 선택만 유지
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

    // ================== 인벤/드롭 기존 로직 ==================

    public void AddOrReplaceSelected(string newItemId)
    {
        if (string.IsNullOrEmpty(newItemId)) return;

        if (string.IsNullOrEmpty(slot1)) { slot1 = newItemId; return; }
        if (string.IsNullOrEmpty(slot2)) { slot2 = newItemId; return; }

        if (selectedSlot == 1)
        {
            Drop(slot1);
            slot1 = newItemId;
        }
        else
        {
            Drop(slot2);
            slot2 = newItemId;
        }
    }

    public bool TryConsume(string itemId)
    {
        if (slot1 == itemId) { slot1 = ""; return true; }
        if (slot2 == itemId) { slot2 = ""; return true; }
        return false;
    }

    void Drop(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        GameObject prefab = GetDropPrefab(itemId);
        if (prefab == null)
        {
            Debug.LogWarning($"[Drop] dropTable에 '{itemId}' 프리팹이 등록되지 않았어.");
            return;
        }

        Vector3 pos = dropPoint ? dropPoint.position : (transform.position + transform.forward * 0.8f + Vector3.up * 0.2f);
        var go = Instantiate(prefab, pos, Quaternion.identity);

        var pickup = go.GetComponent<ItemPickUpTrigger>();
        if (pickup != null) pickup.itemId = itemId;
    }

    GameObject GetDropPrefab(string itemId)
    {
        if (dropTable == null) return null;

        for (int i = 0; i < dropTable.Length; i++)
        {
            if (dropTable[i] != null && dropTable[i].itemId == itemId)
                return dropTable[i].prefab;
        }
        return null;
    }
}
