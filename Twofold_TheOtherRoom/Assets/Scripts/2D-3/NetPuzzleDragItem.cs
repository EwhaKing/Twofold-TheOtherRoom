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
        parentAfterDrag = transform.parent;

        // 핵심: 누르는 순간 발판 뒤로 들어가는 것을 막기 위해 Canvas 최하단(화면 맨 앞)으로 부모 변경
        if (mainCanvas != null)
        {
            transform.SetParent(mainCanvas.transform);
        }
        
        transform.SetAsLastSibling(); // 맨 앞으로 가져오기
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
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