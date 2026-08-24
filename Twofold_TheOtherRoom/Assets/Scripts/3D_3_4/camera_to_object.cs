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


    private float dragStartPitch;
    private float dragPitchDelta;

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
        SoundManager.Instance.PlaySFX(SFXType.DefaultClick);

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
        //playercontrollock
        playerControlLock.Lock(this, behavioursToDisable);

        Transform cameraTransform = inspectionCamera.transform;
        // 카메라는 변경하지 않고 물체만 현재 카메라 정면으로 이동합니다.
        transform.position =
            cameraTransform.position
            + cameraTransform.right * screenOffset.x
            + cameraTransform.up * screenOffset.y
            + cameraTransform.forward * (distanceFromCamera + screenOffset.z);

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
