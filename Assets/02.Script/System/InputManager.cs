/// <summary>
/// PC 플레이어의 키 할당을 일괄 관리하기 위한 InputManager 인스턴스 선언 스크립트입니다.
/// (01.22) 스크립트 작성, 달리기/ 
/// </summary>
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;    // 싱글톤 제작

    void Awake()
    {
        // 싱글톤 보장: 인스턴스가 존재하지 않을 시 자신을 인스턴스화 하고 파괴할 수 없게 함.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // 이미 존재 할 시 기존의 것을 삭제하여 중복 되는 일이 없게 함
        else
        {
            Destroy(gameObject);
        }
    }

    public KeyCode Run = KeyCode.LeftShift;
    /// 
    public KeyCode Lookup = KeyCode.W;
}
