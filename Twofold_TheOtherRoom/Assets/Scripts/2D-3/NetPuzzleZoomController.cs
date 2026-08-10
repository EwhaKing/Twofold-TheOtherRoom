using System.Collections;
using UnityEngine;

public class NetPuzzleZoomController : MonoBehaviour
{
    [Header("확대/축소 대상 그룹")]
    public RectTransform roomBackground; // RoomBackground
    public GameObject puzzleContainer;  // Puzzle_Net

    [Header("깨지는 연출 그룹")]
    public GameObject puzzleBreakEffect; // Puzzle_BreakEffect

    [Header("방 요소들 (확대 시 숨길 것)")]
    public GameObject floorGraphics;   // Simple_Placeholder
    public GameObject rugObject;       // rug
    public GameObject frameButton;     // FrameButton

    [Header("줌 연출 설정")]
    public float zoomDuration = 0.4f;
    public float targetScale = 2.0f;
    public float targetOffsetY = -250f; 

    private Vector3 initialScale = Vector3.one;
    private Vector2 initialPos = Vector2.zero;
    private CanvasGroup puzzleCanvasGroup;
    public bool isZoomed = false;

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

        if (puzzleBreakEffect != null)
        {
            puzzleBreakEffect.SetActive(false);
        }
    }

    public void ZoomInToPuzzle()
    {
        isZoomed = true;
        StopAllCoroutines();
        StartCoroutine(ZoomRoutine(true));
    }

    public void ZoomIn() => ZoomInToPuzzle();
    public void ZoomOut() => ZoomOutToRoom();

    public void ZoomOutToRoom()
    {
        Debug.Log("👈 Back 버튼 클릭됨! 줌아웃 시작");
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

        // 클리어 여부 확인 (Puzzle_BreakEffect가 켜져 있거나 활성화된 적이 있는지)
        bool isPuzzleCleared = (puzzleBreakEffect != null && puzzleBreakEffect.activeSelf);

        if (zoomingIn)
        {
            if (floorGraphics != null) floorGraphics.SetActive(false);
            if (rugObject != null) rugObject.SetActive(false);
            if (frameButton != null) frameButton.SetActive(false);

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
            // ★ 줌아웃 시작 시: 퍼즐 클리어 연출 화면은 꺼주고, 실제 방 바닥에 깨진 상태(HoleOnFloor)를 적용합니다.
            if (isPuzzleCleared)
            {
                PuzzleBreakEffect breakScript = puzzleBreakEffect.GetComponent<PuzzleBreakEffect>();
                if (breakScript != null)
                {
                    breakScript.ApplyRoomBreakState();
                }
                puzzleBreakEffect.SetActive(false);
            }
        }

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / zoomDuration);

            roomBackground.localScale = Vector3.Lerp(startScale, endScale, t);
            roomBackground.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            if (puzzleCanvasGroup != null && !isPuzzleCleared)
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
            if (puzzleCanvasGroup != null)
            {
                puzzleCanvasGroup.alpha = 1f;
                puzzleCanvasGroup.interactable = true;
                puzzleCanvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            // 줌아웃 완료 후 퍼즐 컨테이너 비활성화
            if (puzzleContainer != null) puzzleContainer.SetActive(false);

            if (isPuzzleCleared)
            {
                // 클리어 후 방 화면: 액자 버튼 복구 및 깨진 상태 유지 (러그/원래 바닥은 숨김)
                if (frameButton != null) frameButton.SetActive(true);
            }
            else
            {
                // 클리어 전 그냥 뒤로가기: 원래 방 상태로 복구
                if (floorGraphics != null) floorGraphics.SetActive(true);
                if (frameButton != null) frameButton.SetActive(true);

                if (rugObject != null)
                {
                    rugObject.SetActive(true);
                    RugToggle rugScript = rugObject.GetComponent<RugToggle>();
                    if (rugScript != null) rugScript.UpdateRugSprite();
                }
            }
        }
    }
}