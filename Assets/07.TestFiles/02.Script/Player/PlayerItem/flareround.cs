using UnityEngine;

public class PlayerItemUse : MonoBehaviour
{
    public PlayerItemTrigger inv;

    [Header("Use Key")]
    public KeyCode useKey = KeyCode.Mouse0; // 좌클릭 (원하면 F/우클릭으로 바꿔)

    [Header("FlareGun 1회용")]
    public string flareGunItemId = "FlareGun";
    public OneShotFlareGun heldFlareGun;    // 손에 들고 있는 플레어건(모델) 오브젝트에 붙어있는 스크립트

    void Awake()
    {
        if (inv == null) inv = GetComponent<PlayerItemTrigger>();
    }

    void Update()
    {
        if (!Input.GetKeyDown(useKey)) return;
        if (inv == null) return;

        string selected = inv.GetSelectedItem();
        if (selected != flareGunItemId) return;

        // 1) 발사
        if (heldFlareGun != null)
            heldFlareGun.FireOnce();

        // 2) 아이템 소비(슬롯에서 제거)
        inv.TryConsume(flareGunItemId);

        // 3) 한발쏘고나면 플레어건 자체가 사라짐
		
        if (heldFlareGun != null)
            heldFlareGun.gameObject.SetActive(false);
    }
}
