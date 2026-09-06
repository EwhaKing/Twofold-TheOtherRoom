using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float locomotionBlendSpeed = 3f;
    [Tooltip("스프린트 시 블렌드 입력에 곱하는 배율. 블렌드 트리의 Sprint 반경과 맞추기")]
    [SerializeField] private float sprintBlendMultiplier = 2f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;

    private static int inputXHash = Animator.StringToHash("InputX");
    private static int inputYHash = Animator.StringToHash("InputY");

    private Vector3 _currentBlendInput = Vector3.zero;

    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        Vector2 inputTarget = _playerLocomotionInput.MovementInput;

        // 스프린트
        if (_playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting)
        {
            inputTarget *= sprintBlendMultiplier;
        }

        _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
    }
}
