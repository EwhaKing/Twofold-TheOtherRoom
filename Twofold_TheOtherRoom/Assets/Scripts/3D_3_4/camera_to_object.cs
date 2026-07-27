using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// PlayerInteractor를 통해 물체를 카메라 앞에서 자세히 살펴보게 합니다.
/// 이 컴포넌트를 살펴볼 프리팹의 루트에 추가하세요.
/// </summary>
public class camera_to_object : MonoBehaviour, IInteractable
{
  

    [Header("Inspection View")]
    [SerializeField] private Camera inspectionCamera;
    [SerializeField, Min(0.1f)] private float distanceFromCamera = 3f;
    public Vector3 screenOffset;

    [Header("Inspection UI")]
    [Tooltip("검사 중에만 표시할 뒤로가기 버튼 오브젝트입니다.")]
    [SerializeField] private GameObject backButton;


    [Header("Rotation")]
    [SerializeField, Min(1f)] private float rotationSpeed = 50f;
    [SerializeField, Range(1f, 89f)] private float verticalRotationLimit = 80f;

    [Header("Player Lock")]
    [Tooltip("비워 두면 PlayerController, PlayerLocomotionInput, PlayerInteractor를 자동으로 찾습니다.")]
    [SerializeField] private Behaviour[] behavioursToDisable;

    private readonly List<Behaviour> disabledBehaviours = new List<Behaviour>();
    private Collider[] objectColliders;
    private Rigidbody objectRigidbody;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool[] originalColliderStates;
    private bool originalIsKinematic;
    private bool originalUseGravity;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;
    private bool isInspecting;
    private int enteredFrame;
    private float inspectionYaw;
    private float inspectionPitch;
    private Quaternion objectRotationInCameraSpace;

    private void Awake()
    {
        if (inspectionCamera == null)
            inspectionCamera = Camera.main;

        objectColliders = GetComponentsInChildren<Collider>(true);
        objectRigidbody = GetComponent<Rigidbody>();

        if (backButton != null)
            backButton.SetActive(false);
    }

    private void Update()
    {
        if (!isInspecting)
            return;

        // Interact()가 호출된 같은 프레임의 E 입력으로 즉시 닫히는 것을 방지합니다.
        if (Time.frameCount > enteredFrame &&
            (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
        {
            EndInspection();
            return;
        }

        if (!Input.GetMouseButton(0))
            return;

        float mouseX = Input.GetAxis("Mouse X") * -1f;
        float mouseY = Input.GetAxis("Mouse Y");
        float amount = rotationSpeed * Time.unscaledDeltaTime;

        inspectionYaw += mouseX * amount;
        inspectionPitch = Mathf.Clamp(
            inspectionPitch + mouseY * amount,
            -verticalRotationLimit,
            verticalRotationLimit);

        // 월드 회전을 계속 누적하지 않고 카메라 기준 yaw/pitch로 다시 계산합니다.
        // 좌우와 상하 회전이 섞일 때 생기는 불필요한 Z축 기울기를 방지합니다.
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
        DisablePlayerControl();

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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (backButton != null)
            backButton.SetActive(true);

        isInspecting = true;
        enteredFrame = Time.frameCount;
    }

    private void EndInspection()
    {
        if (!isInspecting)
            return;

        transform.SetPositionAndRotation(originalPosition, originalRotation);
        RestoreObjectState();
        RestorePlayerControl();

        Cursor.lockState = originalCursorLockMode;
        Cursor.visible = originalCursorVisible;
        if (backButton != null)
            backButton.SetActive(false);

        isInspecting = false;
    }

    /// UI Button의 OnClick 이벤트에 연결할 검사 종료 메서드입니다.
    public void CloseInspection()
    {
        EndInspection();
    }

    private void SaveObjectState()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalCursorLockMode = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

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

    private void DisablePlayerControl()
    {
        disabledBehaviours.Clear();

        if (behavioursToDisable == null || behavioursToDisable.Length == 0)
        {
            TryDisable(FindAnyObjectByType<PlayerController>());
            TryDisable(FindAnyObjectByType<PlayerLocomotionInput>());
            TryDisable(FindAnyObjectByType<PlayerInteractor>());
            return;
        }

        foreach (Behaviour behaviour in behavioursToDisable)
            TryDisable(behaviour);
    }

    private void TryDisable(Behaviour behaviour)
    {
        if (behaviour == null || behaviour == this || !behaviour.enabled)
            return;

        behaviour.enabled = false;
        disabledBehaviours.Add(behaviour);
    }

    private void RestorePlayerControl()
    {
        foreach (Behaviour behaviour in disabledBehaviours)
        {
            if (behaviour != null)
                behaviour.enabled = true;
        }

        disabledBehaviours.Clear();
    }

}
