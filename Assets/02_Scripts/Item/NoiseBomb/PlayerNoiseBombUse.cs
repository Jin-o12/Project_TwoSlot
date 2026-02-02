using UnityEngine;

public class PlayerNoiseBombUse : MonoBehaviour
{
    public PlayerItemTrigger inv;

    [Header("Item (WeaponItem)")]
    public WeaponItem noiseBombItem;
    public GameObject noiseBombPrefab;

    [Header("Throw Spawn")]
    public Transform throwPoint;
    public float spawnForwardOffset = 0.6f;
    public float spawnUpOffset = 1.2f;

    [Header("Throw Force (distance scaled)")]
    public float minThrowForce = 6f;     // ✅ 가까울 때 힘
    public float maxThrowForce = 16f;    // ✅ 멀 때 힘
    public float minUpwardForce = 1.5f;  // ✅ 가까울 때 위로
    public float maxUpwardForce = 3.5f;  // ✅ 멀 때 위로
    public float minAimDistance = 1.5f;  // ✅ 이 거리 이하는 "가까움"
    public float maxAimDistance = 10f;   // ✅ 이 거리 이상은 "멀음"

    [Header("Aim")]
    public bool useMouseAim = true;      // 거리 기반이면 보통 true 권장
    public Camera aimCamera;

    [Header("Collision (Optional)")]
    public bool ignoreOwnerColliders = true;

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

        if (noiseBombItem == null)
        {
            Debug.LogWarning("[SoundBomb] noiseBombItem(WeaponItem)이 비어있어!");
            return;
        }

        WeaponItem selected = inv.GetSelectedItem();
        if (selected != noiseBombItem) return;

        if (noiseBombPrefab == null)
        {
            Debug.LogWarning("[SoundBomb] noiseBombPrefab이 비어있어!");
            return;
        }

        Vector3 spawnPos = GetSpawnPos();

        // ✅ 사이드 스크롤: 마우스 월드 포인트 & 방향 & 거리비율(t) 얻기
        Vector3 dir;
        float t; // 0(가까움) ~ 1(멀음)

        if (useMouseAim)
            dir = GetSideScrollMouseAimDir(spawnPos, out t);
        else
        {
            dir = GetPlayerForwardFlat();
            t = 1f;
        }

        // ✅ 거리비율(t)로 힘 보간
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, t);
        float upForce = Mathf.Lerp(minUpwardForce, maxUpwardForce, t);

        GameObject go = Instantiate(noiseBombPrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (ignoreOwnerColliders)
            IgnoreOwnerCollision(go);

        // ✅ 최종 힘 적용
        rb.AddForce(dir * throwForce + Vector3.up * upForce, ForceMode.VelocityChange);

        inv.TryConsume(noiseBombItem);
        inv.SwitchToGun();
    }

    Vector3 GetSpawnPos()
    {
        if (throwPoint != null) return throwPoint.position;
        return transform.position + transform.forward * spawnForwardOffset + Vector3.up * spawnUpOffset;
    }

    Vector3 GetPlayerForwardFlat()
    {
        Vector3 f = transform.forward;
        f.y = 0f; f.z = 0f; // ✅ 사이드 스크롤: 깊이 고정
        if (f.sqrMagnitude < 0.0001f) f = Vector3.right; // 보통 오른쪽이 정면
        return f.normalized;
    }

    // ✅ 사이드 스크롤 전용: Z=spawnPos.z 평면에서 마우스 포인트를 구함
    // 반환: dir(정규화), out t(가까움0~멀음1)
    Vector3 GetSideScrollMouseAimDir(Vector3 spawnPos, out float t)
    {
        t = 1f;
        if (aimCamera == null) return GetPlayerForwardFlat();

        Ray ray = aimCamera.ScreenPointToRay(Input.mousePosition);

        // Z 고정 평면(앞/뒤 깊이 제거)
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, spawnPos.z));

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 point = ray.GetPoint(enter);
            Vector3 d = point - spawnPos;

            d.z = 0f; // 깊이 제거

            float dist = d.magnitude;

            // ✅ 거리→0~1 정규화
            t = Mathf.InverseLerp(minAimDistance, maxAimDistance, dist);

            if (d.sqrMagnitude < 0.0001f) return GetPlayerForwardFlat();
            return d.normalized;
        }

        return GetPlayerForwardFlat();
    }

    void IgnoreOwnerCollision(GameObject proj)
    {
        if (proj == null) return;

        Collider projCol = proj.GetComponent<Collider>();
        if (projCol == null) return;

        Collider[] ownerCols = GetComponentsInChildren<Collider>();
        foreach (Collider c in ownerCols)
        {
            if (c == null) continue;
            Physics.IgnoreCollision(projCol, c, true);
        }
    }
}
