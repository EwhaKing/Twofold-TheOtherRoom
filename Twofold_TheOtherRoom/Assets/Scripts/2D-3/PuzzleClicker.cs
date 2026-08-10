using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzleClicker : MonoBehaviour, IPointerClickHandler
{
    [Header("줌 컨트롤러 연결")]
    public NetPuzzleZoomController zoomController;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (zoomController != null)
        {
            Debug.Log("[PuzzleClicker] 전개도 클릭됨 ➔ 퍼즐 줌인 실행!");
            zoomController.ZoomInToPuzzle();
        }
    }
}