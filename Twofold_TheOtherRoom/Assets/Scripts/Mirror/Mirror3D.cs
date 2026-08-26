using UnityEngine;

public class Mirror3D : MonoBehaviour, IInteractable
{
    [Header("Mirror Settings")]
    [SerializeField] private string mirrorId;

    [Header("Holding Settings")]
    [SerializeField] private float holdDistance = 1.5f;
    [SerializeField] private float holdRightOffset = 0.7f;
    [SerializeField] private float holdDownOffset = 0.5f;

    [Header("Correct Position")]
    [SerializeField] private float snapDistance = 0.8f;

    public bool IsHolding { get; private set; }

    private bool isPlaced;
    private bool isObtain;

    private void Awake()
    {
        IsHolding = false;
        isPlaced = false;
        isObtain = false;
    }

    public void Interact()
    {
        if (isPlaced)
        {
            return;
        }

        if (!IsHolding)
        {
            PickMirror();
        }
    }

    private void Update()
    {
        if (!IsHolding)
        {
            return;
        }

        MoveMirror();
    }

    public void GetMirror()
    {
        if (MirrorManager.Instance == null)
        {
            Debug.LogWarning("[Mirror3D] MirrorManager가 없습니다.");
            return;
        }

        MirrorManager.Instance.GetMirrorPiece(mirrorId);

        isObtain = true;

        Debug.Log($"[Mirror3D] 거울 획득: {mirrorId}");
    }

    private void PickMirror()
    {
        IsHolding = true;

        Debug.Log($"[Mirror3D] 거울 들기: {mirrorId}");

    }

    private void MoveMirror()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            return;
        }

        Vector3 targetPosition =
            cam.transform.position
            + cam.transform.forward * holdDistance
            + cam.transform.right * holdRightOffset
            - cam.transform.up * holdDownOffset;

        transform.position = targetPosition;

        transform.rotation = cam.transform.rotation;

        if (Input.GetKeyDown(KeyCode.E))
        {
            IsHolding=false;
            PutMirror();
        }
    }

    private void PutMirror()
    {
        if (IsCorrectPosition())
        {
            PlaceMirror();
            return;
        }

        PlaceOnGround();

        Debug.Log($"[Mirror3D] 거울을 바닥에 놓음: {mirrorId}");
    }

    private bool IsCorrectPosition()
    {
        if (transform.parent == null)
        {
            Debug.LogWarning(
                $"[Mirror3D] {mirrorId}의 상위 오브젝트가 없습니다."
            );

            return false;
        }

        Vector3 localPosition = transform.localPosition;

        return Mathf.Abs(localPosition.x) <= snapDistance &&
               Mathf.Abs(localPosition.y) <= snapDistance &&
               Mathf.Abs(localPosition.z) <= snapDistance;
    }

    private void PlaceMirror()
    {
        transform.localPosition = Vector3.zero;

        isPlaced = true;
        IsHolding = false;

        MirrorManager.Instance?.MirrorPiecePlaced(mirrorId);

        Debug.Log($"[Mirror3D] 거울 배치 완료: {mirrorId}");
    }

    private void PlaceOnGround()
    {
        Ray ray = new Ray(
            transform.position + Vector3.up * 1f,
            Vector3.down
        );

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            transform.position = hit.point;

            transform.rotation = Quaternion.Euler(
                0f,
                transform.rotation.eulerAngles.z,
                270f
            );

            return;
        }

        Debug.LogWarning(
            $"[Mirror3D] {mirrorId}가 내려놓을 바닥을 찾지 못했습니다."
        );
    }
}