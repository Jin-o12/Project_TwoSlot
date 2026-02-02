using UnityEngine;
using UnityEngine.UI;

public class ItemUIManager : MonoBehaviour
{
    public PlayerItemTrigger inv;

    public Image slot1Icon;
    public Image slot2Icon;

    public Sprite energyDrinkSprite;
    public Sprite soundBombSprite;
    public Sprite glowgunSprite;

    void Awake()
    {
        if (inv == null)
            inv = FindFirstObjectByType<PlayerItemTrigger>();
    }

    void Update()
    {
        if (inv == null) return;

        SetIcon(slot1Icon, inv.slot1);
        SetIcon(slot2Icon, inv.slot2);
    }

    void SetIcon(Image img, string itemIdRaw)
    {
        if (img == null) return;

        string itemId = NormalizeId(itemIdRaw);

        if (string.IsNullOrEmpty(itemId))
        {
            img.gameObject.SetActive(false);
            return;
        }

        img.sprite = GetSprite(itemId);
        img.gameObject.SetActive(img.sprite != null);
    }

    Sprite GetSprite(string id)
    {
        switch (id)
        {
            case "EnergyDrink": return energyDrinkSprite;
            case "SoundBomb": return soundBombSprite;
            case "Glowgun": return glowgunSprite;
        }
        return null;
    }

    static string NormalizeId(string id)
    {
        if (string.IsNullOrEmpty(id)) return "";
        id = id.Trim();
        if (id.Length >= 2 && id[0] == '"' && id[id.Length - 1] == '"')
            id = id.Substring(1, id.Length - 2);
        return id.Trim();
    }
}
