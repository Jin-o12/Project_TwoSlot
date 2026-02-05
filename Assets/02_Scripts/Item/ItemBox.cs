using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemBox : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject dropPrefab;   // 드롭될 아이템 프리팹
    public Transform spawnPoint;    // 박스 위/앞 위치
    public AudioSource source;
    public AudioClip openClip;
    public float popUpHeight = 1.0f;
    public float popUpTime = 0.12f;
    public float fallTime = 0.18f;

    bool playerIn;
    bool opened;

    void Reset()
    {
        // 자동으로 trigger collider 추천
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Update()
    {
        if (!playerIn || opened) return;

        // F UI 표시
        if (FPromptUI.I && !FPromptUI.I.IsShowing(transform))
            FPromptUI.I.Show(transform);

        if (Input.GetKeyDown(KeyCode.F))
        {
            opened = true;
            // 오픈 사운드
            if (source && openClip)
                source.PlayOneShot(openClip, 0.3f);
            if (FPromptUI.I) FPromptUI.I.Hide();
            StartCoroutine(SpawnPopDrop());
        }
    }

    IEnumerator SpawnPopDrop()
    {
        if (!dropPrefab) yield break;

        Vector3 basePos = spawnPoint ? spawnPoint.position : (transform.position + Vector3.up * 1.0f);
        GameObject go = Instantiate(dropPrefab, basePos, Quaternion.identity);

        // 물리 흔들림 방지: 잠깐 kinematic으로 연출
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb) { rb.isKinematic = true; rb.velocity = Vector3.zero; }

        Vector3 upPos = basePos + Vector3.up * popUpHeight;

        // 1) 위로 툭
        float t = 0f;
        while (t < popUpTime)
        {
            t += Time.deltaTime;
            float a = t / popUpTime;
            go.transform.position = Vector3.Lerp(basePos, upPos, a);
            yield return null;
        }

        // 2) 아래로 툭
        t = 0f;
        while (t < fallTime)
        {
            t += Time.deltaTime;
            float a = t / fallTime;
            go.transform.position = Vector3.Lerp(upPos, basePos, a);
            yield return null;
        }

        if (rb) rb.isKinematic = false; // 다시 물리로 떨어지게
        Destroy(gameObject, 0.1f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerIn = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerIn = false;
        if (FPromptUI.I && FPromptUI.I.IsShowing(transform))
            FPromptUI.I.Hide();
    }
}
