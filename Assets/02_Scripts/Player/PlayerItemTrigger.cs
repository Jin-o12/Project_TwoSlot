using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerItemTrigger : MonoBehaviour
{
    [Header("아이템 슬롯 2칸")]
    public string slot1 = "";
    public string slot2 = "";

    [Header("슬롯 고르기")]
    [Range(1, 2)]
    public int selectedSlot = 1;

    [System.Serializable]
    public class ItemDropEntry
    {
        public string itemId;      // 예: "EnergyDrink"
        public GameObject prefab;  // 해당 아이템 프리팹
    }

    [Header("드롭 테이블 (itemId -> prefab)")]
    public ItemDropEntry[] dropTable;

    [Header("드롭 위치(선택)")]
    public Transform dropPoint;

    public bool IsFull => !string.IsNullOrEmpty(slot1) && !string.IsNullOrEmpty(slot2);
    public bool Has(string itemId) => slot1 == itemId || slot2 == itemId;

    public string GetSelectedItem() => selectedSlot == 1 ? slot1 : slot2;

    public void SelectSlot(int slotIndex)
    {
        selectedSlot = (slotIndex == 2) ? 2 : 1;
    }

    public void AddOrReplaceSelected(string newItemId)
    {
        if (string.IsNullOrEmpty(newItemId)) return;

        // 빈 슬롯이 있으면 먼저 채움
        if (string.IsNullOrEmpty(slot1)) { slot1 = newItemId; return; }
        if (string.IsNullOrEmpty(slot2)) { slot2 = newItemId; return; }

        // 둘 다 차 있으면 선택 슬롯 교체
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

        // 드롭된 프리팹에도 ItemPickUpTrigger가 있다면 itemId 갱신
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
