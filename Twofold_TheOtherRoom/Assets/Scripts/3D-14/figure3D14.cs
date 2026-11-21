using System;
using System.Collections;
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
    [Serializable]
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
    [SerializeField] private float dragHeight = 0.1f;

    // 드래그 허용 InspectionCameraRig 카메라 인덱스
    [SerializeField] private int dragCameraIndex = 0;

    [Header("Insert Animation")]
    // 꽂기 시작 높이. 슬롯 기준 보드 로컬 z
    [SerializeField] private float insertHeight = 0.1f;
    // 꽂힌 뒤 슬롯보다 내려간 깊이
    [SerializeField] private float insertDepth = 0.05f;
    [SerializeField] private float insertDuration = 0.15f;
    [SerializeField] private AnimationCurve insertEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private board3D14 board;
    private FigurePositionName currentPosition = FigurePositionName.None;
    private Vector3 returnPosition;

    private bool isDragging = false;
    private Vector3 grabStartPosition;
    private Plane dragPlane;
    private Vector3 dragOffset;

    private Coroutine insertRoutine;

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
        if (insertRoutine != null) return false;
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

        if (!RaycastIgnoringBoard(ray, out RaycastHit hit)) return;
        if (hit.transform != transform && !hit.transform.IsChildOf(transform)) return;

        StartDrag(hit.point);
    }

    // 보드 상호작용 콜라이더 제외 가장 가까운 충돌
    private bool RaycastIgnoringBoard(Ray ray, out RaycastHit nearest)
    {
        nearest = default;
        bool found = false;

        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            if (board != null && hit.collider.gameObject == board.gameObject) continue;
            if (found && hit.distance >= nearest.distance) continue;

            nearest = hit;
            found = true;
        }

        return found;
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
            transform.localPosition = grabStartPosition;
            return;
        }

        currentPosition = nearestPosition.positionName;
        insertRoutine = StartCoroutine(InsertRoutine(nearestPosition.localPosition));
    }

    private IEnumerator InsertRoutine(Vector3 slot)
    {
        Vector3 from = slot + Vector3.forward * insertHeight;
        Vector3 to = slot - Vector3.forward * insertDepth;

        yield return Animate(insertDuration, insertEase,
            t => transform.localPosition = Vector3.LerpUnclamped(from, to, t));

        insertRoutine = null;
        board.CheckPuzzleSolved();
    }

    private IEnumerator Animate(float duration, AnimationCurve ease, Action<float> apply)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            apply(ease.Evaluate(elapsed / duration));
            yield return null;
        }

        apply(1f);
    }

    public void MouseHoldInteract() { }

    public void ResetPos()
    {
        if (insertRoutine != null)
        {
            StopCoroutine(insertRoutine);
            insertRoutine = null;
        }

        transform.localPosition = returnPosition;
        currentPosition = FigurePositionName.None;
    }
}
