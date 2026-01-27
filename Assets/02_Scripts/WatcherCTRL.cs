using System.Collections;
using UnityEngine;

public class WatcherByDistance : MonoBehaviour
{
    [Header("카메라 세팅")]
    public Transform headTr;          // 회전할 카메라 머리
    public Transform playerTr;        // 플레이어
    public AudioSource alarmAudio;    // 경보 사운드(선택)

    [Header("감지 거리")]
    public float detectDistance = 8f;

    [Header("회전 속도")]
    public float rotateSpeed = 6f;

    [Header("소환")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public int spawnCount = 2;
    public float spawnInterval = 0.3f;

    private bool detected = false;
    private bool alarmTriggered = false;

    void Start()
    {
        if (playerTr == null)
            playerTr = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        if (playerTr == null || headTr == null) return;

        float dist = Vector3.Distance(transform.position, playerTr.position);

        //  감지
        detected = dist <= detectDistance;

        if (detected)
        {
            // 응시(2.5D라 Y축 회전만)
            LookAtPlayer();

            // 경보 + 소환 1회
            if (!alarmTriggered)
            {
                alarmTriggered = true;
                StartCoroutine(AlarmAndSpawn());
            }
        }
        else
        {
            
            //alarmTriggered = false;
        }
    }

    void LookAtPlayer()
    {
        Vector3 target = playerTr.position;
        target.y -= 1.0f;

        Vector3 dir = playerTr.position - headTr.position;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        headTr.rotation = Quaternion.Slerp(headTr.rotation, targetRot, Time.deltaTime * rotateSpeed);
    }

    IEnumerator AlarmAndSpawn()
    {
        if (alarmAudio != null)
            alarmAudio.Play();

        if (enemyPrefab != null && spawnPoint != null)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }
}
