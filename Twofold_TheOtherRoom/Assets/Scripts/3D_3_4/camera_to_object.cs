using UnityEngine;

/// <summary>
/// PlayerInteractor를 통해 물체를 카메라 앞에서 자세히 살펴보게 합니다.
/// 이 컴포넌트를 살펴볼 프리팹의 루트에 추가하세요.
/// </summary>
public class camera_to_object : MonoBehaviour, IInteractable, ICloseInspection, IResetInspection
{
  

    [Header("Inspection View")]
    [SerializeField] private Camera inspectionCamera;
    [SerializeField, Min(0.1f)] private float distanceFromCamera = 3f;
    public Vector3 screenOffset;

    [Header("Rotation")]
    [SerializeField, Min(1f)] private float rotationSpeed;
    [SerializeField] private float verticalRotationLimit = 180f;
    [Tooltip("켜면 Transform pivot 대신 자식 Renderer들의 시각적 중심을 기준으로 회전합니다. 모델이 pivot에서 떨어져 있을 때 사용하세요.")]
    [SerializeField] private bool rotateAroundVisualCenter = false;

    [Header("Overlay Rendering (책상/벽 뚫림 방지)")]
    [Tooltip("살펴보는 동안 물체와 자식들을 잠시 바꿔 둘 레이어 이름. 메인 카메라에서는 빼고 오버레이 카메라만 그리도록 설정된 레이어여야 합니다. 비워 두거나 없는 이름이면 레이어를 바꾸지 않습니다.")]
    [SerializeField] private string inspectionLayerName = "InspectionCamera";
    [Tooltip("Main Camera의 Stack에 등록한 Overlay 카메라. FOV를 메인 카메라와 맞추는 데 사용합니다. 비워 두면 FOV 동기화만 생략됩니다.")]
    [SerializeField] private Camera overlayCamera;

    [Header("Player Lock")]
    [Tooltip("비워 두면 PlayerController, PlayerLocomotionInput, PlayerInteractor를 자동으로 찾습니다.")]
    [SerializeField] private Behaviour[] behavioursToDisable;

    private readonly PlayerControlLock playerControlLock = new PlayerControlLock();
    private Collider[] objectColliders;
    private Rigidbody objectRigidbody;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool[] originalColliderStates;
    private bool originalIsKinematic;
    private bool originalUseGravity;
    private bool isInspecting;
    private int enteredFrame;
    private float inspectionYaw;
    private float inspectionPitch;
    private Quaternion objectRotationInCameraSpace;

    // inspectionPoint   : 살펴보기 중 물체가 놓일 화면상의 목표 지점 (월드 좌표)
    // visualCenterLocal : 모델의 실제 가운데 점을 이 오브젝트 기준(로컬) 좌표로 저장한 값

    private Vector3 inspectionPoint;
    private Vector3 visualCenterLocal;

    private float dragStartPitch;
    private float dragPitchDelta;

    // 레이어 교체용: 살펴보기 시작 시 자신+자식들의 Transform과 원래 레이어를 저장해 두고, 끝날 때 그대로 되돌립니다.
    private Transform[] layerTargets;
    private int[] originalLayers;

    private void Awake()
    {
        if (inspectionCamera == null)
            inspectionCamera = Camera.main;

        objectColliders = GetComponentsInChildren<Collider>(true);
        objectRigidbody = GetComponent<Rigidbody>();

    }

    private void Update()
    {
        if (!isInspecting)
            return;

        // Interact()가 호출된 같은 프레임의 E 입력으로 즉시 닫히는 것을 방지합니다.
        if (Time.frameCount > enteredFrame && Input.GetKeyDown(KeyCode.E))
        {
            EndInspection();
            return;
        }

        // 마우스를 처음 누른 순간의 회전값 저장
if (Input.GetMouseButtonDown(0))
{
    dragStartPitch = inspectionPitch;
    dragPitchDelta = 0f;
}

if (!Input.GetMouseButton(0))
    return;

float mouseX = Input.GetAxis("Mouse X") * -1f;
float mouseY = Input.GetAxis("Mouse Y");
float amount = rotationSpeed * Time.unscaledDeltaTime;

inspectionYaw += mouseX * amount;

    // 이번 드래그에서 움직인 양만 제한
    dragPitchDelta = Mathf.Clamp(
        dragPitchDelta + mouseY * amount,
        -verticalRotationLimit,
        verticalRotationLimit);

    inspectionPitch = dragStartPitch + dragPitchDelta;

    transform.rotation =
        inspectionCamera.transform.rotation
        * Quaternion.Euler(inspectionPitch, inspectionYaw, 0f)
        * objectRotationInCameraSpace;

    // 회전 후 위치 보정 (rotateAroundVisualCenter가 켜져 있을 때만 의미 있음)
    ApplyInspectionPosition();
    }




    /// 물체와 모든 자식의 레이어를 Inspection 레이어로 바꿉니다.
    /// 메인 카메라는 이 레이어를 그리지 않고 오버레이 카메라만 배경 위에 덧그리므로,
    /// 물체가 실제로 책상/벽 안에 들어가 있어도 화면에서는 잘리지 않습니다.

    private void SwitchToInspectionLayer()
    {
        layerTargets = null;
        if (string.IsNullOrEmpty(inspectionLayerName))
            return;

        int inspectionLayer = LayerMask.NameToLayer(inspectionLayerName);
        if (inspectionLayer < 0)
        {
            Debug.LogWarning($"[camera_to_object] '{inspectionLayerName}' 레이어가 없습니다. Tags and Layers에서 추가하세요.", this);
            return;
        }

        // 원래 레이어(Interactable 등)를 자식별로 기억해 둡니다.
        layerTargets = GetComponentsInChildren<Transform>(true);
        originalLayers = new int[layerTargets.Length];
        for (int i = 0; i < layerTargets.Length; i++)
        {
            originalLayers[i] = layerTargets[i].gameObject.layer;
            layerTargets[i].gameObject.layer = inspectionLayer;
        }
    }


