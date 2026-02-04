using UnityEngine;

public class PlayerPotionUse : MonoBehaviour
{
    public PlayerItemTrigger inv;
    public PlayerHealth hp;   // ✅ PlayerHP → PlayerHealth로 변경

    [Header("Energy Drink (WeaponItem)")]
    public WeaponItem energyDrinkItem;
    public int healAmount = 25;
    public AudioClip openSound;

    AudioSource audioSource;

    void Awake()
    {
        if (!inv) inv = GetComponent<PlayerItemTrigger>();

        // ✅ PlayerHealth 찾기
        if (!hp)
            hp = GetComponent<PlayerHealth>() ?? GetComponentInParent<PlayerHealth>();

        // ✅ AudioSource 캐싱 (PlayClipAtPoint보다 성능 좋음)
        audioSource = GetComponentInParent<AudioSource>();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (!inv || !hp) return;

        // 아이템 모드일 때만
        if (inv.activeMode != PlayerItemTrigger.ActiveMode.Item) return;

        WeaponItem selected = inv.GetSelectedItem();
        if (!selected) return;

        if (!energyDrinkItem)
        {
            Debug.LogWarning("[EnergyDrink] energyDrinkItem이 비어있음!");
            return;
        }

        if (selected != energyDrinkItem) return;

        // ✅ 풀피면 사용 막기 (강력 추천)
        if (hp.currentHp >= hp.maxHp) return;

        //-------------------
        // 사용
        //-------------------

        hp.Heal(healAmount);

        if (audioSource && openSound)
            audioSource.PlayOneShot(openSound);

        //-------------------
        // 소비 + 총 복귀
        //-------------------

        inv.TryConsume(selected);
        inv.SwitchToGun();
    }
}
