using UnityEngine;
using System.Collections;

public class AmbientSoundPlayer : MonoBehaviour
{
    [Header("오디오 소스 연결")]
    public AudioSource sfxSource; // BGM용 말고, 새로 추가할 소스

    [Header("환경음 클립들")]
    public AudioClip[] ambientClips; // 여기에 준비한 SFX들을 다 넣으세요

    [Header("재생 간격 설정 (초)")]
    public float minInterval = 5.0f;  // 최소 몇 초 뒤에 나올지
    public float maxInterval = 15.0f; // 최대 몇 초 안에 나올지

    [Header("볼륨 랜덤 설정")]
    public float minVolume = 0.5f;
    public float maxVolume = 1.0f;

    void Start()
    {
        // 시작하자마자 코루틴(반복 작업) 시작
        StartCoroutine(PlayRandomAmbient());
    }

    IEnumerator PlayRandomAmbient()
    {
        while (true) // 무한 반복
        {
            // 1. 랜덤한 시간만큼 대기
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // 2. 소리 재생
            PlaySound();
        }
    }

    void PlaySound()
    {
        if (ambientClips.Length == 0 || sfxSource == null) return;

        // 랜덤한 클립 하나 뽑기
        int randomIndex = Random.Range(0, ambientClips.Length);
        AudioClip clip = ambientClips[randomIndex];

        // 랜덤한 볼륨 설정
        float randomVol = Random.Range(minVolume, maxVolume);

        // PlayOneShot: 겹쳐서 재생 가능, BGM 끊김 없음
        sfxSource.PlayOneShot(clip, randomVol);
    }
}