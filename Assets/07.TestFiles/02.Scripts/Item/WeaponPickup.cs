using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//아이템 프리팹에 붙이기 (Collider(Trigger)) 필수
//Project - 03.Resources - SlotItem에 WeaponItem 할당
public class WeaponPickup : MonoBehaviour
{
    public WeaponItem item;

    Inventory2Slots inv;
    bool inRange;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        inv = other.GetComponentInParent<Inventory2Slots>();
        inRange = (inv != null);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        inv = null;
        inRange = false;
    }

    void Update()
    {
        if (!inRange) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            inv.Pickup(item, transform.position);
            Destroy(gameObject);
        }
    }
    
}
