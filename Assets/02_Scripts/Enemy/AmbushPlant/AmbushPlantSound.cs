using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushPlantSound : MonoBehaviour
{
 [Header("Detect (XZ distance only)")]
    public Transform player;          // 비워두면 자동으로 Player 태그 탐색
    public float radius = 15f;        // 감지 반경 (수평 거리)
    public float hysteresis = 0.5f;   // 경계에서 깜빡임 방지

    [Header("Sound (3D Loop)")]
    public AudioClip loopClip;
    [Range(0f, 1f)] public float volume = 1f;
    public float fadeSpeed = 2f;

    [Header("3D Audio Range")]
    public float minDistance = 8f;
    public float maxDistance = 15f;

    AudioSource src;
    float targetVolume = 0f;
    bool isInside = false;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 1f;      // 3D
        src.volume = 0f;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;

        if (loopClip != null) src.clip = loopClip;
    }

    void Start()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    void Update()
    {
        if (player == null || src.clip == null) return;

        // ✅ Y 무시한 수평 거리(XZ)만 체크
        Vector3 a = transform.position; a.y = 0f;
        Vector3 b = player.position;    b.y = 0f;
        float dist = Vector3.Distance(a, b);

        // 경계에서 왔다갔다 할 때 깜빡이는 것 방지 (히스테리시스)
        float enterDist = radius;
        float exitDist  = radius + hysteresis;

        if (!isInside && dist <= enterDist)
        {
            isInside = true;
            targetVolume = volume;
            if (!src.isPlaying) src.Play();
        }
        else if (isInside && dist >= exitDist)
        {
            isInside = false;
            targetVolume = 0f;
        }

        // 페이드
        src.volume = Mathf.MoveTowards(src.volume, targetVolume, fadeSpeed * Time.deltaTime);

        // 완전히 꺼지면 정지
        if (!isInside && src.isPlaying && src.volume <= 0.01f)
            src.Stop();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 바닥(XZ) 기준 반경 시각화
        Gizmos.color = Color.red;
        Vector3 p = transform.position; p.y = 0f;
        Gizmos.DrawWireSphere(p, radius);
    }
#endif
}