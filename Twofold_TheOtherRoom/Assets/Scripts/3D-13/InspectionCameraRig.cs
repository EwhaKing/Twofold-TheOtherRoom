using UnityEngine;

public class InspectionCameraRig : MonoBehaviour
{
    public static InspectionCameraRig Instance { get; private set; }

    [Header("Camera Positions")]
    [SerializeField] private Camera[] objectCameras;

    [Header("View Camera")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private GameObject cameraUI;

    [Header("Player Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Player Lock")]
    [Tooltip("비워 두면 PlayerControlLock이 플레이어 이동과 상호작용을 자동으로 잠금.")]
    [SerializeField] private Behaviour[] behavioursToDisable;

    [Header("Camera Transition")]
    [SerializeField] private float transitionDuration = 0.5f;

    private readonly PlayerControlLock playerControlLock = new PlayerControlLock();

    public int currentCameraIndex = 0;

    // 보기 진입 성공 여부. 공통 캔버스 표시 조건
    public bool IsViewing => isViewing;

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
        if (playerCamera == null)
            playerCamera = Camera.main;

        SetAllObjectCameras(false);
        cameraUI.SetActive(false);

        if (viewCamera != null)
            viewCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isViewing)
            return;
            
        UpdateCameraTransition();
    }

    public void StartView()
    {
        if (objectCameras == null || objectCameras.Length == 0)
        {
            Debug.LogWarning(
                "[ObjectCameraManager] 등록된 카메라가 없습니다."
            );

            return;
        }

        if (viewCamera == null)
        {
            Debug.LogWarning(
                "[ObjectCameraManager] View Camera가 연결되지 않았습니다."
            );

            return;
        }


        isViewing = true;
        isMoving = false;

        currentCameraIndex = 0;


        playerControlLock.Lock(
            this,
            behavioursToDisable,
            alwaysDisablePlayerInteractor: true);


        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);


        viewCamera.gameObject.SetActive(true);
        cameraUI.SetActive(true);

        viewCamera.transform.position =
            objectCameras[currentCameraIndex].transform.position;

        viewCamera.transform.rotation =
            objectCameras[currentCameraIndex].transform.rotation;


        Debug.Log(
            $"[ObjectCameraManager] 카메라 시작: {currentCameraIndex}"
        );
    }


    public void NextCamera()
    {
        if (!isViewing || isMoving)
            return;

        currentCameraIndex++;

        if (currentCameraIndex >= objectCameras.Length)
            currentCameraIndex = 0;

        StartCameraTransition();
    }

    public void PreviousCamera()
    {
        if (!isViewing || isMoving)
            return;
    
        currentCameraIndex--;
    
        if (currentCameraIndex < 0)
            currentCameraIndex = objectCameras.Length - 1;
    
        StartCameraTransition();
    }


    private void StartCameraTransition()
    {
        if (viewCamera == null)
            return;


        startPosition =
            viewCamera.transform.position;

        startRotation =
            viewCamera.transform.rotation;


        targetPosition =
            objectCameras[currentCameraIndex].transform.position;

        targetRotation =
            objectCameras[currentCameraIndex].transform.rotation;


        transitionTimer = 0f;

        isMoving = true;


        Debug.Log(
            $"[ObjectCameraManager] 카메라 이동 → {currentCameraIndex}"
        );
    }


    private void UpdateCameraTransition()
    {
        if (!isMoving)
            return;
    
        transitionTimer += Time.deltaTime;
    
        float t = transitionTimer / transitionDuration;
    
        t = Mathf.Clamp01(t);
    
        // 부드러운 시작 + 부드러운 끝
        t = Mathf.SmoothStep(0f, 1f, t);
    
        viewCamera.transform.position =
            Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );
    
        viewCamera.transform.rotation =
            Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );
    
        if (t >= 1f)
        {
            viewCamera.transform.position =
                targetPosition;
    
            viewCamera.transform.rotation =
                targetRotation;
    
            isMoving = false;
        }
    }

    public void ExitView()
    {
        if (!isViewing)
            return;


        isViewing = false;
        isMoving = false;


        if (viewCamera != null)
            viewCamera.gameObject.SetActive(false);


        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);


        playerControlLock.Unlock();

        cameraUI.SetActive(false);



        currentCameraIndex = 0;


        Debug.Log("[ObjectCameraManager] 카메라 보기 종료");
    }

    private void SetAllObjectCameras(bool active)
    {
        foreach (Camera cam in objectCameras)
        {
            if (cam != null)
                cam.gameObject.SetActive(active);
        }
    }
}