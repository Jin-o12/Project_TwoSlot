using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("컴포넌트 결합")]
    public Image HpBarImage;
    public Text HpText;
    [Header("HP 스탯")]
    public float maxHP = 100f;
    public float currentHP;


    void Start()
    {
        currentHP = maxHP;
        UpdateHP();
    }


    void Update()
    {
        // 테스트용입니다. 스페이스바 : 체력 10깎기, R : 체력 10 회복
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HitDamage(10f);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Heal(10f);
        }
    }
    public void HitDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        UpdateHP();

    }
    public void Heal(float amount)
    {
        currentHP += amount;
        if(currentHP > maxHP)
            currentHP = maxHP;
        UpdateHP();
    }

    void UpdateHP()
    {
        HpBarImage.fillAmount = currentHP / maxHP;
        if (HpText)
        {
            HpText.text = $"{currentHP} / {maxHP}";
        }
    }

}
