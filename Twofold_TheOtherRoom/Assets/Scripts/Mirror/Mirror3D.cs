using UnityEngine;

public class Mirror3D : MonoBehaviour, IMouseHoldable
{
    [Header("Mirror Settings")]
    [SerializeField] private string mirrorId;
    public string MirrorId => mirrorId;

    [Header("Holding Settings")]
    [SerializeField] private float holdDistance = 1.5f;
    [SerializeField] private float holdRightOffset = 0.7f;
    [SerializeField] private float holdDownOffset = 0.5f;

    [Header("Correct Position")]
    [SerializeField] private float snapDistance = 0.5f;

    public bool IsHolding { get; private set; }

    private bool isPlaced;
    public bool IsPlaced => isPlaced;

    private void Awake()
    {
        IsHolding = false;
        isPlaced = false;
    }

    public void MouseHoldInteract()
    { 
        Debug.Log($"[Mirror3D] Interact 호출됨: {mirrorId}");

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

        if (Input.GetMouseButtonUp(0))
        {
            IsHolding=false;
           
            PutMirror(); 
        } 
    }

    public void GetMirror()
    {
        if (MirrorManager.Instance == null)
        {
            Debug.LogWarning("[Mirror3D] MirrorManager가 없습니다.");
            return;
        }

        MirrorManager.Instance.GetMirrorPiece(mirrorId);

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
        //거울조각에 rigidbody가 있으면, 중력 삭제.  
            Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }


        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(-90, 0, 0);

        isPlaced = true;
        IsHolding = false;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Ding);
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

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
