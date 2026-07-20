using System.Collections;
using UnityEngine;

public class NetPuzzleZoomController : MonoBehaviour
{
    [Header("확대/축소 대상 그룹")]
    public RectTransform roomBackground; // RoomBackground
    public GameObject puzzleContainer;  // Puzzle_Net

    [Header("방 요소들 (확대 시 숨길 것)")]
    public GameObject floorGraphics;   // Simple_Placeholder
    public GameObject frameButton;     // FrameButton
    public GameObject backButton;      // BackButton

    [Header("줌 연출 설정")]
    public float zoomDuration = 0.4f;   // 줌 연출 시간
    public float targetScale = 2.0f;    // 확대 배율
    
    // 💡 바닥 퍼즐(화면 아래)을 확대해서 중앙에 맞추려면 offset Y가 '마이너스(-)'여야 합니다!
    public float targetOffsetY = -250f; 

    private Vector3 initialScale = Vector3.one;
    private Vector2 initialPos = Vector2.zero;
    private CanvasGroup puzzleCanvasGroup;
    private bool isZoomed = false;

    void Awake()
    {
        if (roomBackground != null)
        {
            initialScale = roomBackground.localScale;
            initialPos = roomBackground.anchoredPosition;
        }

        if (puzzleContainer != null)
        {
            // CanvasGroup 컴포넌트 자동 가져오기
            puzzleCanvasGroup = puzzleContainer.GetComponent<CanvasGroup>();
            puzzleContainer.SetActive(false);
        }

        if (backButton != null) backButton.SetActive(false);
    }

    // 🔍 바닥 퍼즐 클릭 시 (확대)
    public void ZoomInToPuzzle()
    {
        if (isZoomed) return;
        StopAllCoroutines();
        StartCoroutine(ZoomRoutine(true));
    }

    // 🔙 뒤로가기 클릭 시 (축소)
    public void ZoomOutToRoom()
    {
        if (!isZoomed) return;
        StopAllCoroutines();
        StartCoroutine(ZoomRoutine(false));
    }

    private IEnumerator ZoomRoutine(bool zoomingIn)
    {
        float elapsed = 0f;

        Vector3 startScale = roomBackground.localScale;
        Vector3 endScale = zoomingIn ? Vector3.one * targetScale : initialScale;

        Vector2 startPos = roomBackground.anchoredPosition;
        Vector2 endPos = zoomingIn ? initialPos + new Vector2(0, targetOffsetY) : initialPos;

        // 줌 시작 시 세팅
        if (zoomingIn)
        {
            if (floorGraphics != null) floorGraphics.SetActive(false);
            if (frameButton != null) frameButton.SetActive(false);

            if (puzzleContainer != null)
            {
                puzzleContainer.SetActive(true);
                if (puzzleCanvasGroup != null)
                {
                    puzzleCanvasGroup.alpha = 0f; // 일단 투명하게 시작
                    puzzleCanvasGroup.interactable = false;
                    puzzleCanvasGroup.blocksRaycasts = false;
                }
            }
        }
        else
        {
            if (backButton != null) backButton.SetActive(false);
        }

        // 줌인/줌아웃 애니메이션
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / zoomDuration);

            roomBackground.localScale = Vector3.Lerp(startScale, endScale, t);
            roomBackground.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            // 퍼즐 Alpha 페이드 인/아웃
            if (puzzleCanvasGroup != null)
            {
                puzzleCanvasGroup.alpha = zoomingIn ? t : (1f - t);
            }

            yield return null;
        }

        roomBackground.localScale = endScale;
        roomBackground.anchoredPosition = endPos;
        isZoomed = zoomingIn;

        // 줌 완료 후 세팅
        if (zoomingIn)
        {
            if (backButton != null) backButton.SetActive(true);
            if (puzzleCanvasGroup != null)
            {
                puzzleCanvasGroup.alpha = 1f; // ★ 투명도 100% 확실히 설정
                puzzleCanvasGroup.interactable = true;
                puzzleCanvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            if (puzzleContainer != null) puzzleContainer.SetActive(false);
            if (floorGraphics != null) floorGraphics.SetActive(true);
            if (frameButton != null) frameButton.SetActive(true);
        }
    }
}