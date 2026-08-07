using UnityEngine;

// 범위 내 drag를 가능하게 하는 스크립트
public class DragAndDrop3D : MonoBehaviour
{
    [SerializeField] bool canDrag;
    [SerializeField] Camera puzzleCamera;
    
    [Header("Drag Range")]
    [SerializeField] Transform topLeft;
    [SerializeField] Transform bottomRight;

    Vector3 mouseOffset;

    private Vector3 GetObjectScreenPoint()
    {
        return puzzleCamera.WorldToScreenPoint(transform.position);
    }

    private void OnMouseDown()
    {
        if(!canDrag) return;
        mouseOffset = Input.mousePosition - GetObjectScreenPoint();
    }

    private void OnMouseDrag()
    {
        if(!canDrag) return;
        Vector3 clampedPosition = puzzleCamera.ScreenToWorldPoint(Input.mousePosition - mouseOffset);
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, topLeft.position.x, bottomRight.position.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, bottomRight.position.y, topLeft.position.y);
        transform.position = clampedPosition;
    }
}
