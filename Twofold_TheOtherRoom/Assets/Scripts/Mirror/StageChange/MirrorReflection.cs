using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 거울 평면 반사. 스테이지 전환 연출에 사용.
///
/// 원본 머티리얼 위에 반사 레이어를 슬롯으로 얹고 _Reflectivity 로 세기를 조절.
/// 반사 카메라는 targetTexture 를 가진 별도 카메라이고,
/// beginCameraRendering 에서 소스 카메라 기준 반사 행렬과 오블리크 근평면을 넣음.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class MirrorReflection : MonoBehaviour
{
    [Header("References")]
    [Tooltip("반사 레이어 머티리얼(M_MirrorReflect). 런타임에는 복사본 사용")]
    [SerializeField] private Material reflectMaterial;

    [Header("Reflection")]
    [Tooltip("반사 카메라가 그릴 레이어. 거울 자신은 레이어와 무관하게 자동으로 빠짐")]
    [SerializeField] private LayerMask reflectionCullingMask = ~0;

    [Tooltip("반사 텍스처 가로 크기. 눈 클로즈업까지 확대되므로 1024 이상")]
    [SerializeField] private int textureWidth = 2048;

    [Tooltip("거울 면의 법선 축(로컬). 부호는 무관하고 축만 맞으면 됨")]
    [SerializeField] private Vector3 planeNormalLocal = Vector3.up;

    [Tooltip("근평면을 거울 면에서 띄우는 양. 거울 경계에 실선이 보이면 키울 것")]
    [SerializeField] private float clipPlaneOffset = 0.01f;

    [Header("Debug")]
    [Tooltip("반사 카메라를 Hierarchy 에 노출. 반사가 회색 판으로만 나올 때 축 확인용")]
    [SerializeField] private bool showReflectionCamera = false;

    private static readonly int ReflectionTexID = Shader.PropertyToID("_ReflectionTex");
    private static readonly int ReflectivityID  = Shader.PropertyToID("_Reflectivity");

    private Renderer targetRenderer;

    /// Enable 전 머티리얼 배열. Teardown 에서 그대로 복원
    private Material[] priorMaterials;

    private Material runtimeMaterial;
    private RenderTexture reflectionTexture;
    private Camera reflectionCamera;
    private Camera sourceCamera;

    private bool running;
    private float reflectivity;

    /// <summary>0 = 원본 거울 그대로, 1 = 반사만.</summary>
    public float Reflectivity
    {
        get => reflectivity;
        set
        {
            reflectivity = Mathf.Clamp01(value);
            if (runtimeMaterial != null) runtimeMaterial.SetFloat(ReflectivityID, reflectivity);
        }
    }

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
    }

    private void OnDisable()
    {
        Enable(false);
    }

    // ---------- 외부 구동 (StageChange3D) ----------

    /// <summary>반사가 따라갈 카메라. Enable(true) 전에 넣을 것.</summary>
    public void SetSourceCamera(Camera cam)
    {
        sourceCamera = cam;
    }

    /// <summary>반사 켜기 · 끄기. 두 번 이상 불러도 안전.</summary>
    public void Enable(bool on)
    {
        if (on == running) return;

        if (on) Setup();
        else Teardown();
    }

    // ---------- 내부 ----------

    private void Setup()
    {
        if (reflectMaterial == null)
        {
            Debug.LogError("[MirrorReflection] reflectMaterial 인스펙터 참조 연결할 것", this);
            return;
        }

        if (sourceCamera == null)
        {
            Debug.LogError("[MirrorReflection] SetSourceCamera 를 Enable 전에 부를 것", this);
            return;
        }

        running = true;

        runtimeMaterial = new Material(reflectMaterial) { name = reflectMaterial.name + " (Runtime)" };
        runtimeMaterial.SetFloat(ReflectivityID, reflectivity);

        // 원본을 갈아끼우지 않고 슬롯을 하나 더 얹음.
        // 슬롯이 서브메시보다 많으면 마지막 서브메시가 그만큼 다시 그려짐.
        priorMaterials = targetRenderer.sharedMaterials;

        Material[] combined = new Material[priorMaterials.Length + 1];
        Array.Copy(priorMaterials, combined, priorMaterials.Length);
        combined[priorMaterials.Length] = runtimeMaterial;
        targetRenderer.sharedMaterials = combined;

        CreateTexture();
        CreateCamera();

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering   += OnEndCameraRendering;
    }

    private void Teardown()
    {
        running = false;

        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering   -= OnEndCameraRendering;
        GL.invertCulling = false;

        if (targetRenderer != null)
        {
            // begin 과 end 사이에 꺼졌으면 거울이 안 보이는 채로 남음
            targetRenderer.forceRenderingOff = false;

            if (priorMaterials != null) targetRenderer.sharedMaterials = priorMaterials;
        }

        priorMaterials = null;

        if (reflectionCamera != null)
        {
            reflectionCamera.targetTexture = null;
            Destroy(reflectionCamera.gameObject);
            reflectionCamera = null;
        }

        if (reflectionTexture != null)
        {
            reflectionTexture.Release();
            Destroy(reflectionTexture);
            reflectionTexture = null;
        }

        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
            runtimeMaterial = null;
        }
    }

    /// 소스와 같은 projection 을 쓰므로 화면 비율을 맞춰야 반사가 늘어나지 않음
    private void CreateTexture()
    {
        int height = Mathf.Max(1, Mathf.RoundToInt(textureWidth * Screen.height / (float)Screen.width));

        reflectionTexture = new RenderTexture(textureWidth, height, 24, RenderTextureFormat.DefaultHDR)
        {
            name = "MirrorReflection",
            useMipMap = false,
            antiAliasing = 1
        };

        reflectionTexture.Create();
    }

    private void CreateCamera()
    {
        GameObject go = new GameObject("MirrorReflectionCamera")
        {
            hideFlags = showReflectionCamera ? HideFlags.DontSave : HideFlags.HideAndDontSave
        };

        reflectionCamera = go.AddComponent<Camera>();
        reflectionCamera.CopyFrom(sourceCamera);
        reflectionCamera.targetTexture = reflectionTexture;
        reflectionCamera.cullingMask = reflectionCullingMask;
        reflectionCamera.depth = sourceCamera.depth - 1;   // 소스보다 먼저 그려야 그 프레임에 반영됨

        UniversalAdditionalCameraData data = go.GetComponent<UniversalAdditionalCameraData>();
        if (data == null) data = go.AddComponent<UniversalAdditionalCameraData>();

        data.renderType = CameraRenderType.Base;
        data.renderPostProcessing = false;

        runtimeMaterial.SetTexture(ReflectionTexID, reflectionTexture);
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam != reflectionCamera) return;

        UpdateReflectionMatrices();

        // 자기가 쓰는 RT 를 자기가 읽으면 깨지므로 이 패스에서만 거울을 뺌
        targetRenderer.forceRenderingOff = true;

        // 반사 행렬의 행렬식이 음수라 삼각형 와인딩이 뒤집힘
        GL.invertCulling = true;
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam != reflectionCamera) return;

        targetRenderer.forceRenderingOff = false;
        GL.invertCulling = false;
    }

    private void UpdateReflectionMatrices()
    {
        if (sourceCamera == null || reflectionCamera == null) return;

        Vector3 point  = transform.position;
        Vector3 normal = transform.TransformDirection(planeNormalLocal).normalized;

        // 거울 뒷면 기준으로 계산되지 않도록 법선을 소스 카메라 쪽으로 맞춤
        if (Vector3.Dot(normal, sourceCamera.transform.position - point) < 0f) normal = -normal;

        float distance = -Vector3.Dot(normal, point) - clipPlaneOffset;
        Vector4 plane = new Vector4(normal.x, normal.y, normal.z, distance);

        reflectionCamera.worldToCameraMatrix = sourceCamera.worldToCameraMatrix * ReflectionMatrix(plane);

        // 근평면을 거울 면에 붙여 거울 뒤 벽이 새는 것을 막음. FOV 변화도 여기서 따라감
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera.worldToCameraMatrix, point, normal);
        reflectionCamera.projectionMatrix = sourceCamera.CalculateObliqueMatrix(clipPlane);

        // 컬링과 그림자는 트랜스폼도 참조하므로 행렬과 같은 자리로 옮겨둠
        Matrix4x4 cameraToWorld = reflectionCamera.cameraToWorldMatrix;
        reflectionCamera.transform.SetPositionAndRotation(
            cameraToWorld.GetColumn(3),
            Quaternion.LookRotation(-(Vector3)cameraToWorld.GetColumn(2), cameraToWorld.GetColumn(1)));
    }

    /// 평면에 대한 거울상 변환
    private static Matrix4x4 ReflectionMatrix(Vector4 plane)
    {
        Matrix4x4 m = Matrix4x4.identity;

        m.m00 = 1f - 2f * plane.x * plane.x;
        m.m01 =    - 2f * plane.x * plane.y;
        m.m02 =    - 2f * plane.x * plane.z;
        m.m03 =    - 2f * plane.w * plane.x;

        m.m10 =    - 2f * plane.y * plane.x;
        m.m11 = 1f - 2f * plane.y * plane.y;
        m.m12 =    - 2f * plane.y * plane.z;
        m.m13 =    - 2f * plane.w * plane.y;

        m.m20 =    - 2f * plane.z * plane.x;
        m.m21 =    - 2f * plane.z * plane.y;
        m.m22 = 1f - 2f * plane.z * plane.z;
        m.m23 =    - 2f * plane.w * plane.z;

        return m;
    }

    /// 거울 면을 반사 카메라 공간으로 옮긴 평면. 오블리크 근평면용
    private Vector4 CameraSpacePlane(Matrix4x4 worldToCamera, Vector3 point, Vector3 normal)
    {
        Vector3 offsetPoint  = point + normal * clipPlaneOffset;
        Vector3 cameraPoint  = worldToCamera.MultiplyPoint(offsetPoint);
        Vector3 cameraNormal = worldToCamera.MultiplyVector(normal).normalized;

        return new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z,
                           -Vector3.Dot(cameraPoint, cameraNormal));
    }
}
