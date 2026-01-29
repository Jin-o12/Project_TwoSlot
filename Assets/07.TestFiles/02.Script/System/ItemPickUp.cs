using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public Sprite icon;                 // 획득 시 Slot_1에 넣을 아이콘
    public InventoryUI inventoryUI;     // UI 연결
    public string playerTag = "Player";

    bool inRange;

    void Awake()
    {
        if (!inventoryUI) inventoryUI = FindFirstObjectByType<InventoryUI>();
    }

    void Update()
    {
        if (inRange && Input.GetKeyDown(KeyCode.F))
        {
            inventoryUI?.SetSlot1(icon);
            Destroy(gameObject); // 아이템 사라짐
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) inRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) inRange = false;
    }
}
