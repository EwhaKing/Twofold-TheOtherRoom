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

    private Rigidbody heldBody;
    private BoxCollider heldCollider;
    private RigidbodyConstraints originalConstraints;
    private const float FollowSpeed = 20f;
    private const float MaxHoldSpeed = 10f;

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


        if (!Input.GetMouseButton(0))
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
        heldCollider = GetComponent<BoxCollider>();
        if (heldCollider == null || Camera.main == null)
            return;

        heldBody = GetComponent<Rigidbody>();

        if (heldBody == null)
            return;

        originalConstraints = heldBody.constraints;
        heldBody.constraints = RigidbodyConstraints.FreezeRotation;
        heldBody.angularVelocity = Vector3.zero;
        heldBody.useGravity = false;

        IsHolding = true;
    }

    private void FixedUpdate()
    {
        if (IsHolding)
            MoveMirror();
    }

    private void MoveMirror()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            ReleasePhysics();
            return;
        }

        Vector3 targetPosition =
            cam.transform.position
            + cam.transform.forward * holdDistance
            + cam.transform.right * holdRightOffset
            - cam.transform.up * holdDownOffset;

        // Follow with physics: wall contacts can stop or slide the mirror.
        // Aim its actual centre at the hand position, not the imported pivot.
        heldBody.linearVelocity = Vector3.ClampMagnitude(
            (targetPosition - heldCollider.bounds.center) * FollowSpeed, MaxHoldSpeed);
    }

    private void ReleasePhysics()
    {
        IsHolding = false;
        if (heldBody == null)
            return;

        heldBody.linearVelocity = Vector3.zero;
        heldBody.angularVelocity = Vector3.zero;
        heldBody.constraints = originalConstraints;
        heldBody.useGravity = true;
    }

    private void OnDisable()
    {
        if (IsHolding)
            ReleasePhysics();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && IsHolding)
            ReleasePhysics();
    }

    private void PutMirror()
    {
        ReleasePhysics();
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
        // Let gravity place it; teleporting the pivot onto the floor embeds the mesh.
        foreach (RaycastHit hit in Physics.RaycastAll(
            heldCollider.bounds.center, Vector3.down, 10f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.transform.IsChildOf(transform))
                return;
        }

        // Preserve the requested recovery when there is no floor below the mirror.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            heldBody.position = player.transform.position;
            return;
        }

        Debug.LogWarning($"[Mirror3D] {mirrorId}: 바닥과 Player를 찾지 못했습니다.");
    }
}
