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
    public Inventory2Slots inventory;
    public AimAndFlip aimAndFlip;
    public RigBuilder rigBuilder;
    public Rig rig;

    [Header("Anim")]
    public string hitTrigger = "Hit";
    public string dieTrigger = "Die";

    bool isDead;

    void Awake()
    {
        hp = maxHp;

        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!gunFire) gunFire = GetComponentInChildren<GunFire>();
        if (!playerMove) playerMove = GetComponent<PlayerMove>();
        if (!inventory) inventory = GetComponent<Inventory2Slots>();
        if (!rigBuilder) rigBuilder = GetComponent<RigBuilder>();

        UpdateHPUI();
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        hp = Mathf.Clamp(hp - dmg, 0, maxHp);
        UpdateHPUI();

        if (hp <= 0)
        {
            Die();
            return;
        }

        // ✅ Hit 애니
        if (animator && !string.IsNullOrEmpty(hitTrigger))
            animator.SetTrigger(hitTrigger);

        // ✅ 0.5초 발사 금지
        gunFire?.LockFire(0.5f);
    }

    void UpdateHPUI()
    {
        if (hpFill) hpFill.fillAmount = (float)hp / maxHp;
        if (hpText) hpText.text = $"{hp}/{maxHp}";
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // ✅ 에임/IK 정지
        if (aimAndFlip) aimAndFlip.IsDead = true;
        if (rig) rig.weight = 0f;
        if (rigBuilder) rigBuilder.enabled = false;

        // ✅ 입력/전투/인벤 중단
        if (gunFire) gunFire.enabled = false;
        if (inventory) inventory.enabled = false;

        // ✅ 이동/사운드 중단
        if (playerMove)
        {
            playerMove.enabled = false;
            if (playerMove.audioSource)
                playerMove.audioSource.Stop();
        }

        // ✅ 죽음 애니
        if (animator)
            animator.SetTrigger(dieTrigger);
    }
}

