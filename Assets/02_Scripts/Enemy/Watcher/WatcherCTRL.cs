using System.Collections;
using UnityEngine;

public class WatcherByDistance : MonoBehaviour
{
    [Header("References")]
    public Transform headTr;
    public Transform playerTr;
    public AudioSource alarmAudio;
    public Light warningLight;

    [Header("Distances")]
    public float lightDistance = 15f; // 50m: 노란 경고
    public float spawnDistance = 10f; // 20m: 빨간 경보 + 소환

    [Header("Look")]
    public float rotateSpeed = 6f;

    [Header("Spawn")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public int spawnCount = 2;
    public float spawnInterval = 0.3f;

    [Header("Spawn - Around SpawnPoint")]
    public float spawnSpreadX = 2.0f;    // 스폰포인트 기준 좌우 퍼짐
    public float minGap = 0.8f;          // 적끼리 최소 간격(겹침 방지)
    public int maxTry = 15;              // 자리 찾기 시도 횟수
    public LayerMask enemyLayer;         // Enemy 레이어만 체크해줘
    public bool lockZAxis = true;        // 2.5D면 켜기
    public float lockZ = 0f;             // 고정 Z 값

    [Header("VFX - Light")]
    public Color warnColor = new Color(1f, 0.85f, 0.15f);
    public Color alertColor = new Color(1f, 0.15f, 0.15f);

    public float warnBlinkSpeed = 2.0f;
    public float alertBlinkSpeed = 6.0f;

    public float warnIntensity = 3.0f;
    public float alertIntensity = 6.0f;

    [Header("SFX - Optional")]
    public bool playSoundInWarnRange = false;
    public float warnVolume = 0.25f;
    public float alertVolume = 1.0f;

    public float warnPitch = 1.0f;
    public float alertPitch = 1.15f;

    [Header("Behavior")]
    public bool keepLightOnAfterExit = false;

    private bool spawned = false;
    private bool soundStarted = false;

    void Start()
    {
        if (playerTr == null)
            playerTr = GameObject.FindWithTag("Player")?.transform;

        if (warningLight != null)
            warningLight.enabled = false;

        if (alarmAudio != null)
        {
            alarmAudio.loop = true;
            alarmAudio.playOnAwake = false;
        }
    }

    void Update()
    {
        if (playerTr == null || headTr == null) return;

        float dist = Vector3.Distance(transform.position, playerTr.position);

        bool inWarnRange = dist <= lightDistance;   // 50m
        bool inAlertRange = dist <= spawnDistance;  // 20m

        // 50m 이내: 응시
        if (inWarnRange)
            LookAtPlayer();

        // 라이트 연출
        UpdateWarningLight(inWarnRange, inAlertRange);

        // 사운드 연출
        UpdateSound(inWarnRange, inAlertRange);

        // 20m 이내: 소환(1회)
        if (inAlertRange && !spawned)
        {
            spawned = true;
            StartCoroutine(SpawnEnemies());
        }
    }

    // ✅ 정확히 플레이어를 바라보기(위/아래 포함)
    void LookAtPlayer()
    {
        Vector3 dir = playerTr.position - headTr.position;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        headTr.rotation = Quaternion.Slerp(headTr.rotation, targetRot, Time.deltaTime * rotateSpeed);
    }

    void UpdateWarningLight(bool inWarnRange, bool inAlertRange)
    {
        if (warningLight == null) return;

        if (!inWarnRange)
        {
            if (!keepLightOnAfterExit)
                warningLight.enabled = false;
            return;
        }

        warningLight.enabled = true;

        float blinkSpeed = inAlertRange ? alertBlinkSpeed : warnBlinkSpeed;
        float intensity = inAlertRange ? alertIntensity : warnIntensity;
        Color color = inAlertRange ? alertColor : warnColor;

        warningLight.color = color;

        float wave = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        warningLight.intensity = Mathf.Lerp(0.0f, intensity, wave);
    }

    void UpdateSound(bool inWarnRange, bool inAlertRange)
    {
        if (alarmAudio == null) return;

        if (!inWarnRange)
        {
            if (alarmAudio.isPlaying) alarmAudio.Stop();
            soundStarted = false;
            return;
        }

        if (!playSoundInWarnRange && !inAlertRange)
        {
            if (alarmAudio.isPlaying) alarmAudio.Stop();
            soundStarted = false;
            return;
        }

        if (!soundStarted)
        {
            soundStarted = true;
            alarmAudio.Play();
        }

        alarmAudio.volume = inAlertRange ? alertVolume : warnVolume;
        alarmAudio.pitch = inAlertRange ? alertPitch : warnPitch;
    }

  IEnumerator SpawnEnemies()
{
    if (enemyPrefab == null || spawnPoint == null) yield break;

    if (enemyLayer.value == 0)
        Debug.LogWarning("[WatcherByDistance] enemyLayer가 비어있어요! Enemy 레이어를 체크해 주세요.");

    for (int i = 0; i < spawnCount; i++)
    {
        Vector3 pos = FindSpawnPosNearSpawnPoint();
        Instantiate(enemyPrefab, pos, Quaternion.identity);
        yield return new WaitForSeconds(spawnInterval);
    }
}

Vector3 FindSpawnPosNearSpawnPoint()
{
    if (spawnPoint == null) return transform.position;

    Vector3 basePos = spawnPoint.position;
    if (lockZAxis) basePos.z = lockZ;

    for (int t = 0; t < maxTry; t++)
    {
        float offsetX = Random.Range(-spawnSpreadX, spawnSpreadX);
        Vector3 candidate = basePos + new Vector3(offsetX, 0f, 0f);
        if (lockZAxis) candidate.z = lockZ;

        // ✅ 적 겹침 방지 체크
        bool overlapped = Physics.CheckSphere(candidate, minGap, enemyLayer, QueryTriggerInteraction.Ignore);
        if (!overlapped)
            return candidate;
    }

    return basePos;
}



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lightDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnDistance);
    }

}
