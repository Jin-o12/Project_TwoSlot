using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPromptUI : MonoBehaviour
{
    public static FPromptUI I;
    public Image icon;          // F 이미지
    public Vector3 worldOffset = new Vector3(0, 1.5f, 0);

    Camera cam;
    Transform target;

    void Awake()
    {
        I = this;
        cam = Camera.main;
        Hide();
    }

    void LateUpdate()
    {
        if (!target || !icon) return;

        Vector3 wpos = target.position + worldOffset;
        Vector3 spos = cam.WorldToScreenPoint(wpos);

        // 카메라 뒤면 숨김
        icon.enabled = (spos.z > 0f);
        icon.transform.position = spos;
    }

    public void Show(Transform t)
    {
        target = t;
        if (icon) icon.enabled = true;
    }

    public void Hide()
    {
        target = null;
        if (icon) icon.enabled = false;
    }

    public bool IsShowing(Transform t) => target == t;
}
