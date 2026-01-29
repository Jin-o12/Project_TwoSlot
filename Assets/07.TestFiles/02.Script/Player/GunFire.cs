using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFire : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip fireClip;
    //public Animation fireAni;
    [Header("Refs")]
    public Transform barrel;                 // Barrel_Location
    public GameObject bulletPrefab;          // Bullet 프리팹
    public ParticleSystem shootVfx;          // 발사 이펙트(파티클)

    [Header("Shoot")]
    public float bulletSpeed = 40f;
    public float fireCooldown = 0.1f;

    [Header("Ammo")]
    public int maxAmmo = 10;
    public float reloadTime = 1.0f;

    int currentAmmo;
    bool isReloading;
    float lastFireTime;

    void Awake()
    {
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryShoot();
        }
    }

    void TryShoot()
    {
        
        if (isReloading) return;

        // 쿨다운
        if (Time.time - lastFireTime < fireCooldown) return;

        // 탄이 없으면 자동 장전
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        Fire();
    }

    void Fire()
    {
        
        lastFireTime = Time.time;
        audioSource.PlayOneShot(fireClip, 1.0f);
        //fireAni.Play("fire");
        // 1발 감소
        currentAmmo--;

        // 총알 생성
        Vector3 spawnPos = barrel.position + barrel.forward * 0.2f;

        GameObject b = Instantiate(bulletPrefab, spawnPos, barrel.rotation);
        // 총구 방향(Barrel_Location의 파란축 forward 기준)
        var bulletCol = b.GetComponent<Collider>();

        if (bulletCol != null)
        {
            foreach (var col in GetComponentsInParent<Collider>())
            {
                if (col != null)
                Physics.IgnoreCollision(bulletCol, col, true);
            }
        }
        Rigidbody rb = b.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = barrel.forward * bulletSpeed; // Unity 6+면 linearVelocity, 구버전이면 velocity
        
        }

        // 발사 이펙트 1회 재생
        
        if (shootVfx != null)
        {
            shootVfx.Clear(true);
            shootVfx.Play(true);
        }

        // 탄이 0이 되면 즉시 자동 장전 시작(원하면 즉시/지연 선택 가능)
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }

        // 디버그
        // Debug.Log($"Ammo: {currentAmmo}/{maxAmmo}");
    }

    IEnumerator Reload()
    {
        if (isReloading) yield break;
        isReloading = true;

        // 여기서 재장전 애니/사운드 트리거 가능
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }
}
