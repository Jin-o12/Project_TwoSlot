using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    [Header("# UI Components")]
    public Image hpBarImage;
    public Text hpText;

    [Header("# Player Stats")]
    public float maxHP = 100f;
    public float currentHP;

    void Start()
    {
        currentHP = maxHP;
        // 시작할 때 UI를 꽉 채워둠
        hpBarImage.fillAmount = 1f;
        UpdateText(maxHP);
    }

    void Update()
    {
        // [테스트] 스페이스바 누르면 데미지 , R 누르면 회복
        if (Input.GetKeyDown(KeyCode.Space)) TakeDamage(10f);
        if (Input.GetKeyDown(KeyCode.R)) Heal(10f);


        HandleHPAnimation();
    }

    public void TakeDamage(float damage)
    {

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;


    }

    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
    }


    void HandleHPAnimation()
    {

        float targetFill = currentHP / maxHP;


        if (hpBarImage.fillAmount != targetFill)
        {

            hpBarImage.fillAmount = Mathf.Lerp(hpBarImage.fillAmount, targetFill, Time.deltaTime * 5f);


            float currentVisualHP = hpBarImage.fillAmount * maxHP;
            UpdateText(currentVisualHP);
        }
    }

    void UpdateText(float displayValue)
    {
        if (hpText != null)
        {

            hpText.text = $"{Mathf.RoundToInt(displayValue)} / {maxHP}";
        }
    }
}