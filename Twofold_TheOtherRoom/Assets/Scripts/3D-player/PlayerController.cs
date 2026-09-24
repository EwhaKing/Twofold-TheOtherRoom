using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-1)]
public class PlayerController : MonoBehaviour
{
    #region Class Variables
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;
    [Tooltip("화면 중앙 조준점 UI")]
    [SerializeField] private GameObject reticle;

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
    [Tooltip("발이 지면에서 이만큼 떠 있어도 접지로 인정")]
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

        // 카메라를 현재 방향 기준으로 다시 잡음
        _bodyYaw = transform.eulerAngles.y;
        _cameraPitch = NormalizeAngle(_playerCamera.transform.localEulerAngles.x);
    }

    private void OnDisable()
    {
        if (reticle != null)
        {
            reticle.SetActive(false);
        }
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
        // radius/height/center/skinWidth는 스케일이 적용되지 않은 로컬 값
        Vector3 lossyScale = transform.lossyScale;
        float radiusScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.z));
        float worldRadius = _characterController.radius * radiusScale;
        float worldSkin = _characterController.skinWidth * radiusScale;
        float worldHeight = _characterController.height * Mathf.Abs(lossyScale.y);

        Vector3 feet = transform.TransformPoint(_characterController.center)
                       - Vector3.up * (worldHeight * 0.5f);

        // 시작 시점에 지면과 겹치면 법선이 안 나오므로 스킨 폭만큼 띄워서 출발
        // 캡슐은 스킨 폭만큼 뜬 채로 멈추므로 지면까지 그만큼 더 내려가야 함
        float probeRadius = Mathf.Max(0.01f, worldRadius - worldSkin);
        Vector3 origin = feet + Vector3.up * (probeRadius + worldSkin);
        float distance = worldSkin * 2f + groundProbeDepth;

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
        UpdateReticle();

        // 마우스 delta는 timeScale의 영향을 받지 않음
        if (Time.timeScale <= 0f)
        {
            return;
        }

        Vector2 look = _playerLocomotionInput.LookInput;

        // 좌우(yaw): 몸체만 회전시키면 자식인 카메라도 함께 돌아감 (이중 회전 방지)
        _bodyYaw += lookSenseH * look.x;
        transform.rotation = Quaternion.Euler(0f, _bodyYaw, 0f);

        // 상하(pitch): 카메라만 로컬 회전
        _cameraPitch = Mathf.Clamp(_cameraPitch - lookSenseV * look.y, -lookLimitV, lookLimitV);
        _playerCamera.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }
    #endregion

    #region Camera Control
    // 상호작용 안내 -> 조준점 숨김
    private void UpdateReticle()
    {
        if (reticle == null)
        {
            return;
        }

        PlayerInteractor interactor = PlayerInteractor.Instance;
        bool prompt = interactor != null
                      && ((interactor.interactText != null && interactor.interactText.gameObject.activeSelf)
                          || (interactor.MouseHoldUI != null && interactor.MouseHoldUI.activeSelf));

        reticle.SetActive(!prompt && Time.timeScale > 0f);
    }

    // localEulerAngles는 0~360으로 돌아와 pitch 클램프가 어긋남
    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        return angle > 180f ? angle - 360f : angle;
    }

    private void UpdateCursorLock()
    {
        // 일시정지 중엔 메뉴를 클릭해야 함. Confined라 창 밖으로는 못 나감
        bool look = Time.timeScale > 0f;
        CursorLockMode lockMode = look ? CursorLockMode.Locked : CursorLockMode.Confined;

        // 매 프레임 대입하면 네이티브 호출이 반복돼 커서가 튐
        if (Cursor.lockState != lockMode)
        {
            Cursor.lockState = lockMode;
        }

        if (Cursor.visible == look)
        {
            Cursor.visible = !look;
        }
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
