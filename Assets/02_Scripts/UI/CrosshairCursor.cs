using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairCursor : MonoBehaviour
{
    [SerializeField] private Texture2D crosshairTexture;

    // 핫스팟: 커서 클릭 위치(조준점 위치)
    // 보통 크로스헤어는 중앙이 핫스팟이 좋음
    void Start()
    {
        if (crosshairTexture == null) return;

        Vector2 hotspot = new Vector2(crosshairTexture.width * 0.5f, crosshairTexture.height * 0.5f);
        Cursor.SetCursor(crosshairTexture, hotspot, CursorMode.Auto);

        // FPS처럼 커서를 안 보이게 하고 UI 크로스헤어를 쓰는 경우가 아니라면
        // 잠금은 선택사항이야.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
