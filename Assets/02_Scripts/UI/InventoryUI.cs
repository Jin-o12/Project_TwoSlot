using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Image slot1Icon;

    void Awake()
    {
        if (slot1Icon) slot1Icon.enabled = false; // 처음엔 아이콘 숨김(원하면 삭제)
    }

    public void SetSlot1(Sprite icon)
    {
        if (!slot1Icon) return;
        slot1Icon.sprite = icon;
        slot1Icon.enabled = (icon != null);
    }
}
