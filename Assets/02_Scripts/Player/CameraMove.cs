using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
   [Header("Target")]
    public Transform target;

    [Header("X Follow")]
    public float xOffset = 0f;
    public float xSmoothTime = 0.12f;

    [Header("Look Up Pulse (Press W)")]
    public KeyCode pulseKey = KeyCode.W;
    public float pulseHeight = 3f;          // 위로 올라갈 높이
    public float pulseHoldTime = 0.25f;     // 위에서 유지 시간
    public float ySmoothTime = 0.10f;       // Y 이동 부드러움


    float xVel;
    float yVel;

    float baseY;
    float fixedZ;

    float yExtra;               // baseY에 더해지는 값(0~pulseHeight)
    Coroutine pulseRoutine;

    void Start()
    {
        baseY = transform.position.y;
        fixedZ = transform.position.z;
    }

    void Update()
    {
        // 누르는 동안 위를 올려다 봄
        if (Input.GetKey(pulseKey))
        {
            TriggerPulse();
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // X는 항상 추적
        float desiredX = target.position.x + xOffset;
        float newX = (xSmoothTime <= 0f)
            ? desiredX
            : Mathf.SmoothDamp(transform.position.x, desiredX, ref xVel, xSmoothTime);

        // Y는 baseY + yExtra 로 목표 설정
        float desiredY = baseY + yExtra;
        float newY = (ySmoothTime <= 0f)
            ? desiredY
            : Mathf.SmoothDamp(transform.position.y, desiredY, ref yVel, ySmoothTime);

        transform.position = new Vector3(newX, newY, fixedZ);
    }

    public void TriggerPulse()
    {
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulseRoutine = StartCoroutine(PulseCoroutine());
    }

    IEnumerator PulseCoroutine()
    {
        // 위로 올라가기
        yExtra = pulseHeight;

        // 위에서 잠깐 유지
        yield return new WaitForSeconds(pulseHoldTime);

        // 자동으로 내려오기
        yExtra = 0f;

        pulseRoutine = null;
    }

    // 필요하면 (예: 씬 전환/카메라 리셋 후) 기본 Y를 재설정
    public void RecalibrateBaseY()
    {
        baseY = transform.position.y;
    }
}
