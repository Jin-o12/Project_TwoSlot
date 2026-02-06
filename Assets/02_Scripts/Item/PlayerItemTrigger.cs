using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerItemTrigger : MonoBehaviour
{
    private WeaponItem equippedItem;   // 현재 손에 든 아이템
    public WeaponItem EquippedItem => equippedItem;
    private InputManager inputManager;
    public WeaponItem slot1                                     // 첫번째 아이템이 들어갈 변수
    { get; private set; }
    public WeaponItem slot2                                     // 두번째 아이템이 들어갈 변수
    { get; private set; }

    [Header("UI 피드백 설정")]
    [SerializeField] private float popScale = 1.2f;       // 얼마나 커질지 (1.2배)
    [SerializeField] private float popDuration = 0.15f;  // 커지는 시간

    // 애니메이션 중복 실행 방지를 위한 변수
    private Coroutine slot1AnimRoutine;
    private Coroutine slot2AnimRoutine;
    private Vector3 defaultScale = Vector3.one; // 원래 크기 저장용

    [Header("슬롯 아이콘 UI (Image)")]
    public Image slotIcon1;                                     // 아이템 슬롯 이미지
    public Image slotIcon2;                                     // 아이템 슬롯 이미지

    [Header("아이템 슬롯(1/2)")]
    [Range(1, 2)]
    public int selectedSlot;                                    // 선택 된 슬롯

    [Header("손 장착")]
    [SerializeField] private Transform handSocket;   // 오른손 본/손 위치
    private GameObject heldInstance;

    [Header("드롭 위치")]
    public Transform dropPoint;                                 // 아이템이 떨어질 위치

    [Header("현재 활성 모드")]
    public ActiveMode activeMode;                       // 현재 활성화 된 아이템 모드

    [Header("아이템 선택중(하이라이트만)")]
    public bool selectingItem = false;                  // 아이템을 선택 중인지에 대한 여부

    public enum ActiveMode { Gun, Item }                // 아이템 활성화 여부: 총 or 아이템(선택 슬롯)

    [Header("총 비활성화 제어")]
    [SerializeField] private GameObject gunRoot;                  // 총/손 모델 루트(또는 무기 전체)
    [SerializeField] private Behaviour[] gunBehaviours;           // 총 발사/조준 스크립트(선택)
    private Coroutine useRoutine;

    // ====== 편의 프로퍼티 ======
    public bool IsFull => slot1 != null && slot2 != null;
    public WeaponItem GetSelectedItem() => selectedSlot == 1 ? slot1 : slot2;

    // 총을 쏴도 되는지
    public bool CanFireGun => activeMode == ActiveMode.Gun;

    private void Awake()
    {
        inputManager = InputManager.Instance;
        // 👇 [추가] 게임 시작 시, 에디터에 설정된 원래 크기를 기억함
        if (slotIcon1 != null) defaultScale = slotIcon1.rectTransform.localScale;
    }
    void Start()
    {
        RefreshUI();
        SetGunActive(true);
    }

    /* 인벤토리 관련 기능 초기화 */
    public void InitializedInventory()
    {
        activeMode = ActiveMode.Gun;
        selectedSlot = 1;
        slot1 = null;
        slot2 = null;
        selectingItem = false;
        SetGunActive(true);
    }

    void Update()
    {
        if (GamePauseManager.Paused) return;   // ✅ 일시정지면 슬롯 입력 무시
        
        if (Input.GetKeyDown(inputManager.Slot1Key)) HandleSlotKey(1);
        else if (Input.GetKeyDown(inputManager.Slot2Key)) HandleSlotKey(2);
    }

    // ⭐ [핵심 수정] 원버튼 즉시 장착 로직
    void HandleSlotKey(int slotIndex)
    {
        // 1. 이미 '해당 아이템'을 들고 있다면? -> 총 모드로 복귀 (장착 해제)
        if (activeMode == ActiveMode.Item && selectedSlot == slotIndex)
        {
            SwitchToGun();
            return;
        }

        // 2. 다른 슬롯을 눌렀거나, 총을 들고 있는 상태라면 -> 즉시 교체 시도

        // 슬롯 인덱스 변경
        selectedSlot = slotIndex;

        // 해당 슬롯에 아이템이 없으면? -> 무시
        if (GetSelectedItem() == null)
        {
            Debug.Log("[ItemSelect] 슬롯이 비어있어서 장착 불가");
            return;
        }

        // 3. 즉시 장착 실행
        activeMode = ActiveMode.Item;
        selectingItem = false; // 하이라이트 단계 건너뜀

        // UI 튀어오르는 효과 실행
        TriggerSlotFeedback(slotIndex);

        Debug.Log($"[ItemSelect] 즉시 장착! (슬롯 {selectedSlot})");

        // 실제 모델 손에 들기 & 총 숨기기
        EquipSelectedToHand();
        SetGunActive(false);
    }

    // 아이템 사용 후 자동으로 총로 돌아가고 싶을 때 호출
    public void SwitchToGun()
    {
        activeMode = ActiveMode.Gun;
        selectingItem = false;

        UnequipHandItem();
        Debug.Log("[ItemSelect] 총 모드로 복귀");
        SetGunActive(true);

        // (선택 사항) 총으로 돌아갈 때 아이콘 크기 초기화하고 싶으면 아래 주석 해제
        
        if (slot1AnimRoutine != null) StopCoroutine(slot1AnimRoutine);
        if (slot2AnimRoutine != null) StopCoroutine(slot2AnimRoutine);
        slotIcon1.rectTransform.localScale = defaultScale;
        slotIcon2.rectTransform.localScale = defaultScale;
        
    }

    // ================== ✅ UI 애니메이션 로직 ==================

    void TriggerSlotFeedback(int slotIndex)
    {
        if (slotIndex == 1)
        {
            if (slot1AnimRoutine != null) StopCoroutine(slot1AnimRoutine);
            slot1AnimRoutine = StartCoroutine(PopIconAnimation(slotIcon1.rectTransform));

            if (slot2AnimRoutine != null) StopCoroutine(slot2AnimRoutine);
            slotIcon2.rectTransform.localScale = defaultScale;
        }
        else if (slotIndex == 2)
        {
            if (slot2AnimRoutine != null) StopCoroutine(slot2AnimRoutine);
            slot2AnimRoutine = StartCoroutine(PopIconAnimation(slotIcon2.rectTransform));

            if (slot1AnimRoutine != null) StopCoroutine(slot1AnimRoutine);
            slotIcon1.rectTransform.localScale = defaultScale;
        }
    }

    IEnumerator PopIconAnimation(RectTransform target)
    {
        float timer = 0f;

        // 커짐
        while (timer < popDuration)
        {
            timer += Time.deltaTime;
            float t = timer / popDuration;
            target.localScale = Vector3.Lerp(defaultScale, defaultScale * popScale, t);
            yield return null;
        }

        // 작아짐
        timer = 0f;
        while (timer < popDuration)
        {
            timer += Time.deltaTime;
            float t = timer / popDuration;
            target.localScale = Vector3.Lerp(defaultScale * popScale, defaultScale, t);
            yield return null;
        }

        target.localScale = defaultScale;
    }

    void EquipSelectedToHand()
    {
        if (heldInstance != null) Destroy(heldInstance);

        var item = GetSelectedItem();
        if (item == null || item.pickupPrefab == null) return;

        equippedItem = item;

        heldInstance = Instantiate(item.pickupPrefab, handSocket);
        heldInstance.transform.localPosition = item.heldLocalPos;
        heldInstance.transform.localRotation = Quaternion.Euler(item.heldLocalEuler);
        heldInstance.transform.localScale = item.heldLocalScale;
    }

    void UnequipHandItem()
    {
        if (heldInstance != null) Destroy(heldInstance);
        heldInstance = null;
        equippedItem = null;
    }

    // ================== 아이템 사용 시작/종료 API ==================
    public void BeginUseItem(float autoEndAfterSeconds = 0f)
    {
        if (useRoutine != null)
        {
            StopCoroutine(useRoutine);
            useRoutine = null;
        }

        activeMode = ActiveMode.Item;
        selectingItem = false;
        EquipSelectedToHand();
        SetGunActive(false);

        if (autoEndAfterSeconds > 0f)
            useRoutine = StartCoroutine(AutoEndUse(autoEndAfterSeconds));
    }

    public void EndUseItem()
    {
        if (useRoutine != null)
        {
            StopCoroutine(useRoutine);
            useRoutine = null;
        }
        UnequipHandItem();
        SwitchToGun();
    }

    IEnumerator AutoEndUse(float t)
    {
        yield return new WaitForSeconds(t);
        useRoutine = null;
        SwitchToGun();
    }

    void SetGunActive(bool on)
    {
        if (gunRoot != null) gunRoot.SetActive(on);

        if (gunBehaviours != null)
        {
            for (int i = 0; i < gunBehaviours.Length; i++)
                if (gunBehaviours[i] != null) gunBehaviours[i].enabled = on;
        }
    }

    // ================== 인벤/드롭 로직 ==================

    public void AddOrReplaceSelected(WeaponItem newItem)
    {
        if (newItem == null) return;

        if (slot1 == null)
        {
            slot1 = newItem;
            RefreshUI();
            return;
        }

        if (slot2 == null)
        {
            slot2 = newItem;
            RefreshUI();
            return;
        }

        if (selectedSlot == 1)
        {
            Drop(slot1);
            slot1 = newItem;
        }
        else
        {
            Drop(slot2);
            slot2 = newItem;
        }

        RefreshUI();
    }

    public bool TryConsume(WeaponItem item)
    {
        if (item == null) return false;

        if (slot1 == item)
        {
            slot1 = null;
            RefreshUI();
            return true;
        }

        if (slot2 == item)
        {
            slot2 = null;
            RefreshUI();
            return true;
        }

        return false;
    }

    void Drop(WeaponItem item)
    {
        if (item == null) return;

        if (item.pickupPrefab == null)
        {
            Debug.LogWarning($"[Drop] '{item.name}' pickupPrefab이 없어서 드랍하지 않음.");
            return;
        }

        Vector3 pos = dropPoint
            ? dropPoint.position
            : (transform.position + transform.forward * 0.8f + Vector3.up * 0.2f);

        Instantiate(item.pickupPrefab, pos, Quaternion.identity);
    }

    public void RefreshUI()
    {
        SetSlotIcon(slotIcon1, slot1);
        SetSlotIcon(slotIcon2, slot2);
    }
    public void SetSlots(WeaponItem s1, WeaponItem s2)
    {
        slot1 = s1;
        slot2 = s2;
        RefreshUI();
    }

    void SetSlotIcon(Image img, WeaponItem item)
    {
        if (img == null) return;
        // 👇 [추가] 어떤 상황이든 아이콘을 그릴 때는 크기를 '무조건' 정상으로 돌려놓음
        img.rectTransform.localScale = defaultScale;
        if (item == null || item.icon == null)
        {
            img.enabled = false;
            img.sprite = null;
            return;
        }

        img.enabled = true;
        img.sprite = item.icon;
    }
    public WeaponItem CurrentEquippedItem
    {
        get
        {
            if (activeMode != ActiveMode.Item) return null;
            return GetSelectedItem();
        }
    }
}