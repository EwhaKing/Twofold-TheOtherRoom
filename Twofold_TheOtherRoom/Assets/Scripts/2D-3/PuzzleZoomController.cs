using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleZoomController : MonoBehaviour
{
    [Header("확대/축소할 실제 퍼즐 판 (Puzzle_Net)")]
    public RectTransform puzzleContainer;
    private CanvasGroup puzzleCanvasGroup;

    [Header("방 전체 배경 이미지 (RoomBackground)")]
    public GameObject roomBackground; // ★ 줌인 시 끄고, 줌아웃 시 켤 방 배경!

    [Header("방에 그려진 간이 이미지 (Simple_Placeholder)")]
    public GameObject simplePlaceholder;

    [Header("뒤로가기 버튼")]
    public Button backButton;

    [Header("줌 애니메이션 설정")]
    public float zoomDuration = 0.4f;
    public Vector3 zoomedScale = Vector3.one;
    public Vector2 zoomedPosition = Vector2.zero;

    [Header("시작 위치 (간이 이미지와 똑같은 위치)")]
    public Vector3 originalScale = new Vector3(0.25f, 0.25f, 1f); 
    public Vector2 originalPosition = Vector2.zero;    

    private bool isZoomed = false;

    private void Awake()
    {
        puzzleCanvasGroup = puzzleContainer.GetComponent<CanvasGroup>();
        if (puzzleCanvasGroup == null) puzzleCanvasGroup = puzzleContainer.gameObject.AddComponent<CanvasGroup>();
        
        puzzleCanvasGroup.alpha = 0;
        puzzleCanvasGroup.interactable = false;
        puzzleCanvasGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ZoomOut);
            backButton.gameObject.SetActive(false);
        }

        puzzleContainer.localScale = originalScale;
        puzzleContainer.anchoredPosition = originalPosition;
    }

    // [확대 시작]
    public void ZoomIn()
    {
        if (isZoomed) return;
        isZoomed = true;

        // 1. 간이 이미지 끄기
        if (simplePlaceholder != null) simplePlaceholder.SetActive(false);

        // 2. 방 전체 배경 끄기! (카메라 회색 바닥이 드러남)
        if (roomBackground != null) roomBackground.SetActive(false);
        
        // 3. 실제 퍼즐 보이기
        puzzleCanvasGroup.alpha = 1;
        puzzleCanvasGroup.interactable = true;
        puzzleCanvasGroup.blocksRaycasts = true;

        StopAllCoroutines();
        StartCoroutine(AnimateZoom(zoomedScale, zoomedPosition, true));
    }

    // [축소 시작 (뒤로가기)]
    public void ZoomOut()
    {
        if (!isZoomed) return;
        isZoomed = false;

        StopAllCoroutines();
        StartCoroutine(AnimateZoom(originalScale, originalPosition, false));
    }

    private IEnumerator AnimateZoom(Vector3 targetScale, Vector2 targetPos, bool isZoomingIn)
    {
        Vector3 startScale = puzzleContainer.localScale;
        Vector2 startPos = puzzleContainer.anchoredPosition;
        float elapsed = 0f;

        if (backButton != null) backButton.gameObject.SetActive(false);

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            t = t * t * (3f - 2f * t);

            puzzleContainer.localScale = Vector3.Lerp(startScale, targetScale, t);
            puzzleContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        puzzleContainer.localScale = targetScale;
        puzzleContainer.anchoredPosition = targetPos;

        if (isZoomingIn)
        {
            if (backButton != null) backButton.gameObject.SetActive(true);
        }
        else
        {
            // 줌아웃이 완료되면 방 배경과 간이 이미지 다시 켜기!
            if (roomBackground != null) roomBackground.SetActive(true);
            if (simplePlaceholder != null) simplePlaceholder.SetActive(true);

            puzzleCanvasGroup.alpha = 0;
            puzzleCanvasGroup.interactable = false;
            puzzleCanvasGroup.blocksRaycasts = false;
        }
    }
}
