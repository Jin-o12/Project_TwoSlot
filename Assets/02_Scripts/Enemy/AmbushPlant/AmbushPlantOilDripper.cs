using UnityEngine;

public class AmbushPlantOilDripper : MonoBehaviour
{
    [Header("Spawn")]
    public Transform dropSpawnPoint;    // 파리지옥 아래쪽 위치
    public GameObject oilDropPrefab;    // 떨어질 방울 프리팹(스피어)
    public GameObject puddlePrefab;     // 웅덩이 프리팹(Quad/Decal)

    [Header("Timing")]
    public float interval = 1.2f;       // 몇 초마다 떨어질지
    public float randomInterval = 1.0f; // 랜덤 추가 (0이면 고정)

    [Header("Drop Physics")]
    public float startDownVelocity = 0f;  // 살짝 아래로 속도 주기(원하면)
    public float randomSideForce = 0.2f;  // 약간 흔들리게(0이면 정확히 아래)
    public float dropScale = 0.18f;       // 방울 크기

    [Header("Ground Mask")]
    public LayerMask groundMask = ~0;

    float nextTime;

    void Start()
    {
        ScheduleNext();
    }

    void Update()
    {
        if (Time.time < nextTime) return;
        SpawnDrop();
        ScheduleNext();
    }

    void ScheduleNext()
    {
        float add = randomInterval > 0 ? Random.Range(-randomInterval, randomInterval) : 0f;
        nextTime = Time.time + Mathf.Max(0.05f, interval + add);
    }

    void SpawnDrop()
    {
        if (!oilDropPrefab) return;

        Vector3 pos = dropSpawnPoint ? dropSpawnPoint.position : transform.position;
        var drop = Instantiate(oilDropPrefab, pos, Quaternion.identity);
        drop.transform.localScale = Vector3.one * dropScale;

        // 방울에 puddlePrefab/groundMask 주입
        var oil = drop.GetComponent<OilDrop>();
        if (oil != null)
        {
            oil.puddlePrefab = puddlePrefab;
            oil.groundMask = groundMask;
        }

        // 물리 세팅
        var rb = drop.GetComponent<Rigidbody>();
        if (rb)
        {
            Vector3 v = Vector3.down * startDownVelocity;
            v += new Vector3(Random.Range(-randomSideForce, randomSideForce), 0f, Random.Range(-randomSideForce, randomSideForce));
            rb.velocity = v;
        }
    }
}
