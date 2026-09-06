using System.Collections;
using UnityEngine;

public class MapDotController : MonoBehaviour
{
    [Header("이동 지점")]
    [SerializeField] private RectTransform[] stopPoints;

    [Header("이동 설정")]
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private int startIndex = 2;

    private RectTransform playerDot;
    private int currentIndex;
    private bool isMoving;

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
    }

    private void Update()
    {
        if (isMoving)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveRight();
        }
    }

    public void MoveLeft()
    {
        if (isMoving || currentIndex <= 0)
        {
            return;
        }

        currentIndex--;
        StartCoroutine(MoveSmoothly(currentIndex));
    }

    public void MoveRight()
    {
        if (isMoving || currentIndex >= stopPoints.Length - 1)
        {
            return;
        }

        currentIndex++;
        StartCoroutine(MoveSmoothly(currentIndex));
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
}