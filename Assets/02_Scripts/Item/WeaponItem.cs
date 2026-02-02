using UnityEngine;

[CreateAssetMenu(menuName = "Items/WeaponItem")]
public class WeaponItem : ScriptableObject
{
    public Sprite icon;
    public GameObject weaponPrefab;   // (선택) 손에 들 프리팹
    public GameObject pickupPrefab;   // (선택) 바닥 드랍 프리팹
}
