using System.Collections;
using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public int damage = 100;
    public float lifeTime = 2f;
    public AudioSource hitSound;
    public AudioClip hitClip;
    Transform owner;
    bool hasHit = false;

    void Start()
    {
        Destroy(gameObject, lifeTime); // 시간 지나면 자동 삭제
    }

    public void SetOwner(Transform ownerTransform)
    {
        owner = ownerTransform;

        // 발사자와 총알 충돌 무시 (발사자에 콜라이더가 여러 개일 수 있으니 모두 무시)
        var myCol = GetComponent<Collider>();
        if (myCol == null || owner == null) return;

        foreach (var col in owner.GetComponentsInChildren<Collider>())
        {
            if (col != null) Physics.IgnoreCollision(myCol, col, true);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (owner != null && other.transform.IsChildOf(owner)) return; // 발사자/발사자의 자식 무시

        // 플레이어(또는 PlayerHealth/IDamageable) 찾기
        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg == null) return; // 플레이어가 아니거나 데미지 대상이 아님

        hasHit = true;

        // 데미지 적용
        dmg.TakeDamage(damage);
        //사운드 재생 + 삭제 처리
        StartCoroutine(DieAfterSfx());
    }
    IEnumerator DieAfterSfx()
    {
        float wait = 0.05f;

        // 소리는 “원샷 오디오”로 월드에 재생(끊김 방지)
        if (hitClip != null)
        {
            AudioSource.PlayClipAtPoint(hitClip, transform.position, 1f);
            wait = Mathf.Max(wait, hitClip.length);
        }

        yield return new WaitForSeconds(wait);
        Destroy(gameObject);
    }
}
