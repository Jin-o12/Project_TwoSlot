using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHp = 100;
    int hp;

    [Header("Audio")]
    public AudioSource audioSource;

    [Tooltip("피격 사운드 3개 넣기")]
    public AudioClip[] hitClips;   // ✅ 3개 넣기

    public AudioClip dieClip;

    void Awake()
    {
        hp = maxHp;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;

        PlayRandomHitSound();

        if (hp <= 0)
            Die();
    }

    void PlayRandomHitSound()
    {
        if (hitClips == null || hitClips.Length == 0) return;

        // ✅ 랜덤 선택
        AudioClip clip = hitClips[Random.Range(0, hitClips.Length)];
        if (clip == null) return;

        if (audioSource != null)
            audioSource.PlayOneShot(clip);
        else
            AudioSource.PlayClipAtPoint(clip, transform.position, 1f);
    }

    void Die()
    {
        GameDataManager.Instance.AddScore(10);

        if (dieClip != null)
            AudioSource.PlayClipAtPoint(dieClip, transform.position, 1f);

        Destroy(gameObject);
    }
}
