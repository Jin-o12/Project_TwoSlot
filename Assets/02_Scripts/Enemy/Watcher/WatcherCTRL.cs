using System.Collections;
using UnityEngine;

public class WatcherByDistance : MonoBehaviour
{
    [Header("References")]
    public Transform headTr;
    public Transform playerTr;

    [Header("SFX - Crossfade (AudioSource 2개 필요)")]
    public AudioSource alarmA;        // AudioSource 1
    public AudioSource alarmB;        // AudioSource 2
    public AudioClip warnClip;        // 노랑 구간 클립
    public AudioClip alertClip;       // 빨강 구간 클립
    public float fadeTime = 0.35f;    // 페이드 시간(0.2~0.6 추천)

    public Light warningLight;

    [Header("Distances")]
    public float lightDistance = 15f; // 노랑 경고
    public float spawnDistance = 10f; // 빨강 경보 + 소환

    [Header("Look")]
    public float rotateSpeed = 6f;

    [Header("Spawn")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public int spawnCount = 2;
    public float spawnInterval = 0.3f;

    [Header("Spawn - Around SpawnPoint")]
    public float spawnSpreadX = 2.0f;
    public float minGap = 0.8f;
    public int maxTry = 15;
    public LayerMask enemyLayer;
    public bool lockZAxis = true;
    public float lockZ = 0f;

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

    // ===== Crossfade 내부 변수 =====
    private AudioSource currentSource = null;
    private Coroutine fadeCo = null;

    void Start()
    {
        if (playerTr == null)
            playerTr = GameObject.FindWithTag("Player")?.transform;

        if (warningLight != null)
            warningLight.enabled = false;

        SetupSource(alarmA);
        SetupSource(alarmB);

        // 시작은 무음
        if (alarmA != null) alarmA.volume = 0f;
        if (alarmB != null) alarmB.volume = 0f;
    }

    void SetupSource(AudioSource s)
    {
        if (s == null) return;
        s.playOnAwake = false;
        s.loop = true;
    }

    void Update()
    {
        if (playerTr == null || headTr == null) return;

        float dist = Vector3.Distance(transform.position, playerTr.position);

        bool inWarnRange = dist <= lightDistance;
        bool inAlertRange = dist <= spawnDistance;

        // 50m(노랑) 이내: 응시
        if (inWarnRange)
            LookAtPlayer();

        // 라이트 연출
        UpdateWarningLight(inWarnRange, inAlertRange);

        // 사운드 연출 (크로스페이드)
        UpdateSound(inWarnRange, inAlertRange);

        // 20m(빨강) 이내: 소환(1회)
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

    // ✅ 노랑/빨강 클립을 다르게 + 페이드로 부드럽게 전환
    void UpdateSound(bool inWarnRange, bool inAlertRange)
    {
        // AudioSource 2개 없으면 동작 불가
        if (alarmA == null || alarmB == null) return;

        // 범위 밖이면 페이드아웃 후 종료
        if (!inWarnRange)
        {
            FadeTo(null, 0f, 1f);
            return;
        }

        // 노랑 구간에서 소리 안 쓰는 옵션이면(그리고 빨강도 아니면) 끄기
        if (!playSoundInWarnRange && !inAlertRange)
        {
            FadeTo(null, 0f, 1f);
            return;
        }

        // 어떤 클립을 쓸지 결정
        AudioClip targetClip = inAlertRange ? alertClip : warnClip;
        if (targetClip == null)
        {
            // 해당 구간 클립이 없으면 그냥 끄기
            FadeTo(null, 0f, 1f);
            return;
        }

        float vol = inAlertRange ? alertVolume : warnVolume;
        float pitch = inAlertRange ? alertPitch : warnPitch;

        FadeTo(targetClip, vol, pitch);
    }

    // nextClip == null 이면 둘 다 페이드아웃
    void FadeTo(AudioClip nextClip, float nextVol, float nextPitch)
    {
        // 다음에 틀 소스 선택(현재 소스의 반대)
        AudioSource next = (currentSource == alarmA) ? alarmB : alarmA;

        // ✅ 끄기(둘 다 페이드아웃)
        if (nextClip == null)
        {
            if (fadeCo != null) StopCoroutine(fadeCo);
            fadeCo = StartCoroutine(FadeOutBoth());
            currentSource = null;
            return;
        }

        // ✅ 이미 같은 클립이 current에서 재생 중이면 볼륨/피치만 부드럽게 맞춤
        if (currentSource != null && currentSource.clip == nextClip && currentSource.isPlaying)
        {
            currentSource.pitch = nextPitch;
            if (fadeCo != null) StopCoroutine(fadeCo);
            fadeCo = StartCoroutine(FadeVolume(currentSource, nextVol, fadeTime));
            return;
        }

        // ✅ next 소스에 새 클립 세팅하고 0볼륨에서 시작
        next.clip = nextClip;
        next.pitch = nextPitch;
        next.volume = 0f;
        if (!next.isPlaying) next.Play();

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(CrossFade(currentSource, next, nextVol, fadeTime));

        currentSource = next;
    }

    IEnumerator CrossFade(AudioSource from, AudioSource to, float toVol, float time)
    {
        float t = 0f;
        float fromStart = (from != null) ? from.volume : 0f;
        float toStart = (to != null) ? to.volume : 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            float a = (time <= 0f) ? 1f : Mathf.Clamp01(t / time);

            if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, a);
            if (to != null)   to.volume   = Mathf.Lerp(toStart, toVol, a);

            yield return null;
        }

        if (from != null)
        {
            from.volume = 0f;
            from.Stop();
            from.clip = null;
        }

        if (to != null)
            to.volume = toVol;
    }

    IEnumerator FadeOutBoth()
    {
        float t = 0f;
        float aStart = alarmA.volume;
        float bStart = alarmB.volume;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = (fadeTime <= 0f) ? 1f : Mathf.Clamp01(t / fadeTime);

            alarmA.volume = Mathf.Lerp(aStart, 0f, a);
            alarmB.volume = Mathf.Lerp(bStart, 0f, a);

            yield return null;
        }

        alarmA.volume = 0f; alarmA.Stop(); alarmA.clip = null;
        alarmB.volume = 0f; alarmB.Stop(); alarmB.clip = null;
    }

    IEnumerator FadeVolume(AudioSource src, float target, float time)
    {
        if (src == null) yield break;

        float start = src.volume;
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            float a = (time <= 0f) ? 1f : Mathf.Clamp01(t / time);
            src.volume = Mathf.Lerp(start, target, a);
            yield return null;
        }

        src.volume = target;
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
