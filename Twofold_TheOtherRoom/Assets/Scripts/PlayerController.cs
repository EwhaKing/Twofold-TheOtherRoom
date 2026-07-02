using UnityEngine;

[DefaultExecutionOrder(-1)]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;

    [Header("Base Movement")]
    public float runSpeed = 4f;

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 89f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private float _bodyYaw;       // 몸체 좌우 회전(yaw)
    private float _cameraPitch;   // 카메라 상하 회전(pitch)

    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
    }

    private void Update()
    {
        // 카메라가 바라보는 방향(XZ 평면) 기준으로 이동 방향 계산
        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
        Vector3 movementDirection = cameraForwardXZ * _playerLocomotionInput.MovementInput.y + cameraRightXZ * _playerLocomotionInput.MovementInput.x;

        // 입력에서 속도를 직접 계산 (관성 없는 즉시 이동)
        Vector3 movementVelocity = movementDirection * runSpeed;
        _characterController.Move(movementVelocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        UpdateCursorLock();

        // 우클릭을 누르고 있을 때만 카메라 회전
        if (!_playerLocomotionInput.EnableCameraLook)
        {
            return;
        }

        // 좌우(yaw): 몸체만 회전시키면 자식인 카메라도 함께 돌아감 (이중 회전 방지)
        _bodyYaw += lookSenseH * _playerLocomotionInput.LookInput.x;
        transform.rotation = Quaternion.Euler(0f, _bodyYaw, 0f);

        // 상하(pitch): 카메라만 로컬 회전
        _cameraPitch = Mathf.Clamp(_cameraPitch - lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);
        _playerCamera.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }

    private void UpdateCursorLock()
    {
        // 우클릭 중에는 커서 잠금/숨김, 평소에는 커서로 물체 클릭 가능
        bool look = _playerLocomotionInput.EnableCameraLook;
        Cursor.lockState = look ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !look;
    }
}
