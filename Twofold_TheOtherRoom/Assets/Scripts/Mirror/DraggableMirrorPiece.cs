using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>MirrorFrame 안에서 정답 위치로 드래그하는 공용 거울 조각입니다.</summary>
[RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(Image))]
public class DraggableMirrorPiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private float snapDistance = 80f;

    private string puzzleId;
    private RectTransform rectTransform;
    private RectTransform correctPosition;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform returnParent;
    private Vector2 returnPosition;
    private bool initialized;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(string id, Sprite sprite, RectTransform target, bool isPlaced)
    {
        puzzleId = id;
        correctPosition = target;
        canvas = GetComponentInParent<Canvas>();
        Image image = GetComponent<Image>();
        image.sprite = sprite;
        image.SetNativeSize();
        initialized = true;

        if (isPlaced)
        {
            enabled = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!initialized || canvas == null) return;

        returnParent = transform.parent;
        returnPosition = rectTransform.anchoredPosition;
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!initialized || canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!initialized || canvas == null) return;
        canvasGroup.blocksRaycasts = true;

        if (correctPosition != null &&
            Vector3.Distance(rectTransform.position, correctPosition.position) <= snapDistance)
        {
            transform.SetParent(correctPosition, false);
            rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
            canvasGroup.blocksRaycasts = false;
            enabled = false;
            PuzzleManager2.Instance?.MirrorPiecePlaced(puzzleId);
            return;
        }

        transform.SetParent(returnParent, false);
        rectTransform.anchoredPosition = returnPosition;
    }
}
