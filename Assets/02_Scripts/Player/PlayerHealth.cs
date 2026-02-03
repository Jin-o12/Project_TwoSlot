using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("컴포넌트&스크립트")]
    // 컴포넌트
    private AudioSource audioSource;
    private Animator animator;
    // 스크립트
    private PlayerMove playerMove;
    private Inventory2Slots inventory;
    private AimAndFlip aimAndFlip;
    private RigBuilder rigBuilder;

    [Header("HP")]
    public int maxHp = 100;
    public int hp;

    [Header("UI")]
    public Image hpFill;
    public Text hpText;

    [Header("Refs")]
    public GunFire gunFire;
    public Rig rig;

    [Header("Anim")]
    public string hitTrigger = "Hit";
    public string dieTrigger = "Die";

    bool isDead;

    void Awake()
    {
        // 모든 컴포넌트 및 스크립트를 찾아서 할당
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!gunFire) gunFire = GetComponentInChildren<GunFire>();
        if (!playerMove) playerMove = GetComponent<PlayerMove>();
        if (!inventory) inventory = GetComponent<Inventory2Slots>();
        if (!rigBuilder) rigBuilder = GetComponent<RigBuilder>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    /* 플레이어 체력 수치 초기화 */
    public void Initialized()
    {
        hp = maxHp;
        UpdateHPUI();
    }

    private void Update()
    {
        UpdateHPUI();
    }

    /* 피해를 입음 */
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

        // Hit 애니
        if (animator && !string.IsNullOrEmpty(hitTrigger))
            animator.SetTrigger(hitTrigger);

        // 0.5초 발사 금지
        gunFire?.LockFire(0.5f);
    }

    /* 체력 UI를 갱신 */
    void UpdateHPUI()
    {
        if (hpFill) hpFill.fillAmount = (float)hp / maxHp;
        if (hpText) hpText.text = $"{hp}/{maxHp}";
    }

    /* 사망 처리 */
    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 에임/IK 정지
        if (aimAndFlip) aimAndFlip.IsDead = true;
        if (rig) rig.weight = 0f;
        if (rigBuilder) rigBuilder.enabled = false;

        // 입력/전투/인벤 중단
        if (gunFire) gunFire.enabled = false;
        if (inventory) inventory.enabled = false;

        // 이동/사운드 중단
        if (playerMove)
        {
            playerMove.enabled = false;
            if (audioSource)
                audioSource.Stop();
        }

        // 죽음 애니메이션
        if (animator)
            animator.SetTrigger(dieTrigger);
    }
}

