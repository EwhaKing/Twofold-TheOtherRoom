using System.Collections;
using UnityEngine;

public class AcpdStageZoomController_V2 : MonoBehaviour
{
    [Header("확대/축소 대상 그룹")]
    public RectTransform roomBackground; // RoomBackground
    public GameObject puzzleContainer;  // ACPD_Puzzle_Group

    [Header("방 요소들 (확대 시 숨길 것)")]
    public GameObject wallPlaceholder; // ACPD_Wall_Placeholder
    public GameObject backButton;      // Back_Button

    [Header("게임 매니저 연결")]
    public AcpdStageGameManager_V2 gameManager; 

    [Header("줌 연출 설정")]
    public float zoomDuration = 0.4f;
    public float targetScale = 3.5f;

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
            puzzleCanvasGroup = puzzleContainer.GetComponent<CanvasGroup>();
            puzzleContainer.SetActive(false);
        }

        if (backButton != null) backButton.SetActive(false);
    }

    // 🔍 퍼즐 클릭 시 (확대)
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
        Vector2 endPos = initialPos; // ACPD는 중앙 줌

        if (zoomingIn)
        {
            if (wallPlaceholder != null) wallPlaceholder.SetActive(false);

            if (puzzleContainer != null)
            {
                puzzleContainer.SetActive(true);
                if (puzzleCanvasGroup != null)
                {
                    puzzleCanvasGroup.alpha = 0f;
                    puzzleCanvasGroup.interactable = false;
                    puzzleCanvasGroup.blocksRaycasts = false;
                }
            }
        }
        else
        {
            if (backButton != null) backButton.SetActive(false);
        }

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / zoomDuration);

            roomBackground.localScale = Vector3.Lerp(startScale, endScale, t);
            roomBackground.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            if (puzzleCanvasGroup != null)
            {
                puzzleCanvasGroup.alpha = zoomingIn ? t : (1f - t);
            }

            yield return null;
        }

        roomBackground.localScale = endScale;
        roomBackground.anchoredPosition = endPos;
        isZoomed = zoomingIn;

        if (zoomingIn)
        {
            if (backButton != null) backButton.SetActive(true);
            if (puzzleCanvasGroup != null)
            {
                puzzleCanvasGroup.alpha = 1f;
                puzzleCanvasGroup.interactable = true;
                puzzleCanvasGroup.blocksRaycasts = true;
            }

            // 🌟 줌 완료 시 V2 게임 매니저에 초기화 및 시작 알림
            if (gameManager != null)
            {
                gameManager.InitPuzzleState();
            }
        }
        else
        {
            if (puzzleContainer != null) puzzleContainer.SetActive(false);
            if (wallPlaceholder != null) wallPlaceholder.SetActive(true);
        }
    }
}