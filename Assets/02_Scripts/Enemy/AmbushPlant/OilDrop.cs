using UnityEngine;

public class OilDrop : MonoBehaviour
{
    [Header("Spawn Puddle")]
    public GameObject puddlePrefab;         // 바닥에 생길 웅덩이 프리팹(Quad/Decal 등)
    public float puddleScaleMin = 0.8f;
    public float puddleScaleMax = 1.3f;

    [Header("Puddle Lifetime")]
    public float puddleLifeTime = 10f;      // ✅ 웅덩이 지속 시간(초) - 필요하면 조절
    public bool destroyPuddleAfterTime = true;

    [Header("Raycast")]
    public LayerMask groundMask = ~0;       // 바닥 레이어만 지정하면 더 안전
    public float rayDistance = 0.6f;        // 바닥 체크 거리

    [Header("Lifetime")]
    public float maxLifeTime = 6f;          // 혹시 못 닿으면 자동 제거

    bool spawned;

    void Start()
    {
        Destroy(gameObject, maxLifeTime);
    }

    void OnCollisionEnter(Collision col)
    {
        if (spawned) return;

        // 바닥 레이어가 아니라면 무시하고 싶으면 주석 해제
        // if (((1 << col.gameObject.layer) & groundMask) == 0) return;

        SpawnPuddleAtContact(col);
        spawned = true;
        Destroy(gameObject);
    }

    void SpawnPuddleAtContact(Collision col)
    {
        if (!puddlePrefab) return;

        // 충돌 지점 사용 (가장 정확)
        Vector3 pos = col.contacts[0].point;

        // 바닥에 딱 붙이고 싶으면 레이로 한번 더 보정
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            pos = hit.point;
        }

        // Z-fighting 방지로 살짝 띄우기
        pos += Vector3.up * 0.01f;

        Quaternion rot = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f); // 바닥 기준 회전

        var puddle = Instantiate(puddlePrefab, pos, rot);

        float s = Random.Range(puddleScaleMin, puddleScaleMax);
        puddle.transform.localScale = new Vector3(s, s, s);

        // ✅ 생성된 웅덩이 자동 제거
        if (destroyPuddleAfterTime && puddleLifeTime > 0f)
        {
            Destroy(puddle, puddleLifeTime);
        }
    }
}
