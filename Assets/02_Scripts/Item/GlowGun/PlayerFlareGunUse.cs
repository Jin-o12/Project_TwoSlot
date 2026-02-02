using UnityEngine;

public class PlayerFlareGunUse : MonoBehaviour
{
    public PlayerItemTrigger inv;

    [Header("Item")]
    public string flareGunId = "Glowgun";
    public Rigidbody flareProjectile;              // 발사체 프리팹(반드시 할당)
    
    [Header("Muzzle / Spawn")]
    public Transform muzzle;                       // 총구(권장)
    public float spawnForwardOffset = 0.6f;        // muzzle 없을 때만
    public float spawnUpOffset = 1.2f;

    [Header("Shoot Force")]
    public float shootForce = 35f;
    public ForceMode forceMode = ForceMode.VelocityChange; // 질량 영향 덜 받게(안정적)

    [Header("Aim")]
    public bool forceFlatForward = true;           // 사이드/탑다운에서 수평으로만 쏘고 싶을 때
    public bool useFacingByLocalScaleX = false;    // 캐릭터가 scale.x로 좌우 반전이면 true로
    public Vector3 rightDir = Vector3.right;       // scale.x 기반일 때 사용
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
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (inv == null) return;

        if (inv.activeMode != PlayerItemTrigger.ActiveMode.Item) return;
        if (inv.GetSelectedItem() != flareGunId) return;

        if (flareProjectile == null)
        {
            Debug.LogWarning("[FlareGun] flareProjectile(발사체 프리팹)이 비어있어!");
            return;
        }

        // 1) 스폰 위치/회전
        Vector3 spawnPos = GetSpawnPos();
        Vector3 dir = GetShootDir(spawnPos);
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        // 2) 생성
        Rigidbody b = Instantiate(flareProjectile, spawnPos, rot);

        // 3) 물리 세팅
        b.isKinematic = false;
        b.useGravity = true;
        b.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        b.velocity = Vector3.zero;
        b.angularVelocity = Vector3.zero;

        // 4) 충돌 무시(플레이어 몸에 박혀서 멈추는 문제 방지)
        if (ignoreOwnerColliders)
        {
            var projCol = b.GetComponent<Collider>();
            if (projCol != null)
            {
                var ownerCols = GetComponentsInChildren<Collider>();
                foreach (var c in ownerCols)
                    Physics.IgnoreCollision(projCol, c, true);
            }
        }

        // 5) 발사
        b.AddForce(dir * shootForce, forceMode);

        // 6) FX
        if (muzzleParticles != null)
            Instantiate(muzzleParticles, spawnPos, rot);

        if (audioSource != null && shotSound != null)
            audioSource.PlayOneShot(shotSound);

        // 7) 소모 + 총로 복귀
        inv.TryConsume(flareGunId);
        inv.SwitchToGun();
    }

    Vector3 GetSpawnPos()
    {
        if (muzzle != null) return muzzle.position;

        // muzzle 없을 때: 플레이어 앞 + 위
        return transform.position + transform.forward * spawnForwardOffset + Vector3.up * spawnUpOffset;
    }

    Vector3 GetShootDir(Vector3 spawnPos)
    {
        // (옵션) 스케일 기반 좌/우 방향 강제(2D에서 가장 확실)
        if (useFacingByLocalScaleX)
        {
            bool facingRight = transform.lossyScale.x >= 0f;
            Vector3 d = facingRight ? rightDir : leftDir;
            return SafeDir(d);
        }

        // 기본: muzzle.forward 또는 플레이어 forward
        Vector3 dir = (muzzle != null) ? muzzle.forward : transform.forward;

        if (forceFlatForward)
            dir.y = 0f;

        return SafeDir(dir);
    }

    Vector3 SafeDir(Vector3 d)
    {
        if (d.sqrMagnitude < 0.0001f) d = Vector3.forward;
        return d.normalized;
    }
}
