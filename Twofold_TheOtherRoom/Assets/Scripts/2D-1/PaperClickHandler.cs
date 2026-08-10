using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PaperClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("Paper Zoom Panel")]
    public GameObject paperZoomPanel;

    private void Awake()
    {
        // 투명한 부분은 클릭 판정에서 제외 (알파값 0.1 이상인 부분만 클릭 가능)
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.alphaHitTestMinimumThreshold = 0.1f;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (paperZoomPanel != null)
        {
            paperZoomPanel.SetActive(true);
        }
    }
}