using UnityEngine;

public enum FigurePositionName
{
    Front,
    Right,
    Back,
    Left,
    None
}

public class figure3D14 : MonoBehaviour, IMouseHoldable
{
    [System.Serializable]
    public class FigurePosition
    {
        public FigurePositionName positionName;
        public Vector3 localPosition;
    }

    [Header("Figure Positions")]
    [SerializeField] private FigurePosition[] possiblePositions;

    [Header("Drag Settings")]
    [SerializeField] private float snapDistance = 0.5f;

    // 드래그 중 높이. 보드 로컬 z, 슬롯 높이 0 기준
    [SerializeField] private float dragHeight = 0f;

    // 드래그 허용 InspectionCameraRig 카메라 인덱스
    [SerializeField] private int dragCameraIndex = 0;

    private board3D14 board;
    private FigurePositionName currentPosition = FigurePositionName.None;
    private Vector3 returnPosition;

    private bool isDragging = false;
    private Vector3 grabStartPosition;
    private Plane dragPlane;
    private Vector3 dragOffset;

    private void Awake()
    {
        returnPosition = transform.localPosition;
        board = GetComponentInParent<board3D14>();
    }

    private void Update()
    {
        if (!CanDragNow())
        {
            if (isDragging) CancelDrag();
            return;
        }

        if (!isDragging)
        {
            if (Input.GetMouseButtonDown(0)) TryStartDrag();
            return;
        }

        if (Input.GetMouseButton(0)) DragFigure();
        if (Input.GetMouseButtonUp(0)) ReleaseFigure();
    }

    private Camera GetInspectionCamera()
    {
        if (InspectionCameraRig.Instance == null) return null;

        return InspectionCameraRig.Instance.ViewCamera;
    }

    private bool CanDragNow()
    {
        InspectionCameraRig rig = InspectionCameraRig.Instance;

        if (board != null && board.IsSolved) return false;
        if (rig == null || !rig.IsViewing || rig.IsMoving) return false;

        return rig.currentCameraIndex == dragCameraIndex;
    }

    public FigurePositionName GetCurrentPositionName()
    {
        return currentPosition;
    }

    private void TryStartDrag()
    {
        Camera cam = GetInspectionCamera();
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        if (hit.transform != transform && !hit.transform.IsChildOf(transform)) return;

        StartDrag(hit.point);
    }

    private void StartDrag(Vector3 grabPoint)
    {
        isDragging = true;
        grabStartPosition = transform.localPosition;

        Vector3 beforeLift = transform.position;
        Vector3 pos = transform.localPosition;
        pos.z = dragHeight;
        transform.localPosition = pos;
        Vector3 lift = transform.position - beforeLift;

        Vector3 normal = transform.parent != null ? transform.parent.forward : Vector3.forward;
        dragPlane = new Plane(normal, grabPoint + lift);

        Camera cam = GetInspectionCamera();
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!dragPlane.Raycast(ray, out float distance)) return;

        dragOffset = transform.position - ray.GetPoint(distance);
    }

    private void DragFigure()
    {
        Camera cam = GetInspectionCamera();

        if (cam == null) return;
        if (transform.parent == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!dragPlane.Raycast(ray, out float distance)) return;

        Vector3 worldPosition = ray.GetPoint(distance) + dragOffset;
        Vector3 localPosition = transform.parent.InverseTransformPoint(worldPosition);
        transform.localPosition = new Vector3(localPosition.x, localPosition.y, dragHeight);
    }

    private void CancelDrag()
    {
        isDragging = false;
        transform.localPosition = grabStartPosition;
    }

    private void ReleaseFigure()
    {
        isDragging = false;
        CheckPosition();
    }

    private void CheckPosition()
    {
        FigurePosition nearestPosition = null;
        float nearestDistance = Mathf.Infinity;

        foreach (FigurePosition position in possiblePositions)
        {
            float distance = Vector2.Distance(transform.localPosition, position.localPosition);
            if (distance >= nearestDistance) continue;

            nearestDistance = distance;
            nearestPosition = position;
        }

        if (nearestPosition == null || nearestDistance > snapDistance ||
            !board.CanMoveToPosition(this, nearestPosition.positionName))
        {
            transform.localPosition = returnPosition;
            return;
        }

        transform.localPosition = nearestPosition.localPosition;
        currentPosition = nearestPosition.positionName;
        board.CheckPuzzleSolved();
    }

    public void MouseHoldInteract() { }

    public void ResetPos()
    {
        transform.localPosition = returnPosition;
        currentPosition = FigurePositionName.None;
    }
}
