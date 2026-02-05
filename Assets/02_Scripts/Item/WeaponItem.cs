using UnityEngine;

[CreateAssetMenu(menuName = "Items/WeaponItem")]
public class WeaponItem : ScriptableObject
{
    [Header("손 장착 옵션")]
    public Vector3 heldLocalScale = Vector3.one;   // 기본 1,1,1
    public Vector3 heldLocalPos = Vector3.zero;    // 필요하면 미세조정
    public Vector3 heldLocalEuler = Vector3.zero;  // 필요하면 회전 보정
    public Sprite icon;
    public GameObject weaponPrefab;   // (선택) 손에 들 프리팹
    public GameObject pickupPrefab;   // (선택) 바닥 드랍 프리팹
    public string itemId; // 예: "NoiseBomb", "NoiseBomb"
}
