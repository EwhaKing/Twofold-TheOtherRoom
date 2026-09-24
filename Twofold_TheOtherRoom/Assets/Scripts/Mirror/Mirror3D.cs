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

    [Header("Placement Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    //[SerializeField] private float groundCheckHeight = 1f;
    [SerializeField] private float groundCheckDistance = 6f;

    [SerializeField] private int searchIterations = 20;
    [SerializeField] private float searchStep = 0.1f;

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

        transform.SetPositionAndRotation(targetPosition, cam.transform.rotation);
        if (rb != null)
        {
            rb.position = targetPosition;
            rb.rotation = cam.transform.rotation;
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

        Vector3 localPosition = transform.localPosition;

        return Mathf.Abs(localPosition.x) <= snapDistance &&
               Mathf.Abs(localPosition.y) <= snapDistance &&
               Mathf.Abs(localPosition.z) <= snapDistance;
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

        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;

        // 현재 위치부터 플레이어 방향으로 조금씩 당겨오면서
        // 안전한 위치를 찾음
        for (int i = 0; i < searchIterations; i++)
        {
            Vector3 candidatePosition =
                originalPosition
                - cam.transform.forward * (searchStep * i);

            // 후보 위치 아래에 Ground가 있는지 검사
            if (!TryGetGroundPosition(
                    candidatePosition,
                    out Vector3 groundPosition))
            {
                continue;
            }

            // 바닥에 놓일 때 최종 회전값
            Quaternion placeRotation = Quaternion.Euler(
                0f,
                originalRotation.eulerAngles.z,
                270f
            );

            // 실제로 배치할 위치에서 Wall과 겹치는지 검사
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

    private bool TryGetGroundPosition(
        Vector3 candidatePosition,
        out Vector3 groundPosition)
    {
        Vector3 rayStart =
            candidatePosition;

        // Ground 레이어만 검사
        if (Physics.Raycast(
                rayStart,
                Vector3.down,
                out RaycastHit hit,
                groundCheckDistance,
                groundLayer,
                QueryTriggerInteraction.Ignore))
        {
            groundPosition = hit.point;
            return true;
        }

        groundPosition = Vector3.zero;
        return false;
    }

    private bool IsSafePosition(
        Vector3 position,
        Quaternion rotation)
    {
        if (boxCollider == null)
        {
            return false;
        }

        // BoxCollider.size는 전체 크기이므로
        // CheckBox에 넣기 위해 절반 크기로 변경
        Vector3 halfExtents = Vector3.Scale(
            boxCollider.size * 0.5f,
            transform.lossyScale
        );



        // BoxCollider.center가 (0,0,0)이 아닐 수도 있으므로
        // 실제 월드 중심 계산
        Vector3 scaledCenter = Vector3.Scale(
            boxCollider.center,
            transform.lossyScale
        );

        Vector3 worldCenter =
            position + rotation * scaledCenter;

        // Wall 레이어와 겹치는지 검사
        bool isTouchingWall = Physics.CheckBox(
            worldCenter,
            halfExtents,
            rotation,
            wallLayer,
            QueryTriggerInteraction.Ignore
        );

        Collider[] hits = Physics.OverlapBox(
            worldCenter, halfExtents, rotation, wallLayer,
            QueryTriggerInteraction.Collide);

        foreach (Collider c in hits)
        {
            Debug.Log($"[겹침] {c.name} / layer={LayerMask.LayerToName(c.gameObject.layer)} / trigger={c.isTrigger}");
        }

        // 벽과 안 겹치면 안전
        return !isTouchingWall;
    }


}