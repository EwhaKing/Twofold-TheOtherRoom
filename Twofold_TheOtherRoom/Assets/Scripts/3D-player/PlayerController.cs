using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-1)]
public class PlayerController : MonoBehaviour
{
    #region Class Variables
    // 지면 SphereCast 시작 구가 지면과 겹치지 않도록 띄우는 높이
    private const float ProbeStartGap = 0.05f;

    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;

    [Header("Base Movement")]
    [FormerlySerializedAs("runSpeed")] public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float movingThreshold = 0.01f;

    [Header("Gravity")]
    [Tooltip("낙하 가속도")]
    public float gravity = 25f;
    [Tooltip("낙하 속도 상한")]
    public float terminalVelocity = 40f;
    [Tooltip("접지 중 지면에 눌러 붙이는 하강 속도")]
    public float groundedStickVelocity = -2f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [Tooltip("발밑에서 지면을 찾아 내려가는 깊이")]
    [SerializeField] private float groundProbeDepth = 0.2f;

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 89f;

    [Header("Footstep")]
    [SerializeField] private float footstepInterval = 0.5f;

    private float footstepTimer = 0f;
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;

    private float _bodyYaw;       // 몸체 좌우 회전(yaw)
    private float _cameraPitch;   // 카메라 상하 회전(pitch)

    private bool _isGrounded;
    private Vector3 _groundNormal = Vector3.up;   // 접지면의 법선
    private float _verticalVelocity;

    // 지면 SphereCast 결과 버퍼 (자기 콜라이더 제외하기 위해 NonAlloc 사용)
    private readonly RaycastHit[] _groundHits = new RaycastHit[8];

    // 스프린트 상태 - 이동 입력이 있을 때만 가능
    private bool IsSprinting =>
        _playerLocomotionInput.SprintHeld && _playerLocomotionInput.MovementInput != Vector2.zero;
    #endregion

    #region Startup
    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
    }

    private void OnEnable()
    {
        // 조작 잠금이 풀린 직후 누적된 낙하 속도로 튀지 않게 초기화
        _verticalVelocity = 0f;
    }
    #endregion

    #region Update Logic

    private void Update()
    {
        UpdateGroundedState();
        UpdateMovementState();
        HandleMovement();
        HandleFootstep();
    }

    private void UpdateMovementState()
    {
        bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool isMoving = isMovingLaterally || isMovementInput;

        PlayerMovementState lateralState = isMoving
            ? (IsSprinting ? PlayerMovementState.Sprinting : PlayerMovementState.Running)
            : PlayerMovementState.Idling;
        _playerState.SetPlayerMovementState(lateralState);
    }

    // 발밑 SphereCast로 접지 여부와 지면 법선 갱신
    // CharacterController.isGrounded는 내리막/계단에서 프레임마다 진동해 쓰지 않음
    private void UpdateGroundedState()
    {
        float probeRadius = Mathf.Max(0.01f, _characterController.radius - _characterController.skinWidth);
        Vector3 feet = transform.position + _characterController.center
                       - Vector3.up * (_characterController.height * 0.5f);

        // 시작 시점에 지면과 겹치면 법선이 안 나오므로 구를 발밑보다 위에서 출발시킴
        Vector3 origin = feet + Vector3.up * (probeRadius + ProbeStartGap);
        float distance = ProbeStartGap + groundProbeDepth;

        int hitCount = Physics.SphereCastNonAlloc(origin, probeRadius, Vector3.down, _groundHits,
                                                  distance, groundLayers, QueryTriggerInteraction.Ignore);

        float nearestDistance = float.MaxValue;
        Vector3 nearestNormal = Vector3.up;
        bool foundGround = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _groundHits[i];

            // 처음부터 겹쳐 있던 히트 또는 자기 콜라이더 → 지면 후보에서 제외
            if (hit.distance <= 0f || hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.distance >= nearestDistance) continue;

            nearestDistance = hit.distance;
            nearestNormal = hit.normal;
            foundGround = true;
        }

        _groundNormal = foundGround ? nearestNormal : Vector3.up;

        // 너무 가파른 면은 미끄러지게 설정
        _isGrounded = foundGround
                      && Vector3.Angle(_groundNormal, Vector3.up) <= _characterController.slopeLimit;
    }

    public void HandleMovement()
    {
        // 카메라가 바라보는 방향(XZ 평면) 기준으로 이동 방향 계산
        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
        Vector3 movementDirection = cameraForwardXZ * _playerLocomotionInput.MovementInput.y + cameraRightXZ * _playerLocomotionInput.MovementInput.x;

        // 입력에서 속도를 직접 계산 (관성 없는 즉시 이동)
        Vector3 movementVelocity = movementDirection * (IsSprinting ? sprintSpeed : walkSpeed);

        // 내리막에서는 지면을 따라 내려가야 경사로 끝에서 붕 뜨지 않음
        // 오르막은 CharacterController가 알아서 밀어 올리므로 건드리지 않음
        if (_isGrounded)
        {
            Vector3 slopeVelocity = Vector3.ProjectOnPlane(movementVelocity, _groundNormal);
            if (slopeVelocity.y < 0f)
            {
                movementVelocity = slopeVelocity;
            }
        }

        UpdateVerticalVelocity();

        // Move는 수평·수직을 합쳐 한 번만 호출
        _characterController.Move((movementVelocity + Vector3.up * _verticalVelocity) * Time.deltaTime);
    }

    private void UpdateVerticalVelocity()
    {
        if (_isGrounded && _verticalVelocity <= 0f)
        {
            // 접지 중에는 살짝 눌러 경사로/계단에서 접지 판정이 끊기지 않게 함
            _verticalVelocity = groundedStickVelocity;
            return;
        }

        _verticalVelocity = Mathf.Max(_verticalVelocity - gravity * Time.deltaTime, -terminalVelocity);
    }

    private void HandleFootstep()
    {
        bool isMoving =
            _playerLocomotionInput.MovementInput != Vector2.zero
            && IsMovingLaterally();

        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.FootStep);
                }

                footstepTimer = footstepInterval;
            }
        }
        else
        {
            // 멈추면 다음 이동 시 바로 발소리가 나도록 초기화
            footstepTimer = 0f;
        }
    }
    #endregion

    #region LateUpdate Logic
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
    #endregion

    #region Camera Control
    private void UpdateCursorLock()
    {
        // 우클릭 중에는 커서 잠금/숨김, 평소에는 커서로 물체 클릭 가능
        bool look = _playerLocomotionInput.EnableCameraLook;
        Cursor.lockState = look ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !look;
    }
    #endregion

    #region State Check

    private bool IsMovingLaterally()
    {
        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
        
        return lateralVelocity.magnitude > movingThreshold;
    }
    #endregion
}
