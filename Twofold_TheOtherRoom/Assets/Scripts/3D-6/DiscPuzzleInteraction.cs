using UnityEngine;
public class DiscPuzzleInteraction : MonoBehaviour, IInteractable, ICloseInspection
{
    [Header("Cameras")]
    [Tooltip("탐험 중 사용하는 플레이어 카메라. 비우면 Camera.main")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("CinemachineBrain이 붙은 퍼즐 렌더링 카메라")]
    [SerializeField] private Camera puzzleCamera;

    [Header("View")]
    [Tooltip("side/front vcam 우선순위를 바꾸는 ViewSwitcher")]
    [SerializeField] private ViewSwitcher viewSwitcher;

    [Header("UI")]
    [Tooltip("진입 중에만 표시할 side/front 전환 버튼 canvas")]
    [SerializeField] private GameObject switchCanvas;

    [Header("Player Lock")]
    [Tooltip("비워 두면 PlayerController, PlayerLocomotionInput, PlayerInteractor를 자동으로 찾습니다.")]
    [SerializeField] private Behaviour[] behavioursToDisable;

    private readonly PlayerControlLock playerControlLock = new PlayerControlLock();
    private bool isEntered;
    private int enteredFrame;

    // E용 큰 콜라이더
    private Collider interactionCollider;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        interactionCollider = GetComponent<Collider>();

        // 시작 시엔 퍼즐 카메라/캔버스를 꺼서 side 뷰로 바로 넘어가는 것 방지.
        if (puzzleCamera != null)
            puzzleCamera.enabled = false;
        if (switchCanvas != null)
            switchCanvas.SetActive(false);
    }

    private void Update()
    {
        if (!isEntered)
            return;

        // Interact()가 호출된 같은 프레임의 E 입력으로 즉시 닫히는 것을 방지.
        if (Time.frameCount > enteredFrame &&
            (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
        {
            Exit();
        }
    }

    public void Interact()
    {
        if (isEntered)
            Exit();
        else
            Enter();
    }

    private void Enter()
    {
           //playercontrollock
        playerControlLock.Lock(this, behavioursToDisable);

        // 카메라 전환: 플레이어 카메라 끄고 퍼즐 카메라 켜기
        if (playerCamera != null)
            playerCamera.enabled = false;
        if (puzzleCamera != null)
            puzzleCamera.enabled = true;

        // 원판 클릭 레이캐스트를 가리지 않도록 큰 콜라이더 끄기
        if (interactionCollider != null)
            interactionCollider.enabled = false;

        // 진입은 항상 side 뷰에서 시작
        if (viewSwitcher != null)
            viewSwitcher.SwitchToSide();

        if (switchCanvas != null)
            switchCanvas.SetActive(true);

        if (InspectionUIController.Instance != null)
            InspectionUIController.Instance.Show(this);

        isEntered = true;
        enteredFrame = Time.frameCount;
    }

    private void Exit()
    {
        if (!isEntered)
            return;

        if (switchCanvas != null)
            switchCanvas.SetActive(false);

        if (InspectionUIController.Instance != null)
            InspectionUIController.Instance.Hide(this);

        if (puzzleCamera != null)
            puzzleCamera.enabled = false;
        if (playerCamera != null)
            playerCamera.enabled = true;

        // 다시 상호작용 가능하도록 콜라이더 복구
        if (interactionCollider != null)
            interactionCollider.enabled = true;

         //playercontrolunlock
        playerControlLock.Unlock();

        isEntered = false;
    }

    /// UI 버튼 등에서 호출할 수 있는 종료 메서드
    public void CloseInteraction()
    {
        Exit();
    }

    public void CloseInspection()
    {
        Exit();
    }

}
