using System.Collections.Generic;
using UnityEngine;

public class InspectionCameraRig : MonoBehaviour
{
    public static InspectionCameraRig Instance { get; private set; }

    [Header("Camera Positions")]
    [SerializeField] private Camera[] cameraPoints;

    [Header("View Camera")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private GameObject cameraUI;

    [Header("Player Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Player Lock")]
    [Tooltip("비워 두면 PlayerControlLock이 플레이어 이동과 상호작용을 자동으로 잠금.")]
    [SerializeField] private Behaviour[] behavioursToDisable;

    [Header("Player Model")]
    [Tooltip("보기 중 숨길 캐릭터 모델. 비우면 Player 태그 오브젝트")]
    [SerializeField] private GameObject playerModel;

    [Header("Camera Transition")]
    [SerializeField] private float transitionDuration = 0.5f;

    public int currentCameraIndex = 0;

    // 보기 진입 성공 여부. 공통 캔버스 표시 조건
    public bool IsViewing => isViewing;

    // 보기 중 화면을 그리는 카메라. 클릭 레이 원점
    public Camera ViewCamera => viewCamera;

    private readonly PlayerControlLock playerControlLock = new PlayerControlLock();
    private readonly Dictionary<Renderer, bool> hiddenRenderers = new Dictionary<Renderer, bool>();

    private bool isViewing = false;
    private bool isMoving = false;

    private float transitionTimer;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        SetAllObjectCameras(false);
        cameraUI.SetActive(false);

        if (viewCamera != null) viewCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isViewing) return;

        UpdateCameraTransition();
    }

    public void StartView()
    {
        if (cameraPoints == null || cameraPoints.Length == 0)
        {
            Debug.LogWarning($"[{nameof(InspectionCameraRig)}] 등록된 카메라가 없습니다.");
            return;
        }
        if (viewCamera == null)
        {
            Debug.LogWarning($"[{nameof(InspectionCameraRig)}] View Camera가 연결되지 않았습니다.");
            return;
        }

        isViewing = true;
        isMoving = false;
        currentCameraIndex = 0;

        playerControlLock.Lock(this, behavioursToDisable, alwaysDisablePlayerInteractor: true);
        HidePlayerModel();

        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        viewCamera.gameObject.SetActive(true);
        cameraUI.SetActive(true);

        Transform startPoint = cameraPoints[currentCameraIndex].transform;
        viewCamera.transform.SetPositionAndRotation(startPoint.position, startPoint.rotation);

        Debug.Log($"[{nameof(InspectionCameraRig)}] 카메라 시작: {currentCameraIndex}");
    }

    public void ExitView()
    {
        if (!isViewing) return;

        isViewing = false;
        isMoving = false;
        currentCameraIndex = 0;

        if (viewCamera != null) viewCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);
        cameraUI.SetActive(false);

        playerControlLock.Unlock();
        RestorePlayerModel();

        Debug.Log($"[{nameof(InspectionCameraRig)}] 카메라 보기 종료");
    }

    /// <summary>지정 카메라로 전환. Btn_top은 0, Btn_side는 1</summary>
    public void ShowCamera(int index)
    {
        if (!isViewing || isMoving) return;
        if (index == currentCameraIndex) return;

        if (cameraPoints == null || index < 0 || index >= cameraPoints.Length)
        {
            Debug.LogWarning($"[{nameof(InspectionCameraRig)}] 카메라 인덱스 범위 밖: {index}");
            return;
        }

        currentCameraIndex = index;
        StartCameraTransition();
    }

    private void StartCameraTransition()
    {
        if (viewCamera == null) return;

        Transform target = cameraPoints[currentCameraIndex].transform;

        startPosition = viewCamera.transform.position;
        startRotation = viewCamera.transform.rotation;
        targetPosition = target.position;
        targetRotation = target.rotation;

        transitionTimer = 0f;
        isMoving = true;

        Debug.Log($"[{nameof(InspectionCameraRig)}] 카메라 이동 → {currentCameraIndex}");
    }

    private void UpdateCameraTransition()
    {
        if (!isMoving) return;

        transitionTimer += Time.deltaTime;

        // 부드러운 시작 + 부드러운 끝
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(transitionTimer / transitionDuration));

        viewCamera.transform.SetPositionAndRotation(
            Vector3.Lerp(startPosition, targetPosition, t),
            Quaternion.Slerp(startRotation, targetRotation, t));

        if (t < 1f) return;

        viewCamera.transform.SetPositionAndRotation(targetPosition, targetRotation);
        isMoving = false;
    }

    private void SetAllObjectCameras(bool active)
    {
        foreach (Camera cam in cameraPoints)
        {
            if (cam != null) cam.gameObject.SetActive(active);
        }
    }

    private void HidePlayerModel()
    {
        RestorePlayerModel();

        GameObject target = playerModel != null
            ? playerModel
            : GameObject.FindGameObjectWithTag("Player");

        if (target == null)
        {
            Debug.LogWarning($"[{nameof(InspectionCameraRig)}] 숨길 플레이어 모델 없음");
            return;
        }

        foreach (Renderer targetRenderer in target.GetComponentsInChildren<Renderer>(true))
        {
            hiddenRenderers.Add(targetRenderer, targetRenderer.enabled);
            targetRenderer.enabled = false;
        }
    }

    private void RestorePlayerModel()
    {
        foreach (KeyValuePair<Renderer, bool> state in hiddenRenderers)
        {
            if (state.Key != null) state.Key.enabled = state.Value;
        }

        hiddenRenderers.Clear();
    }
}
