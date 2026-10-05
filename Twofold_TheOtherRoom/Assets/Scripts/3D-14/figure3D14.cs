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
    private board3D14 board;

    [System.Serializable]
    public class FigurePosition
    {
        public FigurePositionName positionName;
        public Vector3 localPosition;
    }

    [Header("Figure Positions")]
    [SerializeField] private FigurePosition[] possiblePositions;

    private FigurePositionName currentPosition = FigurePositionName.None;
    
    [Header("Drag Settings")]
    [SerializeField] private float snapDistance = 0.5f;

    [Header("Inspection Camera")]
    [SerializeField] private Camera inspectionCamera;

    private bool isDragging = false;

    private Vector3 returnPosition;

    private Plane dragPlane;
    private Vector3 dragOffset;

    private void Awake()
    {
        returnPosition = transform.localPosition;
        board = GetComponentInParent<board3D14>();
    }

    private void Update()
    {
        if (board != null && board.IsSolved)
        {
            return;
        }

        if (!IsInspectionView())
        {
            return;
        }

        if (!isDragging)
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryStartDrag();
            }

            return;
        }

        if (Input.GetMouseButton(0))
        {
            DragFigure();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseFigure();
        }
    }

    private Camera GetInspectionCamera()
    {
        if (InspectionCameraRig.Instance == null)
            return null;

        return InspectionCameraRig.Instance.ViewCamera;
    }

    private bool IsInspectionView()
    {
        if (InspectionCameraRig.Instance == null)
            return false;

        return InspectionCameraRig.Instance.IsViewing;
    }

    public FigurePositionName GetCurrentPositionName()
    {
        return currentPosition;
    }

    private void TryStartDrag()
    {
        if (inspectionCamera == null)
            return;

        Ray ray = inspectionCamera.ScreenPointToRay(
            Input.mousePosition
        );

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            return;
        }

        if (hit.transform != transform &&
            !hit.transform.IsChildOf(transform))
        {
            return;
        }

        StartDrag();
    }

    private void StartDrag()
    {
        isDragging = true;

        Vector3 pos = transform.localPosition;
        pos.z = 0f;
        transform.localPosition = pos;

        Vector3 normal =
            transform.parent != null
            ? transform.parent.forward
            : Vector3.forward;

        dragPlane = new Plane(
            normal,
            transform.position
        );
        Camera cam = GetInspectionCamera();

        if (cam != null)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (dragPlane.Raycast(ray, out float distance))
            {
                Vector3 mouseWorldPosition = ray.GetPoint(distance);

                dragOffset = transform.position - mouseWorldPosition;
            }
        }
    }

    private void DragFigure()
    {
        Camera cam = GetInspectionCamera();

        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(
            Input.mousePosition
        );

        if (!dragPlane.Raycast(
                ray,
                out float distance))
        {
            return;
        }

        Vector3 worldPosition =
            ray.GetPoint(distance);

        worldPosition += dragOffset;
    
        if (transform.parent != null)
        {
            Vector3 localPosition =
                transform.parent.InverseTransformPoint(worldPosition);
    
            transform.localPosition = new Vector3(
                localPosition.x,
                localPosition.y,
                0f
            );
        }
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
            float distance = Vector3.Distance(
                transform.localPosition,
                position.localPosition
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPosition = position;
            }
        }

        if (nearestPosition == null ||
            nearestDistance > snapDistance)
        {
            transform.localPosition = returnPosition;

            Debug.Log(
                $"[Figure] {currentPosition} → 이동 실패 (위치를 찾지 못함)"
            );

            return;
        }

        FigurePositionName previousPosition = currentPosition;

        bool canMove = board.CanMoveToPosition(
            this,
            nearestPosition.positionName
        );

        if (canMove)
        {
            transform.localPosition =
                nearestPosition.localPosition;

            currentPosition =
                nearestPosition.positionName;
            
            board.CheckPuzzleSolved();
        }
        else
        {
            transform.localPosition = returnPosition;
        }

    }

    public void MouseHoldInteract(){}

    public void ResetPos()
    {
        transform.localPosition = returnPosition;

        currentPosition = FigurePositionName.None;
    }
}