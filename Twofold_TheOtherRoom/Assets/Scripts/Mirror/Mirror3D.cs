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
    [Tooltip("거울 조각은 로컬 X축이 면의 법선이라, 카메라 쪽을 보도록 돌려서 듦")]
    [SerializeField] private Vector3 holdRotationOffset = new Vector3(0f, 90f, 0f);

    [Header("Correct Position")]
    [Tooltip("카메라 정면 레이로 MirrorFrame 태그 프레임을 감지하는 거리")]
    [SerializeField] private float frameDetectDistance = 2.5f;

    [Header("Placement Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    //[SerializeField] private float groundCheckHeight = 1f;
    [SerializeField] private float groundCheckDistance = 6f;

    [Tooltip("플레이어 쪽(뒤)으로 당겨 보는 횟수")]
    [SerializeField] private int searchIterations = 20;
    [Tooltip("뒤로 한 번 당길 때 이동 거리")]
    [SerializeField] private float searchStep = 0.1f;

    [Tooltip("좁은 복도 대응: 각 거리마다 좌우로 옮겨 보는 최대 횟수 (한쪽 기준). 0이면 좌우 탐색 안 함")]
    [SerializeField] private int lateralSearchCount = 10;
    [Tooltip("좌우로 한 번 옮길 때 이동 거리")]
    [SerializeField] private float lateralSearchStep = 0.1f;

    [Tooltip("벽/문에서 이만큼 떨어진 곳까지만 거울을 들 수 있음")]
    [SerializeField] private float wallMargin = 0.1f;

    public bool IsHolding { get; private set; }

    private bool isPlaced;
    public bool IsPlaced => isPlaced;

    private BoxCollider boxCollider;
    private Rigidbody rb;

    [Header("Placement Debug")]
    [SerializeField] private bool debugPlacement = true;


    private void Awake()
    {
        IsHolding = false;
        isPlaced = false;

        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
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

        // 좌클릭을 뗐을 때 거울 놓기
        if (Input.GetMouseButtonUp(0))
        {
            IsHolding = false;
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

        // 들고 있는 동안 벽과 충돌하지 않게 Collider 끄기
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        // 들고 있는 동안 물리 영향 제거
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.useGravity = false;
            rb.isKinematic = true;
        }

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

        // 카메라와 거울 사이에 벽/문이 있으면 벽 앞까지만 들고 있음
        Vector3 toTarget = targetPosition - cam.transform.position;
        if (Physics.Raycast(
                cam.transform.position,
                toTarget.normalized,
                out RaycastHit wallHit,
                toTarget.magnitude,
                wallLayer,
                QueryTriggerInteraction.Ignore))
        {
            targetPosition =
                cam.transform.position
                + toTarget.normalized * Mathf.Max(0f, wallHit.distance - wallMargin);
        }

        Quaternion holdRotation = cam.transform.rotation * Quaternion.Euler(holdRotationOffset);

        // 조각의 pivot은 전체 거울 원점이라 조각마다 메쉬가 pivot에서 떨어져 있음
        // → 조각 중심(콜라이더 중심)이 목표 위치에 오도록 보정
        if (boxCollider != null)
        {
            Vector3 pieceOffset = holdRotation * Vector3.Scale(boxCollider.center, transform.lossyScale);
            targetPosition -= pieceOffset;
        }

        transform.SetPositionAndRotation(targetPosition, holdRotation);
        if (rb != null)
        {
            rb.position = targetPosition;
            rb.rotation = holdRotation;
        }
    }

    private void PutMirror()
    {
        // 정답 위치 근처라면 거울판에 스냅
        if (IsCorrectPosition())
        {
            PlaceMirror();
            return;
        }

        // 일반 바닥에 놓기 시도
        if (TryPlaceOnGround())
        {
            Debug.Log($"[Mirror3D] 거울을 바닥에 놓음: {mirrorId}");
            return;
        }

        // 안전한 위치를 못 찾으면 다시 들기
        Debug.LogWarning(
            $"[Mirror3D] {mirrorId}를 놓을 안전한 위치를 찾지 못했습니다."
        );

        PickMirror();
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

        Camera cam = Camera.main;

        if (cam == null)
        {
            return false;
        }

        if (!Physics.Raycast(
                cam.transform.position,
                cam.transform.forward,
                out RaycastHit hit,
                frameDetectDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide))
        {
            return false;
        }

        // MirrorFrame 닿았는지
        return hit.collider.CompareTag("MirrorFrame");
    }

    private void PlaceMirror()
    {
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(-90, 0, 0);
        if (rb != null)
        {
            rb.position = transform.position;
            rb.rotation = transform.rotation;
        }
        Physics.SyncTransforms();

        isPlaced = true;
        IsHolding = false;

        // 정답 위치에 고정된 거울은 Collider 불필요
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Ding);
        }


        MirrorManager.Instance?.MirrorPiecePlaced(mirrorId);

        Debug.Log($"[Mirror3D] 거울 배치 완료: {mirrorId}");
    }

    private bool TryPlaceOnGround()
    {
        Camera cam = Camera.main;

        if (cam == null || boxCollider == null)
        {
            return false;
        }

        Quaternion placeRotation = Quaternion.Euler(0f, cam.transform.eulerAngles.z, 270f);
        Vector3 originalPosition = transform.TransformPoint(boxCollider.center);

        // 수평 방향만 사용 (아래를 보고 있어도 후보가 위로 뜨지 않게)
        Vector3 flatForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 flatRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        // 현재 위치부터 플레이어 방향으로 조금씩 당겨오면서,
        // 각 거리마다 좌우로도 옮겨 보며 안전한 위치를 찾음
        // (좁은 복도에서 한쪽 벽에 붙어 있어도 가운데 쪽 빈 공간을 찾을 수 있음)
        //
        //   뒤로 i칸 × 좌우 j칸 순서: (0,0) (0,-1) (0,+1) (0,-2) (0,+2) ... (1,0) (1,-1) ...
        //   → 원래 위치에서 가까운 곳부터 우선
        int lateralTries = 1 + Mathf.Max(0, lateralSearchCount) * 2;
        for (int n = 0; n < searchIterations * lateralTries; n++)
        {
            int i = n / lateralTries;           // 뒤로 몇 칸
            int k = n % lateralTries;           // 0, 1, 2, 3, 4 ...
            int j = (k + 1) / 2 * (k % 2 == 1 ? -1 : 1); // 0, -1, +1, -2, +2 ... (왼쪽 먼저)

            Vector3 candidatePosition =
                originalPosition
                - flatForward * (searchStep * i)
                + flatRight * (lateralSearchStep * j);

            // 후보 위치 아래에 Ground가 있는지 검사
            if (!TryGetGroundPosition(
                    candidatePosition,
                    placeRotation,
                    out Vector3 groundPosition))
            {
                continue;
            }

            // 카메라에서 놓을 위치까지 벽/문에 막혀 있으면
            // 벽 너머 바닥이므로 제외
            if (IsBlockedFromCamera(cam, groundPosition))
            {
                continue;
            }

            // 배치할 위치에서 계단을 포함한 Ground와 Wall의 겹침 검사
            if (!IsSafePosition(
                    groundPosition,
                    placeRotation))
            {
                continue;
            }

            // 안전한 위치를 찾았으면 실제 배치
            transform.position = groundPosition;
            transform.rotation = placeRotation;

            if (boxCollider != null)
            {
                boxCollider.enabled = true;
            }

            // 검사한 좌표를 Rigidbody에도 적용한 뒤 물리를 재개한다.
            if (rb != null)
            {
                rb.position = groundPosition;
                rb.rotation = placeRotation;
                rb.detectCollisions = true;
            }
            Physics.SyncTransforms();

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = true;
                rb.WakeUp();
            }

            return true;
        }

        return false;
    }

    private Vector3 GetPlacementHalfExtents()
    {
        Vector3 scale = transform.lossyScale;
        scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        return Vector3.Scale(boxCollider.size * 0.5f, scale);
    }

    private bool TryGetGroundPosition(
        Vector3 candidatePosition,
        Quaternion rotation,
        out Vector3 groundPosition)
    {
        if (Physics.Raycast(
                candidatePosition,
                Vector3.down,
                out RaycastHit hit,
                groundCheckDistance,
                groundLayer,
                QueryTriggerInteraction.Ignore))
        {
            Vector3 halfExtents = GetPlacementHalfExtents();
            float verticalExtent =
                Mathf.Abs((rotation * Vector3.right).y) * halfExtents.x +
                Mathf.Abs((rotation * Vector3.up).y) * halfExtents.y +
                Mathf.Abs((rotation * Vector3.forward).y) * halfExtents.z;
            Vector3 centerOffset = rotation * Vector3.Scale(boxCollider.center, transform.lossyScale);

            // 피벗 대신 콜라이더 밑면을 바닥 위에 둔다. 간격은 바닥 접촉의 오검출을 방지한다.
            groundPosition = hit.point + Vector3.up * (verticalExtent + 0.02f) - centerOffset;
            return true;
        }

        groundPosition = Vector3.zero;
        return false;
    }

    private bool IsBlockedFromCamera(Camera cam, Vector3 groundPosition)
    {
        // 바닥 표면에 딱 붙은 점은 바닥 모서리에 걸릴 수 있어 살짝 띄움
        Vector3 target = groundPosition + Vector3.up * 0.05f;

        return Physics.Linecast(
            cam.transform.position,
            target,
            wallLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private bool IsSafePosition(
        Vector3 position,
        Quaternion rotation)
    {
        if (boxCollider == null)
        {
            return false;
        }

        Vector3 worldCenter = position + rotation * Vector3.Scale(
            boxCollider.center, transform.lossyScale);
        Collider[] hits = Physics.OverlapBox(
            worldCenter,
            GetPlacementHalfExtents(),
            rotation,
            groundLayer | wallLayer,
            QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            if (hit == boxCollider || (rb != null && hit.attachedRigidbody == rb))
            {
                continue;
            }

            if (debugPlacement)
            {
                Debug.Log($"[Mirror3D] 배치 공간 겹침: {hit.name}");
            }
            return false;
        }

        return true;
    }
}
