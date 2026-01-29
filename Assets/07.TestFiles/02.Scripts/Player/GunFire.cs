using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFire : MonoBehaviour
{
   [Header("Refs")]
    public Camera cam;
    public Transform barrel;
    public GameObject bulletPrefab;

    [Header("Targeting")]
    public LayerMask enemyMask;          // ENEMY
    public LayerMask worldMask = ~0;
    public float maxDistance = 200f;

    [Header("2.5D")]
    public float defaultCombatZ = 0f;

    [Header("Shoot")]
    public float bulletSpeed = 40f;
    public float fireCooldown = 0.1f;
    public float spawnForwardOffset = 0.6f;

    [Header("Ammo")]
    public int maxAmmo = 10;
    public int currentAmmo = 10;
    public float reloadTime = 1.5f;   // 재장전 시간
    bool isReloading = false;

    [Header("VFX / SFX")]
    public AudioSource audioSource;
    public AudioClip fireClip;
    public ParticleSystem muzzleFlash;

    float lastFire;
    bool fireLocked;
    public void LockFire(float sec)
    {
        CancelInvoke(nameof(UnlockFire));
        fireLocked = true;
        Invoke(nameof(UnlockFire), sec);
    }
    void UnlockFire() => fireLocked = false;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (currentAmmo <= 0) currentAmmo = maxAmmo; // 시작 시 보정
    }

    void Update()
    {
        // ✅ 단발: 클릭 1번에 1발
        if (Input.GetMouseButtonDown(0))
        {
            TryFire();
        }
    }

    void TryFire()
    {
        if (fireLocked) return;
        // 재장전 중이면 발사 불가
        if (isReloading) return;

        // 탄 없으면 자동 리로드
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Time.time - lastFire < fireCooldown) return;

        lastFire = Time.time;
        currentAmmo--;

        Fire();

    // 마지막 탄 쏜 직후에도 자동 리로드
    if (currentAmmo <= 0)
    {
        StartCoroutine(Reload());
    }
    }

    void Fire()
    {
        if (!cam || !barrel || !bulletPrefab) return;

        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);

        float targetZ = defaultCombatZ;
        bool hasEnemyHit = Physics.Raycast(mouseRay, out RaycastHit enemyHit, maxDistance, enemyMask);
        if (hasEnemyHit)
            targetZ = enemyHit.collider.transform.position.z;
        else
            targetZ = FindNearestEnemyZ(barrel.position, 20f);

        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, targetZ));
        Vector3 aimPoint;
        if (!plane.Raycast(mouseRay, out float enter))
        {
            aimPoint = barrel.position + barrel.forward * 10f;
            aimPoint.z = targetZ;
        }
        else
        {
            aimPoint = mouseRay.GetPoint(enter);
        }

        Vector3 dir = (aimPoint - barrel.position);
        dir.z = 0f;
        if (dir.sqrMagnitude < 0.000001f) return;
        dir.Normalize();

        Vector3 spawnPos = barrel.position + dir * spawnForwardOffset;
        spawnPos.z = targetZ;

        GameObject b = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir, Vector3.forward));

        var lockZ = b.GetComponent<LockZ>();
        if (lockZ != null) lockZ.fixedZ = targetZ;

        IgnoreMyColliders(b);

        if (b.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.velocity = dir * bulletSpeed;
        }

        // ✅ 이펙트/사운드는 발사 성공 시에만 1번 실행
        if (audioSource != null && fireClip != null)
            audioSource.PlayOneShot(fireClip);

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Simulate(0f, true, true, true);
            muzzleFlash.Play(true);
        }
    }

    float FindNearestEnemyZ(Vector3 origin, float radius)
    {
        Collider[] cols = Physics.OverlapSphere(origin, radius, enemyMask);
        if (cols == null || cols.Length == 0) return defaultCombatZ;

        float bestZ = cols[0].transform.position.z;
        float best = float.MaxValue;

        foreach (var c in cols)
        {
            float dz = Mathf.Abs(c.transform.position.z - origin.z);
            if (dz < best)
            {
                best = dz;
                bestZ = c.transform.position.z;
            }
        }
        return bestZ;
    }

    void IgnoreMyColliders(GameObject bullet)
    {
        var bulletCol = bullet.GetComponent<Collider>();
        if (bulletCol == null) return;

        foreach (var col in GetComponentsInParent<Collider>())
        {
            if (col != null) Physics.IgnoreCollision(bulletCol, col, true);
        }
    }
    System.Collections.IEnumerator Reload()
    {
    if (isReloading) yield break;

    isReloading = true;

    // 여기서 리로드 사운드/애니 넣어도 됨

    yield return new WaitForSeconds(reloadTime);

    currentAmmo = maxAmmo;
    isReloading = false;
    }
}