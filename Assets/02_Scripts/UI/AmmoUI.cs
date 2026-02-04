using UnityEngine;
using TMPro; // 텍스트메쉬프로 사용 필수

public class AmmoUI : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI ammoText; // 숫자를 표시할 텍스트 UI

    [Header("데이터 연결")]
    // 플레이어의 총 시스템(GunFire)을 연결할 변수
    public GunFire gunFireScript;

    [Header("색상 설정 (옵션)")]
    public Color normalColor = Color.white;  // 평소 색깔
    public Color emptyColor = Color.red;     // 총알 없을 때 색깔

    void Update()
    {
        // 1. 연결된 총 스크립트가 없으면 작동 중지 (에러 방지)
        if (gunFireScript == null)
        {
            ammoText.text = "- / -";
            return;
        }

        // 2. GunFire 스크립트에서 탄약 정보 가져오기
        int current = gunFireScript.currentAmmo; // 현재 탄창 (6)
        int max = gunFireScript.maxAmmo;         // 남은 탄약 (30)

        // 3. 텍스트 갱신 (예: "6/30")
        
        ammoText.text = $"{current}/{max}";

        // 4. (추가 기능) 총알이 0발이면 빨간색으로 경고
        if (current <= 0)
            ammoText.color = emptyColor;
        else
            ammoText.color = normalColor;
    }
}