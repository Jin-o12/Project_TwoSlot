/// <summary>
/// 게임 전체의 데이터들을 계속해서 저장 및 관리 할 싱글톤
/// 총알, 체력, 아이템 등 유지 되어야 할 정보들을 저장해둡니다
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    [Header("플레이어 데이터")]
    public int currentHp;           // 현재 체력
    public int currentAmmo;         // 현재 총알 수
    public WeaponItem slot1Item;    // 현재 1번 슬롯의 아이템
    public WeaponItem slot2Item;    // 현재 2번 슬롯의 아이템
    public int score;               // 총 점수

    void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
            // 씬이 바뀌어도 파괴되지 않음
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 씬 로드 과정에서 중복된 인스턴스가 생성되면 새 인스턴스를 제거하여 중복 방지
            Destroy(gameObject);
        }
    }

    /* 씬 전환 이전에 플레이어의 데이터를 받아서 저장해둔다 */
    public void SavePlayerData(int _currentHp, int _currentAmmo, 
                               WeaponItem _slot1Item, WeaponItem _slot2Item, 
                               int _score)
    {
        currentHp = _currentHp;
        currentAmmo = _currentAmmo;
        slot1Item = _slot1Item;
        slot2Item = _slot2Item;
        score = _score;
    }
}
