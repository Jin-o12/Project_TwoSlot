using UnityEngine;

public class PlayerPotionUse : MonoBehaviour
{
    public PlayerItemTrigger inv;
    public PlayerHP hp;

    [Header("Energy Drink")]
    public string energyDrinkId = "EnergyDrink";
    public float healAmount = 25f;

    void Awake()
    {
        if (inv == null) inv = GetComponent<PlayerItemTrigger>();
        if (hp == null) hp = GetComponent<PlayerHP>() ?? GetComponentInParent<PlayerHP>();
    }

    void Update()
    {
        // ✅ 좌클릭 입력 확인(디버그)
        if (Input.GetMouseButtonDown(0))
            Debug.Log($"[Click] 감지됨 / mode={inv?.activeMode} / item={inv?.GetSelectedItem()}");

        if (!Input.GetMouseButtonDown(0)) return;
        if (inv == null || hp == null) return;

        // 아이템 모드일 때만 발동
        if (inv.activeMode != PlayerItemTrigger.ActiveMode.Item) return;

        // 에너지 드링크만
        if (inv.GetSelectedItem() != energyDrinkId) return;

        // ✅ 체력 상관없이 사용
        hp.Heal(healAmount);
        inv.TryConsume(energyDrinkId);

        // 사용 후 총로 복귀
        inv.SwitchToGun();

        Debug.Log("[EnergyDrink] 사용됨 + 소비됨 + 총로 복귀");
    }
}
