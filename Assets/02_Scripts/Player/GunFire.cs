using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFire : MonoBehaviour
{
    [Header("총 오브젝트 이름")]
    public string gunName;                  // 총 오브젝트를 찾기 위한 오브젝트 이름

    [Header("참조 오브젝트 & 컴포넌트")]
    public Camera cam;                      // 카메라
    public Transform barrel;                // 총구 위치
    public GameObject bulletPrefab;         // 총알 프리팹
    public Transform gunObject;             // 총 오브젝트

    [Header("Targeting")]
    public LayerMask enemyMask;             // 레이어: ENEMY
    public float maxDistance = 20f;        // 탄환 z축 보정 최댓값

    [Header("2.5D")]
    public float defaultCombatZ;            //

    [Header("Shoot")]
    public float bulletSpeed;               // 총알 속도
    public float fireCooldown;              // 발사 쿨다운
    public float spawnForwardOffset;        // 총알 발사 시 생성 지점 보정치

    [Header("Ammo")]
    public int magSize = 10;                // 탄창 크기
    public int maxAmmo = 30;                // 최대 총알
    public int currentAmmo                  // 현재 총알
    { get; private set; }
    public float reloadTime = 2.3f;         // 재장전 시간
    bool isReloading = false;

    [Header("VFX / SFX")]
    public AudioSource audioSource;
    public AudioClip fireClip;
    public AudioClip reloadClip;
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

        // 총 오브젝트 관련 컴포넌트 불러오기 및 초기화
        if(gunObject==null)     gunObject = FindChildByName(transform, gunName);
        if(audioSource==null)   audioSource = gunObject.GetComponent<AudioSource>();
        if(muzzleFlash==null)   muzzleFlash = gunObject.GetComponent<ParticleSystem>();
        if(barrel==null)        barrel = gunObject.transform;

        // 플레이어와 Z 값 맞춤
        defaultCombatZ = gameObject.transform.position.z;

        // 총 발사에 관한 변수들 일관 초기화
        bulletSpeed = 40f;
        fireCooldown = 0.25f;
        spawnForwardOffset = 0.6f;
        currentAmmo = 10;
    }

    /* 자식 오브젝트 중 특정 이름의 오브젝트를 가져오는 코드 */
    Transform FindChildByName(Transform parent, string nameToFind)
    {
        Transform[] allChildren = parent.GetComponentsInChildren<Transform>(true);

        // 자식 오브젝트를 순회하며 해당 이름의 오브젝트를 찾음
        foreach (Transform child in allChildren)
        {
            if (child.name == nameToFind)
            {
                return child;
            }
        }
        // 없을 시 null 반환
        Debug.Log($"GunFire.cs: {transform.name} 오브젝트 하위에 {nameToFind}가 존재하지 않습니다.");
        return null;
    }

    /* 게임 시작시 총알 수 초기화 */
    public void InitializedGun()
    {
        currentAmmo = Mathf.Clamp(currentAmmo, 0, magSize);
        maxAmmo = Mathf.Max(0, maxAmmo); // 시작 시 총알이 없을 시 총알 보충
    }

    void Update()
    {
        if (InputPauseManager.IsPaused) return;
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
            if (maxAmmo > 0) StartCoroutine(Reload());
            return;
        }

        if (Time.time - lastFire < fireCooldown) return;

        lastFire = Time.time;
        currentAmmo--;

        Fire();

        // 마지막 탄 쏜 직후에도 자동 리로드
        if (currentAmmo <= 0 && maxAmmo > 0)
        {
            StartCoroutine(Reload());
        }
    }

    void Fire()
    {
        if (!cam || !barrel || !bulletPrefab) return;

        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);

        float targetZ = defaultCombatZ;
        RaycastHit[] hits = Physics.RaycastAll(mouseRay, maxDistance, enemyMask);
        if (hits.Length > 0)
        {
            // 카메라에서 가장 가까운 적 hit 선택
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            targetZ = hits[0].collider.transform.position.z;
        }
        else
        {
            targetZ = FindNearestEnemyZ(barrel.position, 20f);
        }

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
            audioSource.PlayOneShot(fireClip, 0.55f);

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Simulate(0f, true, true, true);
            muzzleFlash.Play(true);
        }
    }

    private float FindNearestEnemyZ(Vector3 origin, float radius)
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

    private void IgnoreMyColliders(GameObject bullet)
    {
        var bulletCol = bullet.GetComponent<Collider>();
        if (bulletCol == null) return;

        foreach (var col in GetComponentsInParent<Collider>())
        {
            if (col != null) Physics.IgnoreCollision(bulletCol, col, true);
        }
    }
    IEnumerator Reload()
    {
        if (isReloading) yield break;

        // 이미 꽉 찼거나 예비탄 없으면 리로드 안 함
        if (maxAmmo <= 0) yield break;
        if (currentAmmo >= magSize) yield break;

        isReloading = true;
        audioSource.PlayOneShot(reloadClip, 1f);
        // 여기서 리로드 사운드/애니 넣어도 됨

        yield return new WaitForSeconds(reloadTime);

        int need = magSize - currentAmmo;          // 채워야 할 탄 수
        int load = Mathf.Min(need, maxAmmo);       // 예비탄에서 꺼낼 수 있는 만큼

        currentAmmo += load;                        // 탄창 채우고
        maxAmmo -= load;                            // 예비탄 차감

        isReloading = false;
    }
    public void AddMaxAmmo(int amount)
    {
        maxAmmo += amount;

        //UpdateAmmoUI();      - 아직 구현 안됨
    }
}