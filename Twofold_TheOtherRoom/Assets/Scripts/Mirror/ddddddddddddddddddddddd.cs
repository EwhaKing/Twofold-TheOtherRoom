using UnityEngine;

public class Mddddddddd : MonoBehaviour, IInteractable
{
    public enum MirrorType
    {
        InMirror,
        OutMirror
    }

    [Header("Mirror Settings")]
    [SerializeField] private MirrorType mirrorType;
    [SerializeField] private string mirrorId;

    [Tooltip("OutMirror일 때만 연결합니다.")]
    [SerializeField] private GameObject inMirror;

    [Header("Holding Settings")]
    [SerializeField] private float holdDistance = 2f;

    // 외부에서 확인할 수 있는 정보
    public MirrorType Type => mirrorType;
    public string MirrorId => mirrorId;
    public GameObject InMirror => inMirror;

    // 현재 거울을 획득했는지
    public bool IsObtain { get; private set; }

    // 현재 거울을 들고 있는지
    public bool IsHolding { get; private set; }


    private void Awake()
    {
        IsObtain = false;
        IsHolding = false;
    }


    public void Interact()
    {
        switch (mirrorType)
        {
            case MirrorType.OutMirror:
                ObtainMirror();
                break;

            case MirrorType.InMirror:
                StartHolding();
                break;
        }
    }

    private void ObtainMirror()
    {
        if (MirrorManager.Instance == null)
        {
            Debug.LogWarning("[Mirror3D] MirrorManager가 없습니다.");
            return;
        }

        if (IsObtain)
        {
            return;
        }

        MirrorManager.Instance.GetMirrorPiece(mirrorId);

        gameObject.SetActive(false);

        if (inMirror != null)
        {
            inMirror.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                $"[Mirror3D] {mirrorId}의 InMirror가 연결되지 않았습니다.",
                this
            );
        }

        IsObtain = true;

        Debug.Log($"[Mirror3D] 거울 획득: {mirrorId}");
    }

    private void StartHolding()
    {
        if (IsHolding)
        {
            return;
        }

        IsHolding = true;

        Debug.Log($"[Mirror3D] 거울 들기: {mirrorId}");
    }
/// InMirror만
    private void Update()
    {
        if (!IsHolding)
        {
            return;
        }

        // E를 누르고 있는 동안 거울 이동
        if (Input.GetKey(KeyCode.E))
        {
            MoveMirror();
        }

        // E를 뗀 순간
        if (Input.GetKeyUp(KeyCode.E))
        {
            StopHolding();
        }
    }

    private void MoveMirror()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            return;
        }

        Vector3 targetPosition =
            cam.transform.position +
            cam.transform.forward * holdDistance;

        transform.position = targetPosition;

        transform.rotation = cam.transform.rotation;
    }

    private void StopHolding()
    {
        if (!IsHolding)
        {
            return;
        }

        IsHolding = false;

        Debug.Log($"[Mirror3D] 거울 놓음: {mirrorId}");

        // 여기서 자기 자리인지 확인
        // 자기 자리라면 PlaceMirror() 호출
    }

    public void PlaceMirror()
    {
        if (MirrorManager.Instance == null)
        {
            Debug.LogWarning("[Mirror3D] MirrorManager가 없습니다.");
            return;
        }

        if (!IsObtain)
        {
            Debug.LogWarning(
                $"[Mirror3D] 획득하지 않은 거울입니다: {mirrorId}"
            );

            return;
        }

        MirrorManager.Instance.MirrorPiecePlaced(mirrorId);

        Debug.Log($"[Mirror3D] 거울 배치 완료: {mirrorId}");
    }
}