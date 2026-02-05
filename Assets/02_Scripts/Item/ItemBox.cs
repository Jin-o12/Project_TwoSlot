using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemBox : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject dropPrefab;   // 드롭될 아이템 프리팹
    public Transform spawnPoint;    // 박스 위/앞 위치
    public float popUpHeight = 1.0f;
    public float popUpTime = 0.12f;
    public float fallTime = 0.18f;

    bool playerIn;
    bool opened;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Update()
    {
        if (!playerIn || opened) return;

        if (FPromptUI.I && !FPromptUI.I.IsShowing(transform))
            FPromptUI.I.Show(transform);

        if (Input.GetKeyDown(KeyCode.F))
        {
            opened = true;
            if (FPromptUI.I) FPromptUI.I.Hide();
            StartCoroutine(SpawnPopDrop());
        }
    }

    IEnumerator SpawnPopDrop()
    {
        if (!dropPrefab) yield break;

        Vector3 basePos = spawnPoint ? spawnPoint.position : (transform.position + Vector3.up * 1.0f);
        GameObject go = Instantiate(dropPrefab, basePos, Quaternion.identity);

        // ✅ 1) 연출 중에는 줍기 금지: 픽업 트리거/콜라이더 잠그기
        LockPickup(go, true);

        // 물리 흔들림 방지: 잠깐 kinematic으로 연출
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Vector3 upPos = basePos + Vector3.up * popUpHeight;

        // 1) 위로 툭
        float t = 0f;
        while (t < popUpTime)
        {
            if (!go) yield break; // 안전장치
            t += Time.deltaTime;
            float a = t / popUpTime;
            go.transform.position = Vector3.Lerp(basePos, upPos, a);
            yield return null;
        }

        // 2) 아래로 툭
        t = 0f;
        while (t < fallTime)
        {
            if (!go) yield break; // 안전장치
            t += Time.deltaTime;
            float a = t / fallTime;
            go.transform.position = Vector3.Lerp(upPos, basePos, a);
            yield return null;
        }

        // ✅ 2) 연출 끝: 다시 물리 활성 + 줍기 허용
        if (go && rb) rb.isKinematic = false;

        LockPickup(go, false);

        // 박스는 잠깐 뒤 파괴
        Destroy(gameObject, 0.1f);
    }

    // ✅ 드랍 아이템의 "줍기" 기능 잠그기/해제
    void LockPickup(GameObject go, bool locked)
    {
        if (!go) return;

        // 1) 네가 쓰는 픽업 스크립트 잠그기
        var pickup = go.GetComponentInChildren<ItemPickUpTrigger>(true);
        if (pickup) pickup.enabled = !locked;

        // 2) 트리거 콜라이더도 잠그면 더 확실 (ItemPickUpTrigger가 붙어있는 오브젝트 기준)
        //    (프리팹 구조에 따라 자식에 있을 수 있어서 InChildren 사용)
        var cols = go.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
        {
            // "픽업 트리거"만 끄고 싶으면 tag/layer/name으로 필터링 가능.
            // 여기서는 안전하게 trigger만 끔 (물리 바닥 충돌 collider는 보통 isTrigger=false)
            if (c.isTrigger) c.enabled = !locked;
        }
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

    void OnDisable()
    {
        // 박스가 꺼질 때 코루틴이 남아있으면 정리
        StopAllCoroutines();
    }
}
