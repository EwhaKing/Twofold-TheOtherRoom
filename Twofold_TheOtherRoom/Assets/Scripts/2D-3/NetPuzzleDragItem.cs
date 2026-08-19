 using UnityEngine;
using UnityEngine.EventSystems;

public class NetPuzzleDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("아이템 고유 ID (1~5)")]
    public int itemID;

    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public Transform startParent;
    [HideInInspector] public Vector2 startAnchoredPos;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        startParent = transform.parent;
        startAnchoredPos = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        rectTransform.anchoredPosition = Vector2.zero; // 슬롯 중앙 정렬
        canvasGroup.blocksRaycasts = true;
    }

    public void ResetToOriginalPosition()
    {
        parentAfterDrag = startParent;
        transform.SetParent(startParent);
        rectTransform.anchoredPosition = startAnchoredPos;
    }
}
