using UnityEngine;

public class UIPanelController : MonoBehaviour
{
    public void CloseSelf()
    {
        // 1. 만약 게임 중(GamePauseManager가 존재함)이라면?
        // -> 매니저한테 "나 끄고, 일시정지 화면 다시 켜줘"라고 부탁함
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.CloseSettings();
        }
        // 2. 만약 메인 메뉴라면? (매니저가 없음)
        // -> 그냥 나 자신만 끄면 됨 (메인 메뉴는 원래 뒤에 배경이 있으니까)
        else
        {
            gameObject.SetActive(false);
        }
    }
}