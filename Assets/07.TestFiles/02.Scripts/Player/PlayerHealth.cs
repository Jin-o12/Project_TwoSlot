using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
     [Header("HP")]
    public int maxHp = 100;
    public int hp;

    [Header("UI")]
    public Image hpFill;
    public Text hpText;

    [Header("Refs")]
    public Animator animator;
    public PlayerMove playerMove;
    public GunFire gunFire;
    public AimAndFlip aimAndFlip;
    public RigBuilder rigBuilder;
    public Rig rig; // Rig Layers에 들어있는 그 Rig(1개면 1개만)

    [Header("Anim")]
    public string dieTrigger = "Die";
    public string hitTrigger = "Hit"; // 있으면 쓰고, 없으면 비워도 됨

    bool isDead;

    void Awake()
    {
        hp = maxHp;
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rigBuilder) rigBuilder = GetComponent<RigBuilder>();
        if (!playerMove) playerMove = GetComponent<PlayerMove>();
        UpdateHPUI();
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        hp = Mathf.Clamp(hp - dmg, 0, maxHp);
        
        UpdateHPUI();

        if (hp <= 0) Die();
        else if (animator && !string.IsNullOrEmpty(hitTrigger))
            animator.SetTrigger(hitTrigger);
        
    }
    void UpdateHPUI()
    {
        hpFill.fillAmount = (float)hp / (float)maxHp;
        if (hpText) hpText.text = $"{hp}/{maxHp}";
    }
    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1) 에임/IK 즉시 중단 (마지막 마우스 포즈 방지)
        if (aimAndFlip) aimAndFlip.IsDead = true;

        // 2) Animation Rigging 완전 종료
        if (rig) rig.weight = 0f;
        if (rigBuilder) rigBuilder.enabled = false;

        // 3) 입력/발사/이동 중단
        if (gunFire) gunFire.enabled = false;
        if (playerMove) 
        {
            playerMove.enabled = false;
            playerMove.audioSource.Stop();
        }
        // 4) 죽는 애니만 실행
        if (animator) animator.SetTrigger(dieTrigger);
    }
}
