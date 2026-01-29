using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAndFlip : MonoBehaviour
{

    [Header("Refs")]
    public Camera cam;
    public Transform playerRoot;      // 캐릭터 전체를 뒤집을 루트 (Yaw 180)
    public Transform aimPivot;        // 어깨/가슴 기준점 (Spine1 등)
    public Transform ikTarget;        // RightHand_IK_Target

    [Header("Aim Plane")]
    public float aimPlaneZ = 0f;      // 2.5D면 고정 Z
    public float targetDistance = 1.2f; // pivot에서 타겟까지 거리(조준 길이)

    [Header("Clamp & Flip")]
    public float maxArmAngle = 180f;  // 팔이 허용하는 회전 범위(도)
    public float flipHysteresis = 5f; // 경계에서 덜덜 떨림 방지

    [Header("Smoothing")]
    public float posSmooth = 25f;
    public float rotSmooth = 25f;

    // 내부 상태: 현재 캐릭터가 +X를 바라보는지(-X를 바라보는지)
    // +X면 facing = +1, -X면 facing = -1
    int facing = 1;
    public float bodyTurnSpeed = 15f;
    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!playerRoot) playerRoot = transform;
    }

    void LateUpdate()
    {
        if (!cam || !playerRoot || !aimPivot || !ikTarget) return;

        // 1) 마우스를 월드로 (Z 평면 투영)
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, aimPlaneZ));
        if (!plane.Raycast(ray, out float enter)) return;
        Vector3 mouseWorld = ray.GetPoint(enter);

        // 2) pivot -> mouse 방향 (2D)
        Vector3 dir = mouseWorld - aimPivot.position;
        dir.z = 0f;
        if (dir.sqrMagnitude < 0.000001f) return;
        dir.Normalize();

        // 3) "현재 바라보는 방향 기준"으로 각도 계산
        //    기준축: 바라보는 쪽의 +X (facing=+1이면 +X, -1이면 -X)
        Vector3 baseAxis = (facing == 1) ? Vector3.right : Vector3.left;

        // signed angle: baseAxis -> dir (Z축 기준)
        float signed = Vector3.SignedAngle(baseAxis, dir, Vector3.forward);

        // 4) 팔 허용각(=maxArmAngle) 밖이면 캐릭터 뒤집기
        //    예: maxArmAngle=180 => signed가 -90~+90 범위를 넘으면 뒤집는 식으로 동작하게 만들려면
        //    half = 90
        float half = maxArmAngle * 0.5f;

        if (signed > half + flipHysteresis)
        {
            Flip();
            baseAxis = (facing == 1) ? Vector3.right : Vector3.left;
            signed = Vector3.SignedAngle(baseAxis, dir, Vector3.forward);
        }
        else if (signed < -half - flipHysteresis)
        {
            Flip();
            baseAxis = (facing == 1) ? Vector3.right : Vector3.left;
            signed = Vector3.SignedAngle(baseAxis, dir, Vector3.forward);
        }

        // 5) 팔 각도는 clamp
        float clamped = Mathf.Clamp(signed, -half, half);

        // 6) IK Target 위치/회전 세팅
        //    - 위치: pivot에서 clamped 방향으로 targetDistance 만큼
        //    - 회전: +X가 총구 방향(너가 말한 기준)이라면, ikTarget의 +X가 조준 방향을 바라보게
        Vector3 aimDir = Quaternion.AngleAxis(clamped, Vector3.forward) * baseAxis;
        Vector3 targetPos = aimPivot.position + aimDir * targetDistance;
        targetPos.z = aimPlaneZ;

        Quaternion targetRot = Quaternion.FromToRotation(Vector3.forward, aimDir); // +X가 조준방향

        ikTarget.position = Vector3.Lerp(ikTarget.position, targetPos, Time.deltaTime * posSmooth);
        ikTarget.rotation = Quaternion.Slerp(ikTarget.rotation, targetRot, Time.deltaTime * rotSmooth);
        

    }

    void Flip()
    {
        facing *= -1;
        // Yaw 180 회전(뒤로 회전)
        Vector3 e = playerRoot.eulerAngles;
        e.y = (facing == 1) ? 90f : 270f; 
        // ⚠️ 여기 90/270은 네 캐릭터 기본 방향에 맞춰야 함
        // 기본이 y=90일 때 +X를 보는 모델이라면 위처럼,
        // 기본이 y=0일 때 +Z 보는 모델이면 0/180으로 바꿔야 함.
        playerRoot.eulerAngles = e;
    }
}
