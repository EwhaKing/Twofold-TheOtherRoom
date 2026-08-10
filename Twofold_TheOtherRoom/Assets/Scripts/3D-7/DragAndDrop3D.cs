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

        Vector3 clampedPosition = puzzleCamera.ScreenToWorldPoint(Input.mousePosition - mouseOffset);
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, topLeft.position.x, bottomRight.position.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, bottomRight.position.y, topLeft.position.y);
        transform.position = clampedPosition;
    }

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
