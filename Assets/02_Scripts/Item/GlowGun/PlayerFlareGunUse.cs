using UnityEngine;

public class PlayerFlareGunUse : MonoBehaviour
{
    public PlayerItemTrigger inv;

    [Header("Item (WeaponItem)")]
    public WeaponItem flareGunItem;

    [Header("Projectile")]
    public Rigidbody flareProjectile;

    [Header("Muzzle / Spawn")]
    public Transform muzzle;
    public float spawnForwardOffset = 0.6f;
    public float spawnUpOffset = 1.2f;

    [Header("Shoot Force")]
    public float shootForce = 35f;
    public ForceMode forceMode = ForceMode.VelocityChange;

    [Header("Aim")]
    public bool useMouseAim = true;          // ✅ 추가
    public Camera aimCamera;                // ✅ 추가
    public bool useFacingByLocalScaleX = false;
    public Vector3 rightDir = Vector3.right;
    public Vector3 leftDir = Vector3.left;

    [Header("FX (Optional)")]
    public GameObject muzzleParticles;
    public AudioSource audioSource;
    public AudioClip shotSound;

    [Header("Collision (Optional)")]
    public bool ignoreOwnerColliders = true;

    void Awake()
    {
        if (inv == null) inv = GetComponent<PlayerItemTrigger>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (aimCamera == null) aimCamera = Camera.main;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (inv == null) return;

        if (inv.activeMode != PlayerItemTrigger.ActiveMode.Item) return;

        var selected = inv.GetSelectedItem();
        if (selected != flareGunItem) return;

        if (flareProjectile == null)
        {
            Debug.LogWarning("[FlareGun] flareProjectile(발사체 프리팹)이 비어있어!");
            return;
        }

        Vector3 spawnPos = GetSpawnPos();
        spawnPos.z = transform.position.z; // ✅ 사이드스크롤: 깊이 고정(카메라에 보이게)

        Vector3 dir = GetShootDir_SideScroll(spawnPos);
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, dir); 
        // ✅ 사이드스크롤(2D 느낌): forward는 Z, up은 dir(=XY 방향)
        // 3D 모델이면 rot 방식은 바꿔야 할 수도 있음 (아래 참고)

        Rigidbody b = Instantiate(flareProjectile, spawnPos, rot);

        b.isKinematic = false;
        b.useGravity = true;
        b.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        b.velocity = Vector3.zero;
        b.angularVelocity = Vector3.zero;

        if (ignoreOwnerColliders)
            IgnoreOwnerCollision(b);

        // ✅ 확실하게 움직이게: velocity로도 가능
        b.AddForce(dir * shootForce, forceMode);

        if (muzzleParticles != null)
            Instantiate(muzzleParticles, spawnPos, rot);

        if (audioSource != null && shotSound != null)
            audioSource.PlayOneShot(shotSound);

        inv.TryConsume(flareGunItem);
        inv.SwitchToGun();
    }

    Vector3 GetSpawnPos()
    {
        if (muzzle != null) return muzzle.position;
        return transform.position + transform.right * spawnForwardOffset + Vector3.up * spawnUpOffset;
        // ✅ 사이드스크롤은 forward 대신 right가 보통 "정면"
    }

    Vector3 GetShootDir_SideScroll(Vector3 spawnPos)
    {
        // 1) 마우스 에임
        if (useMouseAim && aimCamera != null)
        {
            Ray ray = aimCamera.ScreenPointToRay(Input.mousePosition);

            // ✅ Z=spawnPos.z 평면에서 마우스 월드 포인트 구하기
            Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, spawnPos.z));
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 point = ray.GetPoint(enter);
                Vector3 d = point - spawnPos;
                d.z = 0f;

                if (d.sqrMagnitude > 0.0001f)
                    return d.normalized;
            }
        }

        // 2) (옵션) 스케일 기반 좌/우 고정
        if (useFacingByLocalScaleX)
        {
            bool facingRight = transform.lossyScale.x >= 0f;
            Vector3 d = facingRight ? rightDir : leftDir;
            d.z = 0f;
            return SafeDir(d);
        }

        // 3) 기본: 캐릭터가 바라보는 방향
        Vector3 fallback = transform.right; // ✅ 사이드스크롤 기본 정면
        fallback.z = 0f;
        return SafeDir(fallback);
    }

    void IgnoreOwnerCollision(Rigidbody projRb)
    {
        if (projRb == null) return;

        Collider projCol = projRb.GetComponent<Collider>();
        if (projCol == null) return;

        var ownerCols = GetComponentsInChildren<Collider>();
        foreach (var c in ownerCols)
            if (c != null) Physics.IgnoreCollision(projCol, c, true);
    }

    Vector3 SafeDir(Vector3 d)
    {
        if (d.sqrMagnitude < 0.0001f) d = Vector3.right;
        return d.normalized;
    }
}
