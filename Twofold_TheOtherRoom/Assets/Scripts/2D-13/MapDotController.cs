using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapDotController : MonoBehaviour
{
    [Header("이동 지점")]
    [SerializeField] private RectTransform[] stopPoints;

    [Header("이동 설정")]
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private int startIndex = 2;

    [Header("이동 버튼")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("정답")]
    [SerializeField] int answerIndex = 4;

    private RectTransform playerDot;
    private int currentIndex;
    private bool isMoving;

    private Puzzle13 puzzle;
    private bool lastReported;
    private bool cleared;

    public int CurrentIndex => currentIndex;

    private void Awake()
    {
        playerDot = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (playerDot == null)
        {
            Debug.LogError("PlayerDot에 RectTransform이 없습니다.");
            enabled = false;
            return;
        }

        if (stopPoints == null || stopPoints.Length == 0)
        {
            Debug.LogError("Stop Points가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        for (int i = 0; i < stopPoints.Length; i++)
        {
            if (stopPoints[i] == null)
            {
                Debug.LogError($"Stop Points의 Element {i}가 비어 있습니다.");
                enabled = false;
                return;
            }
        }

        currentIndex = Mathf.Clamp(
            startIndex,
            0,
            stopPoints.Length - 1
        );

        MoveImmediately(currentIndex);
        RefreshButtons();
    }

    private void Update()
    {
        if (cleared) return;

        Puzzle13 found = ResolvePuzzle();
        if (found == null) return;

        ReportPosition(found);

        if (!found.Solved) return;

        CompletePuzzle();
    }

    private void CompletePuzzle()
    {
        cleared = true;
        RefreshButtons();

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(Puzzle13.Id2D, PuzzleDimension.TwoD);
        }

        Debug.Log($"[{nameof(MapDotController)}] 좌표 일치. 2D-13 클리어");
    }

    public void MoveLeft()
    {
        if (cleared || isMoving || currentIndex <= 0)
        {
            return;
        }

        currentIndex--;
        RefreshButtons();
        StartCoroutine(MoveSmoothly(currentIndex));
    }

    public void MoveRight()
    {
        if (cleared || isMoving || currentIndex >= stopPoints.Length - 1)
        {
            return;
        }

        currentIndex++;
        RefreshButtons();
        StartCoroutine(MoveSmoothly(currentIndex));
    }

    private void RefreshButtons()
    {
        if (leftButton != null)
            leftButton.interactable = !cleared && currentIndex > 0;

        if (rightButton != null)
            rightButton.interactable = !cleared && currentIndex < stopPoints.Length - 1;
    }

    private IEnumerator MoveSmoothly(int targetIndex)
    {
        isMoving = true;

        Vector2 startPosition = playerDot.anchoredPosition;

        Vector2 targetPosition = new Vector2(
            stopPoints[targetIndex].anchoredPosition.x,
            startPosition.y
        );

        Debug.Log(
            $"이동: {startPosition.x} → {targetPosition.x}"
        );

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsedTime / moveDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            playerDot.anchoredPosition = Vector2.Lerp(
                startPosition,
                targetPosition,
                t
            );

            yield return null;
        }

        playerDot.anchoredPosition = targetPosition;
        isMoving = false;
    }

    private void MoveImmediately(int index)
    {
        Vector2 position = playerDot.anchoredPosition;

        position.x = stopPoints[index].anchoredPosition.x;

        playerDot.anchoredPosition = position;
    }

    #region 연동 퍼즐 보고

    private void ReportPosition(Puzzle13 found)
    {
        bool match = currentIndex == answerIndex;
        if (match == lastReported) return;

        lastReported = match;
        found.ReportMatch(PuzzleDimension.TwoD, match);
    }

    private Puzzle13 ResolvePuzzle()
    {
        if (puzzle == null)
            puzzle = CoopPuzzle.Find<Puzzle13>(Puzzle13.Key);

        return puzzle;
    }

    #endregion
}
