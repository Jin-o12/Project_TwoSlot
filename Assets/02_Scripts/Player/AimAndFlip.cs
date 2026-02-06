using System.Collections;
using UnityEngine;

public class AimAndFlip : MonoBehaviour
{
    public Vector3 AimWorldPoint { get; private set; }

    [Header("Refs")]
    public Camera cam;
    public Transform playerRoot;   // 캐릭터 전체를 뒤집을 루트
    public Transform aimPivot;     // 어깨/가슴 기준점
    public Transform ikTarget;     // RightHand_IK_Target

    [Header("Aim Plane")]
    public float aimPlaneZ = 0f;       // 2.5D면 고정 Z
    public float targetDistance = 1.2f;

    [Header("Clamp & Flip")]
    public float maxArmAngle = 140f;   // 120~140 추천 (180은 팔이 너무 뒤로 감)
    public float flipHysteresis = 5f;  // 경계 떨림 방지
    public float flipLockSeconds = 0.12f; // 플립 연타 방지

    [Header("Smoothing")]
    public float posSmooth = 25f;
    public float rotSmooth = 25f;

    [Header("Body Turn")]
    public float bodyTurnSpeed = 15f;  // 몸 회전 부드러움

    [Header("IK Rotation (SpotLight가 Z forward라면 그대로 두면 됨)")]
    public Vector3 ikEulerOffset = Vector3.zero; // 필요하면 (0,90,0) 이런식으로 보정

    // 상태
    int facing = 1;                 // +1 오른쪽, -1 왼쪽
    public int Facing => facing;
    public bool IsDead { get; set; }

    private Quaternion targetBodyRot;
    private bool flipLocked;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!playerRoot) playerRoot = transform;

        aimPlaneZ = transform.position.z;
        targetBodyRot = playerRoot.rotation;
    }

    void LateUpdate()
    {
        if (GamePauseManager.Paused) return;
        if (IsDead) return;
        if (!cam || !playerRoot || !aimPivot || !ikTarget) return;

        // 1) 마우스를 월드로 (Z 평면 투영)
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, aimPlaneZ));
        if (!plane.Raycast(ray, out float enter)) return;

        Vector3 mouseWorld = ray.GetPoint(enter);
        AimWorldPoint = mouseWorld;

        // 2) pivot -> mouse 방향(2D)
        Vector3 dir = mouseWorld - aimPivot.position;
        dir.z = 0f;
        if (dir.sqrMagnitude < 0.000001f) return;
        dir.Normalize();

        // 3) 현재 바라보는 방향 기준축
        Vector3 baseAxis = (facing == 1) ? Vector3.right : Vector3.left;

        // baseAxis -> dir (Z축 기준 signed angle)
        float signed = Vector3.SignedAngle(baseAxis, dir, Vector3.forward);

        float half = maxArmAngle * 0.5f;

        // 4) 허용 각도 밖이면 플립 (연타 방지)
        if (!flipLocked)
        {
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
        }

        // 5) 팔 각도 clamp
        float clamped = Mathf.Clamp(signed, -half, half);

        // 6) IK Target 위치/회전
        Vector3 aimDir = Quaternion.AngleAxis(clamped, Vector3.forward) * baseAxis;

        Vector3 targetPos = aimPivot.position + aimDir * targetDistance;
        targetPos.z = aimPlaneZ;

        // ✅ 손전등(SpotLight)이 Z forward라면, 이 회전이 가장 자연스럽다.
        //    forward = aimDir, up = Vector3.up
        Quaternion targetRot = Quaternion.LookRotation(aimDir, Vector3.up) * Quaternion.Euler(ikEulerOffset);

        ikTarget.position = Vector3.Lerp(ikTarget.position, targetPos, Time.deltaTime * posSmooth);
        ikTarget.rotation = Quaternion.Slerp(ikTarget.rotation, targetRot, Time.deltaTime * rotSmooth);

        // 7) 몸 회전은 부드럽게 목표로
        playerRoot.rotation = Quaternion.Slerp(playerRoot.rotation, targetBodyRot, Time.deltaTime * bodyTurnSpeed);
    }

    void Flip()
    {
        facing *= -1;

        float targetY = (facing == 1) ? 90f : 270f; // 네 캐릭터 기본방향 기준 유지
        targetBodyRot = Quaternion.Euler(0f, targetY, 0f);

        if (flipLockSeconds > 0f)
            StartCoroutine(FlipLock());
    }

    IEnumerator FlipLock()
    {
        flipLocked = true;
        yield return new WaitForSeconds(flipLockSeconds);
        flipLocked = false;
    }
}
