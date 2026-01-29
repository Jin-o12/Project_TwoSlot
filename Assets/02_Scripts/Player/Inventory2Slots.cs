using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory2Slots : MonoBehaviour
{
    [Header("UI")]
    public Image slot1Icon, slot2Icon;

    [Header("Equip")]
    public Transform handMount;
    public WeaponItem startEquipped;

    WeaponItem slot1, slot2, equippedItem;
    GameObject equippedGO;

    void Start()
    {
        // 아이콘은 enabled 건드리지 말고(꼬임 방지) 알파로만 숨김/표시
        if (slot1Icon) slot1Icon.enabled = true;
        if (slot2Icon) slot2Icon.enabled = true;
        Equip(startEquipped);
        RefreshUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) Swap(ref slot1);
        if (Input.GetKeyDown(KeyCode.E)) Swap(ref slot2);
    }

    // F 픽업 규칙:
    // 1) Slot1 비면 Slot1
    // 2) Slot1 차면 Slot2
    // 3) 둘 다 차면 손(장착)과 교체 (기존 손 무기는 드랍)
    public void Pickup(WeaponItem newItem, Vector3 pickupPos)
    {
        if (!slot1) { slot1 = newItem; RefreshUI(); return; }
        if (!slot2) { slot2 = newItem; RefreshUI(); return; }

        DropEquipped(pickupPos);
        Equip(newItem);
        RefreshUI();
    }

    void Swap(ref WeaponItem slot)
    {
        if (!slot && !equippedItem) return;

        (slot, equippedItem) = (equippedItem, slot);
        Equip(equippedItem);
        RefreshUI();
    }

    void Equip(WeaponItem item)
    {
        equippedItem = item;
        
        if (equippedGO) Destroy(equippedGO);
        equippedGO = null;

        equippedItem = item;
        if (!item || !item.weaponPrefab || !handMount) return;

        equippedGO = Instantiate(item.weaponPrefab, handMount);
        equippedGO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    void DropEquipped(Vector3 pos)
    {
        if (!equippedItem) return;

        if (equippedItem.pickupPrefab)
            Instantiate(equippedItem.pickupPrefab, pos, Quaternion.identity);

        equippedItem = null;
        if (equippedGO) Destroy(equippedGO);
        equippedGO = null;
    }

    void RefreshUI()
    {
        SetIcon(slot1Icon, slot1);
        SetIcon(slot2Icon, slot2);
    }

    void SetIcon(Image img, WeaponItem item)
    {
        if (!img) return;
        
        img.enabled = true;
        img.sprite = item ? item.icon : null;

        var c = img.color;
        c.a = (item && item.icon) ? 1f : 0f; // 알파로 숨김/표시
        img.color = c;
    }
}