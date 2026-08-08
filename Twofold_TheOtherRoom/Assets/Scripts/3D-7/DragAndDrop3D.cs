using System;
using UnityEngine;

// 범위 내 drag를 가능하게 하는 범용 스크립트
// drag 시작, 끝 이벤트 발행
// drag할 object에 부착
public class DragAndDrop3D : MonoBehaviour
{
    [SerializeField] bool canDrag;
    [SerializeField] Camera puzzleCamera;
    
    [Header("Drag Range")]
    [SerializeField] Transform topLeft;
    [SerializeField] Transform bottomRight;

    Vector3 mouseOffset;

    public event Action OnPickUp;
    public event Action OnRelease;

    private Vector3 GetObjectScreenPoint()
    {
        return puzzleCamera.WorldToScreenPoint(transform.position);
    }

    private void OnMouseDown()
    {
        if(!canDrag) return;
        mouseOffset = Input.mousePosition - GetObjectScreenPoint();
        OnPickUp?.Invoke();
    }

    private void OnMouseDrag()
    {
        if(!canDrag) return;
        Vector3 clampedPosition = puzzleCamera.ScreenToWorldPoint(Input.mousePosition - mouseOffset);
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, topLeft.position.x, bottomRight.position.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, bottomRight.position.y, topLeft.position.y);
        transform.position = clampedPosition;
    }

    private void OnMouseUp()
    {
        if(!canDrag) return;
        OnRelease?.Invoke();
    }
}
