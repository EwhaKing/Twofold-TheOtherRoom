using UnityEngine;
using UnityEngine.EventSystems;

public class BoxClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("Connected Panels")]
    public GameObject lockZoomPanel;    // 자물쇠 확대 패널 (Lock_Zoom_Panel)
    public GameObject boxInsidePanel;  // 상자 안속/거울 확대 패널 (Box_Inside_Panel)
    public PadlockController puzzleLogic; // 기존 PadlockController 연결!

    public void OnPointerClick(PointerEventData eventData)
    {
        if (puzzleLogic == null) return;

        if (!puzzleLogic.IsUnlocked)
        {
            // 안 풀림 -> 자물쇠 패널 열기
            if (lockZoomPanel != null) lockZoomPanel.SetActive(true);
        }
        else
        {
            // 풀림 -> 상자 안속/거울 패널 열기
            if (boxInsidePanel != null) boxInsidePanel.SetActive(true);
        }
    }
}