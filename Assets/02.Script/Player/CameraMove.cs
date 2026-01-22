/// <summary>
/// 카메라의 이동과 방향의 지정에 대한 스크립트입니다. 
/// (01.22) 스크립트 작성, 플레이어 추적/ 위 올려다보기 기능 추가
/// </summary>
using System.Collections;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
   [Header("플레이어 (추적 대상)")]
    public Transform target;                // 카메라가 따라 갈 대상

    [Header("추적 이동 관련 수치")]
    public float xOffset = 0f;              // X축 카메라 보정 값  
    public float xSmoothTime = 0.12f;       // x축 이동 보정 값

    [Header("Look Up Pulse (Press W)")]
    public float pulseHeight = 3f;          // 위로 올라갈 높이
    public float pulseHoldTime = 0.25f;     // 위에서 유지 시간
    public float ySmoothTime = 0.10f;       // Y 이동 부드러움

    [Header("참조 컴포넌트")]
    private InputManager inputManager;

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

        /// [강다영] 싱글톤 인스턴스인 InputManager를 추가해 입력을 일괄적으로 관리합니다. ///
        inputManager = InputManager.Instance;

    }

    void Update()
    {
        // "누른 순간"에만 트리거
        if (Input.GetKeyDown(inputManager.Lookup))
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

    /* 카메라 시점을 위로 올려 더 높은 시점을 올려다 보는 기능 */
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
