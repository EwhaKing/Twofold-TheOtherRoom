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
        
        // 드래그를 시작할 때 어두운 배경에 가려지지 않도록 맨 앞으로 끌어올림
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
        
        // 슬롯이나 기존 부모로 들어간 뒤에도 발판 뒤로 숨지 않도록 부모 내에서 최상단 배치
        transform.SetAsLastSibling();
        
        canvasGroup.blocksRaycasts = true;
    }

    public void ResetToOriginalPosition()
    {
        parentAfterDrag = startParent;
        transform.SetParent(startParent);
        rectTransform.anchoredPosition = startAnchoredPos;
        
        // 원위치로 리셋될 때도 배경 뒤로 가려지지 않게 UI 순서를 맨 앞으로 당김
        transform.SetAsLastSibling();
    }
}