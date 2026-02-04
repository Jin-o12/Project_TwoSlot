/// <summary>
/// 게임 전체의 데이터들을 계속해서 저장 및 관리 할 싱글톤
/// 총알, 체력, 아이템 등 유지 되어야 할 정보들을 저장해둡니다
/// 
/// (02.03/강다영) 플레이어의 데이터를 저장한느 과정에서 코드가 난잡합니다. 이후 최적화 할 예정입니다. 
/// </summary>
using UnityEngine;

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
    public WeaponItem slot1Item;            // 현재 1번 슬롯의 아이템
    public WeaponItem slot2Item;            // 현재 2번 슬롯의 아이템
    public int score;                       // 총 점수

    void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
            // 부모가 있다면 부모 해제 후 보존 (싱글톤이 상속되어 있기 때문에 안전장치 추가)
            transform.SetParent(null);
            // 씬이 바뀌어도 파괴되지 않음
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 씬 로드 과정에서 중복된 인스턴스가 생성되면 새 인스턴스를 제거하여 중복 방지
            Destroy(gameObject);
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
    }

    /* 씬 전환 이전에 플레이어의 데이터를 받아서 저장해둔다 */
    public void SavePlayerData()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) {
            Debug.LogWarning("SavePlayerData: player not found!");
            return;
        }
        currentHp = player.GetComponent<PlayerHealth>().currentHp;
        currentAmmo = player.GetComponent<GunFire>().currentAmmo;
        slot1Item = player.GetComponent<PlayerItemTrigger>().slot1;
        slot2Item = player.GetComponent<PlayerItemTrigger>().slot2;
    }

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

    public void AddScore(int plus)
    {
        score += plus;
    }
}
