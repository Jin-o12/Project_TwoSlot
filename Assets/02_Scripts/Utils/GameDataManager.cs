/// <summary>
/// 게임 전체의 데이터들을 계속해서 저장 및 관리 할 싱글톤
/// 총알, 체력, 아이템 등 유지 되어야 할 정보들을 저장해둡니다
/// </summary>
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameDataManager : MonoBehaviour
{
    // 싱글톤 제작
    public static GameDataManager Instance;

    [Header("데이터를 가져올 컴포넌트")]
    private PlayerItemTrigger inventory;    // 플레이어 움직임 스크립트
    private PlayerHealth playerHealth;      // 플레이어 체력 스크립트
    private GunFire playerGun;              // 플레이어 총 관련 수치 스크립트

    [Header("저장 할 플레이어 데이터")]
    public int currentHp;                   // 현재 체력
    public int currentAmmo;                 // 현재 총알 수
    public int savedMaxAmmo;                // 저장된 최대 총알 수
    public WeaponItem slot1Item;            // 현재 1번 슬롯의 아이템
    public WeaponItem slot2Item;            // 현재 2번 슬롯의 아이템
    public int score;                       // 총 점수
    bool hasSavedData = false;

    void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
            // 부모가 있다면 부모 해제 후 보존 (싱글톤이 상속되어 있기 때문에 안전장치 추가)
            transform.SetParent(null);
            // 씬이 바뀌어도 파괴되지 않음
            DontDestroyOnLoad(gameObject);
            
            // 씬 로드 완료 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // 씬 로드 과정에서 중복된 인스턴스가 생성되면 새 인스턴스를 제거하여 중복 방지
            Destroy(gameObject);
        }
    }
    void OnDestroy()
    {
        // 중복 구독 방지
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 바뀌고 난 뒤 자동으로 Player 찾아서 데이터 적용
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ChangePlayerObject();

        // 플레이어 캐릭터가 없는 씬이라면 실행 안 함
        if (playerHealth == null) return;

        // 데이터 저장 여부에 따른 분기 처리
        if (!hasSavedData)
        {
            // [첫 게임 시작] 초기 데이터 세팅 후 저장 플래그 활성화
            GameDataInitialize();
            Debug.Log("[INIT] 첫 번째 스테이지: 플레이어 데이터 초기화 완료");
        }
        else
        {
            // [씬 전환] 이미 저장된 데이터가 있다면 1프레임 뒤에 적용
            StartCoroutine(ApplyNextFrame());
            Debug.Log("[LOAD] 스테이지 이동: 저장된 데이터 복구 완료");
        }
    }

    public void ResetData()
    {
        hasSavedData = false;
        score = 0;
        // 필요 시 아이템이나 기타 변수들도 기본값으로 초기화
    }

    IEnumerator ApplyNextFrame()
    {
        yield return null; // ✅ 한 프레임 대기(플레이어 Start 끝난 뒤)

        if (playerHealth != null) playerHealth.SetHp(currentHp);
        if (playerGun != null)
        {
            playerGun.SetCurrentAmmo(currentAmmo);
            // playerGun.SetMaxAmmo(savedMaxAmmo); // 필요하면
        }

        if (inventory != null)
        {
            inventory.SetSlots(slot1Item, slot2Item);
        // 슬롯 UI/탄 UI 갱신 함수가 있으면 여기서 호출
            inventory.RefreshUI();     
        // playerGun.UpdateAmmoUI();  // 너 코드에 있으면 호출
        }
        Debug.Log($"[APPLY] hp={currentHp} ammo={currentAmmo}/{savedMaxAmmo}");
    }

    // 씬 전환 이전에 플레이어의 데이터를 받아서 저장해둔다
    public void SavePlayerData()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("SavePlayerData: player not found!");
            return;
        }
        var hp = player.GetComponent<PlayerHealth>();
        var gun = player.GetComponent<GunFire>();
        var inv = player.GetComponent<PlayerItemTrigger>();
        
        currentHp = hp.currentHp;
        currentAmmo = gun.GetCurrentAmmo();
        savedMaxAmmo = gun.GetMaxAmmo();
        
        slot1Item = inv.slot1;
        slot2Item = inv.slot2;
        
        hasSavedData = true;
        Debug.Log($"[SAVE] hp={currentHp} ammo={currentAmmo}/{savedMaxAmmo}");
    }

    /* 씬 전환시 새로 생성된 플레이어 오브젝트로 플레이어 데이터 새로 할당 */
    public void ChangePlayerObject()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if(player)
        {
            inventory = player.GetComponent<PlayerItemTrigger>();   // 아이템 슬롯 1, 2
            playerHealth = player.GetComponent<PlayerHealth>();     // 플레이어 체력
            playerGun = player.GetComponent<GunFire>();             // 플레이어 총알 갯수
        }
    }
    
    /* 게임 시작시 플레이어 데이터 일괄 초기화 하는 함수 */
    public void GameDataInitialize()
    {
        // 플레이어 데이터를 찾고, 수치 일괄 초기화
        ChangePlayerObject();

        playerHealth.Initialized();
        inventory.InitializedInventory();
        playerGun.InitializedGun();
        score = 0;
        
        // 초기값도 저장해두면 다음 씬에서도 동일하게 시작 가능
        SavePlayerData();
    }  

    public void AddScore(int plus) => score += plus;
}