    /// SwitchToInspectionLayer에서 저장해 둔 원래 레이어로 되돌립니다.

    private void RestoreOriginalLayers()
    {
        if (layerTargets == null)
            return;

        for (int i = 0; i < layerTargets.Length; i++)
        {
            if (layerTargets[i] != null)
                layerTargets[i].gameObject.layer = originalLayers[i];
        }

        layerTargets = null;
    }


    /// 살펴보기 중 물체의 위치를 맞춥니다.
    /// - 옵션 OFF: 기존처럼 pivot(transform.position)을 화면 목표 지점에 둡니다.
    /// - 옵션 ON : 모델의 시각적 중심이 화면 목표 지점에 오도록 pivot 위치를 역으로 계산합니다.

    private void ApplyInspectionPosition()
    {
        if (!rotateAroundVisualCenter)
        {
            transform.position = inspectionPoint;
            return;
        }

        // 현재 회전 상태에서 "pivot → 시각적 중심" 방향/거리
        Vector3 centerOffset = transform.TransformPoint(visualCenterLocal) - transform.position;

        // 시각적 중심 = inspectionPoint 가 되도록 pivot을 그만큼 반대로 이동
        transform.position = inspectionPoint - centerOffset;
    }

    //중심 계산 
    private Vector3 CalculateVisualCenterLocal()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return Vector3.zero;

        // Renderer.bounds는 월드 기준 박스이므로 모두 합친 뒤 중심만 로컬로 변환합니다.
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return transform.InverseTransformPoint(bounds.center);
    }

    public void Interact()
    {
        if (isInspecting)
            EndInspection();
        else
            BeginInspection();
    }

    private void BeginInspection()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
        }

        if (inspectionCamera == null)
        {
            inspectionCamera = Camera.main;
            if (inspectionCamera == null)
            {
                Debug.LogWarning("[camer_to_3D_4] 검사에 사용할 카메라가 없습니다.", this);
                return;
            }
        }

        SaveObjectState();
        // 오버레이 카메라만 그리도록 레이어 교체 (책상/벽 뚫림 방지)
        SwitchToInspectionLayer();
        //playercontrollock
        playerControlLock.Lock(this, behavioursToDisable);

        Transform cameraTransform = inspectionCamera.transform;
        //pivot 중심 계산 
        if (rotateAroundVisualCenter)
            visualCenterLocal = CalculateVisualCenterLocal();

        // 물체가 놓일 화면상의 목표 지점 (카메라 정면 + Screen Offset)
        inspectionPoint =
            cameraTransform.position
            + cameraTransform.right * screenOffset.x
            + cameraTransform.up * screenOffset.y
            + cameraTransform.forward * (distanceFromCamera + screenOffset.z);
        ApplyInspectionPosition();

        inspectionYaw = 0f;
        inspectionPitch = 0f;
        objectRotationInCameraSpace =
            Quaternion.Inverse(cameraTransform.rotation) * transform.rotation;

        if (InspectionUIController.Instance != null)
            InspectionUIController.Instance.Show(this);

        isInspecting = true;
        enteredFrame = Time.frameCount;
    }

    private void EndInspection()
    {
        if (!isInspecting)
            return;

        transform.SetPositionAndRotation(originalPosition, originalRotation);
        RestoreObjectState();
        // 원래 레이어(Interactable 등)로 복구
        RestoreOriginalLayers();
        //playerControlunLock
        playerControlLock.Unlock();
        if (InspectionUIController.Instance != null)
            InspectionUIController.Instance.Hide(this);


        isInspecting = false;
    }

    public void ResetInspectionView()
{
    if (!isInspecting || inspectionCamera == null)
        return;

    // E를 누르기 전의 원래 회전으로 복원
    transform.rotation = originalRotation;
    // 회전을 되돌렸으니 시각적 중심이 다시 화면 가운데에 오도록 위치도 맞춥니다.
    ApplyInspectionPosition();

    // 마우스 회전 누적값 초기화
    inspectionYaw = 0f;
    inspectionPitch = 0f;

    // 현재 카메라 기준으로 초기 회전 관계 재설정
    objectRotationInCameraSpace =
        Quaternion.Inverse(inspectionCamera.transform.rotation)
        * originalRotation;

    if (objectRigidbody != null)
    {
        objectRigidbody.linearVelocity = Vector3.zero;
        objectRigidbody.angularVelocity = Vector3.zero;
    }
}

    /// UI Button의 OnClick 이벤트에 연결할 검사 종료 메서드입니다.
    public void CloseInspection()
    {
        EndInspection();
    }

    public void ResetInspection()
    {
        ResetInspectionView();
    }

    private void SaveObjectState()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalColliderStates = new bool[objectColliders.Length];
        for (int i = 0; i < objectColliders.Length; i++)
        {
            originalColliderStates[i] = objectColliders[i].enabled;
            objectColliders[i].enabled = false;
        }

        if (objectRigidbody != null)
        {
            originalIsKinematic = objectRigidbody.isKinematic;
            originalUseGravity = objectRigidbody.useGravity;
            objectRigidbody.linearVelocity = Vector3.zero;
            objectRigidbody.angularVelocity = Vector3.zero;
            objectRigidbody.isKinematic = true;
            objectRigidbody.useGravity = false;
        }
    }

    private void RestoreObjectState()
    {
        for (int i = 0; i < objectColliders.Length; i++)
            objectColliders[i].enabled = originalColliderStates[i];

        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = originalIsKinematic;
            objectRigidbody.useGravity = originalUseGravity;
        }
    }

}
