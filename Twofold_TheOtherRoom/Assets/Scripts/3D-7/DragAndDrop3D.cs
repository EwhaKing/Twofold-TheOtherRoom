using System;
using UnityEngine;

// 범위 내 drag를 가능하게 하는 범용 스크립트
// 누르고 움직이면 drag, 누르고 안 움직이면 click으로 구분해서 이벤트 발행
// drag할 object에 부착
public class DragAndDrop3D : MonoBehaviour
{
    [SerializeField] bool canDrag;
    [SerializeField] Camera puzzleCamera;

    [Header("Drag Range")]
    [SerializeField] Transform topLeft;
    [SerializeField] Transform bottomRight;

    [Header("Click/Drag")]
    [Tooltip("이 픽셀 이상 움직여야 drag로 판정. 그 전에 떼면 click")]
    [SerializeField] float dragThreshold = 8f;

    Vector3 mouseOffset;
    Vector3 mouseDownPosition;
    bool isDragging;

    public event Action OnPickUp;
    public event Action OnRelease;
    public event Action OnClick;

    private Vector3 GetObjectScreenPoint()
    {
        return puzzleCamera.WorldToScreenPoint(transform.position);
    }

    private void OnMouseDown()
    {
        if(!canDrag) return;
        // 아직 drag인지 click인지 모르므로 OnPickUp은 미룬다
        mouseOffset = Input.mousePosition - GetObjectScreenPoint();
        mouseDownPosition = Input.mousePosition;
        isDragging = false;
    }

    private void OnMouseDrag()
    {
        if(!canDrag) return;

        if(!isDragging)
        {
            if(Vector3.Distance(Input.mousePosition, mouseDownPosition) < dragThreshold) return;
            isDragging = true;
            OnPickUp?.Invoke();
        }

        transform.position = ClampToRange(puzzleCamera.ScreenToWorldPoint(Input.mousePosition - mouseOffset));
    }

    // topLeft/bottomRight의 부모 공간에서 자름
    // 월드 축으로 자르면 부모가 회전된 배치에서 min/max가 뒤집혀 한쪽 경계로 튕겨나감
    private Vector3 ClampToRange(Vector3 worldPosition)
    {
        Transform range = topLeft.parent;

        Vector3 corner1 = ToRange(range, topLeft.position);
        Vector3 corner2 = ToRange(range, bottomRight.position);
        Vector3 local = ToRange(range, worldPosition);

        local.x = Mathf.Clamp(local.x, Mathf.Min(corner1.x, corner2.x), Mathf.Max(corner1.x, corner2.x));
        local.y = Mathf.Clamp(local.y, Mathf.Min(corner1.y, corner2.y), Mathf.Max(corner1.y, corner2.y));

        return FromRange(range, local);
    }

    private static Vector3 ToRange(Transform range, Vector3 world) => range ? range.InverseTransformPoint(world) : world;
    private static Vector3 FromRange(Transform range, Vector3 local) => range ? range.TransformPoint(local) : local;

    private void OnMouseUp()
    {
        if(!canDrag) return;

        if(isDragging)
        {
            isDragging = false;
            OnRelease?.Invoke();
        }
        else OnClick?.Invoke();
    }
}
