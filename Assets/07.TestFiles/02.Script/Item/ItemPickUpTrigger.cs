using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))] 
// 이 스크립트가 붙은 오브젝트에는 반드시 Collider가 있어야 한다는 의미
// 없으면 Unity가 자동으로 추가해줌
public class ItemPickUpTrigger : MonoBehaviour
{
    [Header("Item ID (프리팹마다 다르게)")]
    public string itemId = "";
    // 이 아이템이 어떤 아이템인지 구분하는 ID (예: "Potion", "Key")

    [Header("UI")]
    public string promptFormat = "F : 줍기 ({0})";
    // 플레이어가 근처에 왔을 때 보여줄 UI 문구
    // {0} 자리에 itemId가 들어감 → "F : 줍기 (GlowStick)"

    [Header("옵션")]
    public bool destroyOnPickup = true;
    // 아이템을 주웠을 때 오브젝트를 삭제할지 여부

    bool _playerIn; 
    // 플레이어가 범위 안에 있는지 여부

    PlayerItemTrigger _inv;
    // 플레이어의 인벤토리 스크립트를 저장할 변수

    void Reset()
    {
        // Reset()은 컴포넌트를 처음 붙일 때 자동 실행됨
        // 콜라이더를 트리거로 자동 설정
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // 플레이어가 트리거 범위에 들어왔을 때 실행
        if (!other.CompareTag("Player")) return;

        _playerIn = true;

        // 플레이어 오브젝트에서 PlayerItemTrigger(인벤토리) 찾기
        _inv = other.GetComponent<PlayerItemTrigger>() 
            ?? other.GetComponentInParent<PlayerItemTrigger>();

        // 인벤토리가 있다면 UI 표시
        //if (_inv != null)
            //PickupUI.Instance?.Show(string.Format(promptFormat, itemId));
    }

    void OnTriggerExit(Collider other)
    {
        // 플레이어가 범위를 벗어났을 때 실행
        if (!other.CompareTag("Player")) return;

        _playerIn = false;
        _inv = null;

        // UI 숨기기
        //PickupUI.Instance?.Hide();
    }

    void Update()
    {
        // 플레이어가 범위 안에 없거나 인벤토리가 없으면 아무것도 안 함
        if (!_playerIn || _inv == null) return;

        // F키를 눌렀을 때 아이템 줍기
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 인벤토리에 아이템 추가
            _inv.AddOrReplaceSelected(itemId);

            // UI 숨기기
            //PickupUI.Instance?.Hide();

            // 아이템 오브젝트 삭제 또는 비활성화
            if (destroyOnPickup)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}