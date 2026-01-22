using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    [Header("UI 및 슬롯 상태")]
    public GameObject slot1IconObj;
    public GameObject slot2IconObj;
    public bool isSlot1Full = false;
    public bool isSlot2Full = false;

    // 현재 슬롯에 담긴 아이템의 종류를 기억하는 변수
    private string slot1ItemName = "";
    private string slot2ItemName = "";

    [Header("아이템 이미지 에셋 (새 아이템은 이곳에 등록)")]
    public Sprite noiseLureSprite; // 소음 미끼 
    public Sprite flareSprite;     // 화학 조명탄 
    // public Sprite healkitSprite;   // 구급키트
    // [확장 지점] 여기에 새로운 아이템의 Sprite 변수를 계속 추가하세요. 
    // 예: public Sprite specialKeySprite; 

    void Start()
    {
        slot1IconObj.SetActive(false);
        slot2IconObj.SetActive(false);
    }

    void Update()
    {
        // [테스트용] 기획서에 명시된 기본 아이템 습득 테스트 
        if (Input.GetKeyDown(KeyCode.Alpha1)) GetItem(noiseLureSprite, "NoiseLure");
        if (Input.GetKeyDown(KeyCode.Alpha2)) GetItem(flareSprite, "Flare");

        // [테스트용] 1번 슬롯 사용(Q), 2번 슬롯 사용(E)
        if (Input.GetKeyDown(KeyCode.Q)) UseItem(1);
        if (Input.GetKeyDown(KeyCode.E)) UseItem(2);
    }

    // 아이템 습득 공통 함수
    public void GetItem(Sprite itemImage, string itemName)
    {
        if (isSlot1Full == false)
        {
            slot1IconObj.GetComponent<Image>().sprite = itemImage;
            slot1IconObj.SetActive(true);
            slot1ItemName = itemName;
            isSlot1Full = true;
        }
        else if (isSlot2Full == false)
        {
            slot2IconObj.GetComponent<Image>().sprite = itemImage;
            slot2IconObj.SetActive(true);
            slot2ItemName = itemName;
            isSlot2Full = true;
        }
        else { Debug.Log("인벤토리가 가득 찼습니다! (최대 2슬롯)"); }

    }

    // 아이템 사용 공통 함수
    public void UseItem(int slotNumber)
    {
        if (slotNumber == 1 && isSlot1Full)
        {
            ApplyEffect(slot1ItemName); // 아이템별 고유 효과 실행
            slot1IconObj.SetActive(false);
            isSlot1Full = false;
            slot1ItemName = "";
        }
        else if (slotNumber == 2 && isSlot2Full)
        {
            ApplyEffect(slot2ItemName);
            slot2IconObj.SetActive(false);
            isSlot2Full = false;
            slot2ItemName = "";
        }
        else { Debug.Log(slotNumber + "번 슬롯이 비어있습니다."); }
    }


    // 아이템 효과 분류
    // 새로운 아이템을 추가했다면, 아래에 else if 문을 추가하여 연결

    void ApplyEffect(string itemName)
    {
        if (itemName == "NoiseLure") { ThrowNoiseLure(); }
        else if (itemName == "Flare") { UseFlare(); }
        else if (itemName == "Healkit") { UseHealkit(); }

        // [예시] 새로운 아이템 효과 연결
        // else if (itemName == "NewItemName") { UseNewItem(); }
    }


    // [확장 지점 2] 실제 아이템 세부 효과 구현
    // 각 아이템이 실제로 어떤 일을 할지 메소드를 작성하는 공간입니다.


  
    void ThrowNoiseLure() // 소음 미끼 [cite: 51]
    {
        Debug.Log("소음 미끼 투척! 적의 인식을 한곳으로 모읍니다.");
        // TODO: 적을 유인하는 사운드 및 로직 구현 [cite: 51, 76]
    }

    
    void UseFlare() // 화학 조명탄 [cite: 52]
    {
        Debug.Log("조명탄 작동! 안개를 일시적으로 제거하고 시야를 확보합니다.");
        // TODO: 안개 제거 연출 및 적 유인 리스크 구현
    }

    
    void UseHealkit() // 에너지 드링크
    {
        Debug.Log("에너지 드링크 섭취! 이동속도가 일시적으로 증가 합니다!");
        // 캐릭터의 이동속도가 일정시간 증가하는 효과 구현
    }

 
    /*void UseNewItem()
    {
        Debug.Log("새로운 아이템의 효과가 발동되었습니다!");
    }*/
}