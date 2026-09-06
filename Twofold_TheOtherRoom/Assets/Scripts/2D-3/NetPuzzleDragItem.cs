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
    private Canvas mainCanvas;

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
        // 최상위 Canvas 찾기
        mainCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
{
    // 드롭 실패 시 돌아갈 현재 부모
    parentAfterDrag = transform.parent;

    // 드래그하는 동안 Puzzle_Net 바로 아래로 꺼내기
    transform.SetParent(startParent, true);
    transform.SetAsLastSibling();

    canvasGroup.blocksRaycasts = false;
}

    public void OnDrag(PointerEventData eventData)
{
    rectTransform.anchoredPosition += eventData.delta;
}

    public void OnEndDrag(PointerEventData eventData)
    {
        // 손을 뗐을 때 지정된 슬롯/부모로 이동
        transform.SetParent(parentAfterDrag);
        rectTransform.anchoredPosition = Vector2.zero; // 슬롯 중앙 정렬
        
        canvasGroup.blocksRaycasts = true;
    }

    public void ResetToOriginalPosition()
    {
        parentAfterDrag = startParent;
        transform.SetParent(startParent);
        rectTransform.anchoredPosition = startAnchoredPos;
        
        transform.SetAsLastSibling();
    }
}