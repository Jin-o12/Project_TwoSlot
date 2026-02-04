/// <summary>
/// 플레이어의 체력 수치 관링와 사망 판정 수행
/// </summary>
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("컴포넌트&스크립트")]
    // 컴포넌트
    private AudioSource audioSource;
    public AudioClip hitClip;
    private Animator animator;
    // 스크립트
    private PlayerMove playerMove;
    private AimAndFlip aimAndFlip;
    private RigBuilder rigBuilder;
    private StageManager stageManager;

    [Header("HP")]
    public int maxHp = 100;                 // 최대 체력
    public int currentHp                    // 현재 체력
    { get; private set; }

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
        if (!animator)      animator = GetComponentInChildren<Animator>();
        if (!gunFire)       gunFire = GetComponentInChildren<GunFire>();
        if (!playerMove)    playerMove = GetComponent<PlayerMove>();
        if (!rigBuilder)    rigBuilder = GetComponent<RigBuilder>();
        if (!audioSource)   audioSource = GetComponent<AudioSource>();
        if (!aimAndFlip)    aimAndFlip = GetComponent<AimAndFlip>();
        if(!stageManager)   stageManager = StageManager.Instance;
    }

    public void Start()
    {
        Initialized();
    }

    /* 플레이어 체력 수치 초기화 */
    public void Initialized()
    {
        currentHp = maxHp;        
    }

    /* 피해를 입음 */
    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHp = Mathf.Clamp(currentHp - dmg, 0, maxHp);
        
        if (currentHp <= 0)
        {
            Die();
            return;
        }

        // Hit 애니
        if (animator && !string.IsNullOrEmpty(hitTrigger))
            animator.SetTrigger(hitTrigger);
            audioSource.PlayOneShot(hitClip);

        // 0.5초 발사 금지
        gunFire?.LockFire(0.5f);
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

        // 입력/전투 중단
        if (gunFire) gunFire.enabled = false;

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

        // 사망 모션 재생 기다렸다가 씬 전환
        Invoke("GameoverScene", 3.0f);
    }

    void GameoverScene()
    {
        stageManager.GameOver();
    }
}

