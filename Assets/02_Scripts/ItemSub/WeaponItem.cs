using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/WeaponItem")]
public class WeaponItem : ScriptableObject
{
    public Sprite icon;
    public GameObject weaponPrefab;   // 손에 들 프리팹
    public GameObject pickupPrefab;   // (옵션) 바닥에 떨굴 프리팹(없으면 드랍 안 함)
}
