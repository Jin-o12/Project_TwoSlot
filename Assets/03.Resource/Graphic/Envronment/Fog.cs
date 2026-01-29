using UnityEngine;

public class FogRevealController : MonoBehaviour
{
    [Header("Assign these in Inspector")]
    public Material fogMaterial;              // M_FogReveal
    public Transform flashlightOrigin;        // ������ ������(��/�ѱ�/����Ʈ����Ʈ ��ġ)
    public float correctFlashPosX;
    public Transform flashlightDirSource;     // ���� ����(���� ����Ʈ����Ʈ Transform)

    [Header("Optional (Recommended): Fog Quad Transform")]
    public Transform fogPlane;                // Fog_Quad1(�Ȱ� Quad). Z ��� ���߱��

    [Header("Flashlight Params (same meaning as your shader)")]
    public float range = 12f;                 // _FlashRange
    [Range(1f, 179f)] public float angleDeg = 50f;    // Unity Spot Angle(��ü ����)ó�� ����, ���ο��� �ݰ����� ��ȯ
    [Range(0f, 0.2f)] public float edgeSoft = 0.05f;  // _EdgeSoft
    [Range(0f, 5f)] public float endSoft = 1.0f;      // _EndSoft
    [Range(0f, 1f)] public float revealStrength = 0.4f; // _RevealStrength

    [Header("Side View Plane Lock")]
    public bool lockToFogPlaneZ = true;       // true�� ���� Z�� FogPlane Z�� ����(������ ����)
    public bool flattenDirToXY = true;        // 2.5D(�¿� X, ���Ʒ� Y)�� true ����. (Z ���� ����)

    [Header("Direction Axis")]
    public bool useForwardAxis = true;        // ����Ʈ����Ʈ�� forward�� ����Ű�� true
    public bool invertDir = false;            // ���� �ݴ�� üũ

    void LateUpdate()
    {
        // �ʼ� �Ҵ� üũ
        if (fogMaterial == null || flashlightOrigin == null || flashlightDirSource == null)
            return;

        // 1) ����
        Vector3 correctFlashPos = flashlightOrigin.position + new Vector3(correctFlashPosX, 0, 0);
        Vector3 origin = correctFlashPos;

        // �Ȱ� Quad�� ī�޶� �տ� �� ������, ��� ����� �޶����� ������ �յڷ� �и� �� ����
        // -> ���� Z�� FogQuad�� Z�� ���缭 ������ ��鿡�� ��ꡱ�ϰ� ����
        if (lockToFogPlaneZ && fogPlane != null)
            origin.z = fogPlane.position.z;

        // 2) ���� (����Ʈ����Ʈ�� �ٶ󺸴� �࿡ ���� ����)
        Vector3 dir = useForwardAxis ? flashlightDirSource.forward : flashlightDirSource.right;

        // ���̵��(���� XY ���)��� Z���� �����ؼ� ��� ��� ����ȭ
        if (flattenDirToXY)
            dir.z = 0f;

        if (invertDir)
            dir = -dir;

        dir.Normalize();

        // 3) ����: �ſ� �߿�!
        // ShaderGraph�� dot/cos ���� ������ ���ݰ�(half-angle)���� �������� ��.
        // Unity Spot Angle�� ����ü ����(full angle)���̹Ƿ� 0.5�� ���ؼ� cos�� ��ȯ�ؾ� ��ġ/���� �ڿ������� ����.
        float cosAngle = Mathf.Cos((angleDeg * 0.5f) * Mathf.Deg2Rad);

        // 4) ���̴� �Ķ���� ����
        fogMaterial.SetVector("_FlashOriginWS", origin);
        fogMaterial.SetVector("_FlashDirWS", dir);
        fogMaterial.SetFloat("_FlashRange", range);
        fogMaterial.SetFloat("_CosAngle", cosAngle);
        fogMaterial.SetFloat("_EdgeSoft", edgeSoft);
        fogMaterial.SetFloat("_EndSoft", endSoft);
        fogMaterial.SetFloat("_RevealStrength", revealStrength);

        // �����: �� �信�� �ʷ� ���� ������ ������ ���󰡸� ����
        Debug.DrawRay(origin, dir * range, Color.green);
    }
}
