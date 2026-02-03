using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdditionalAmmo : MonoBehaviour
{
    public int amount = 10;
    bool canPickup;
    Transform player;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        canPickup = true;
        player = other.transform;

        if (FPromptUI.I) FPromptUI.I.Show(transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        canPickup = false;
        player = null;
        
        if (FPromptUI.I && FPromptUI.I.IsShowing(transform))
            FPromptUI.I.Hide();
    }

    private void Update()
    {
        if (!canPickup || player == null) return;

        // F키로 습득 통일
        if (Input.GetKeyDown(KeyCode.F))
        {
            var gun = player.GetComponentInChildren<GunFire>();
            if (gun == null) return;
            
            gun.AddMaxAmmo(amount); //  예비탄 +10
            if (FPromptUI.I && FPromptUI.I.IsShowing(transform)) FPromptUI.I.Hide();
            Destroy(gameObject);          //  먹자마자 삭제
            
        }
    }
}
