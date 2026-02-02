using UnityEngine;

public class PlayerPotionUse : MonoBehaviour
{
    public PlayerItemTrigger inv;
    public PlayerHP hp;

    [Header("Energy Drink (WeaponItem)")]
    public WeaponItem energyDrinkItem;   // ✅ 인스펙터에 EnergyDrink WeaponItem 에셋 넣기
    public float healAmount = 25f;

    void Awake()
    {
        if (inv == null) inv = GetComponent<PlayerItemTrigger>();
        if (hp == null) hp = GetComponent<PlayerHP>() ?? GetComponentInParent<PlayerHP>();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (inv == null || hp == null) return;

        // 아이템 모드일 때만 발동
        if (inv.activeMode != PlayerItemTrigger.ActiveMode.Item) return;

        // 선택 아이템
        WeaponItem selected = inv.GetSelectedItem();
        if (selected == null) return;

        // 에너지 드링크만
        if (energyDrinkItem == null)
        {
            Debug.LogWarning("[EnergyDrink] energyDrinkItem(WeaponItem)이 비어있어!");
            return;
        }
        if (selected != energyDrinkItem) return;

        // 사용
        hp.Heal(healAmount);

        // 소비
        inv.TryConsume(selected);

        // 총로 복귀
        inv.SwitchToGun();

        Debug.Log("[EnergyDrink] 사용됨 + 소비됨 + 총로 복귀");
    }
}
