using UnityEngine;

public class PlayerNoiseBombUse : MonoBehaviour
{
    public PlayerItemTrigger inv;

    [Header("Item")]
    public string noiseBombId = "SoundBomb";
    public GameObject noiseBombPrefab;

    [Header("Throw Spawn")]
    public Transform throwPoint;                 // Player 밑에 빈 오브젝트로 만들기 추천
    public float spawnForwardOffset = 0.6f;      // throwPoint 없을 때만 사용
    public float spawnUpOffset = 1.2f;

    [Header("Throw Force")]
    public float throwForce = 12f;
    public float upwardForce = 2.5f;

    [Header("Aim")]
    public bool useMouseAim = false;             // ✅ 이거 false면 무조건 플레이어 정면으로 던짐
    public Camera aimCamera;                     // useMouseAim=true일 때만 필요
    public float aimMaxDistance = 60f;
    public LayerMask aimMask = ~0;

    void Awake()
    {
        if (inv == null) inv = GetComponent<PlayerItemTrigger>();
        if (aimCamera == null) aimCamera = Camera.main;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (inv == null) return;

        if (inv.activeMode != PlayerItemTrigger.ActiveMode.Item) return;
        if (inv.GetSelectedItem() != noiseBombId) return;

        if (noiseBombPrefab == null)
        {
            Debug.LogWarning("[SoundBomb] noiseBombPrefab이 비어있어!");
            return;
        }

        // 1) 스폰 위치
        Vector3 spawnPos = GetSpawnPos();

        // 2) 던질 방향(dir) 계산
        Vector3 dir = useMouseAim ? GetMouseAimDir(spawnPos) : GetPlayerForwardFlat();

        // 3) 생성
        var go = Instantiate(noiseBombPrefab, spawnPos, Quaternion.identity);

        // 4) Rigidbody 물리
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(dir * throwForce + Vector3.up * upwardForce, ForceMode.VelocityChange);

        // 5) 소모 + 총로 복귀
        inv.TryConsume(noiseBombId);
        inv.SwitchToGun();
    }

    Vector3 GetSpawnPos()
    {
        if (throwPoint != null) return throwPoint.position;

        // throwPoint 없을 때: 플레이어 앞 + 위
        return transform.position + transform.forward * spawnForwardOffset + Vector3.up * spawnUpOffset;
    }

    // ✅ 옆으로 나가는 문제를 잡는 핵심: "플레이어 정면을 수평으로"
    Vector3 GetPlayerForwardFlat()
    {
        Vector3 f = transform.forward;
        f.y = 0f;
        if (f.sqrMagnitude < 0.0001f) f = Vector3.forward;
        return f.normalized;
    }

    // 마우스 조준(카메라 필요). 카메라가 옆에 있으면 당연히 옆으로 던져질 수 있음.
    Vector3 GetMouseAimDir(Vector3 spawnPos)
    {
        if (aimCamera == null) return GetPlayerForwardFlat();

        Ray ray = aimCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, aimMaxDistance, aimMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 d = hit.point - spawnPos;
            d.y = 0f; // 수평으로만 던지기(원하면 제거 가능)
            if (d.sqrMagnitude < 0.0001f) return GetPlayerForwardFlat();
            return d.normalized;
        }

        // 안 맞으면 카메라 정면
        Vector3 fallback = ray.direction;
        fallback.y = 0f;
        if (fallback.sqrMagnitude < 0.0001f) return GetPlayerForwardFlat();
        return fallback.normalized;
    }
}
