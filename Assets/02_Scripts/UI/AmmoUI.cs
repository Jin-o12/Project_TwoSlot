using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필수

public class AmmoUI : MonoBehaviour
{
    [Header("UI 텍스트")]
    public TextMeshProUGUI ammoText;            // 총알 텍스트 UI

    [Header("참조 컴포넌트")]
    public GunFire gunFireScript;               // 총 발사에 관한 스크립트

    [Header("총알 갯수에 따른 UI 색상")]
    public Color normalColor = Color.white;     // 평소의 색깔
    public Color emptyColor = Color.red;        // 장전된 총알이 없을 시 UI 색깔

    private int current;
    private int max;

    void Update()
    {
        // 1. 총 발사에 관한 스크립트가 없을 시 총알 갯수를 표시하지 않음
        if (gunFireScript == null)
        {
            ammoText.text = "- / -";
            return;
        }

        UpdateBulletNumText();

        // 4. 장탄 수에 따른 UI 색상 변경
        if (current <= 0)
            ammoText.color = emptyColor;
        else
            ammoText.color = normalColor;
    }

    private void UpdateBulletNumText()
    {
        // 2. GunFire 스크립트로 부터 현재 정보를 가져옴
        current = gunFireScript.currentAmmo; // 현재 총알 갯수
        max = gunFireScript.maxAmmo;         // 최대 총알 갯수

        // 3. "현재 총알 갯수/최대 총알 갯수"가 UI 상에 표시
        ammoText.text = $"{current}/{max}";
    }
}