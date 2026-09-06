using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlantMove : MonoBehaviour
{
    [Header("식물")]
    [SerializeField] private RectTransform plantRect;
    [SerializeField] private Button plantButton;
    [SerializeField] private Image plantImage;


    [Header("식물 이동 설정")]
    [SerializeField] private float moveDistance = 150f;
    [SerializeField] private float moveDuration = 0.4f;


     [SerializeField] private GameObject holeButton;


    private bool plantMoved;
    private bool isMoving;
    private bool puzzleCleared;

    public bool PuzzleCleared => puzzleCleared;

    private void Start()
    {
        RegisterButtonEvents();
    }


    private void RegisterButtonEvents()
    {
        if (plantButton != null)
        {
            plantButton.onClick.AddListener(MovePlant);
        }

    }

    public void MovePlant()
    {
        if (plantMoved || isMoving)
        {
            return;
        }

        if (plantRect == null)
        {
            Debug.LogError(
                "PlantMove: Plant Rect가 연결되지 않았습니다."
            );

            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Scrape);
        }

        StartCoroutine(MovePlantRight());
    }

    private IEnumerator MovePlantRight()
    {
        isMoving = true;

        Vector2 startPosition = plantRect.anchoredPosition;

        Vector2 targetPosition = new Vector2(
            startPosition.x + moveDistance,
            startPosition.y
        );

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / moveDuration
            );

            float smoothProgress = Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

            plantRect.anchoredPosition = Vector2.Lerp(
                startPosition,
                targetPosition,
                smoothProgress
            );

            yield return null;
        }

        plantRect.anchoredPosition = targetPosition;

        plantMoved = true;
        isMoving = false;

        if (plantButton != null)
        {
            plantButton.interactable = false;
        }

        if (plantImage != null)
        {
            plantImage.raycastTarget = false;
        }

    }

    public void NotifyPuzzleCleared()
    {
        if (puzzleCleared)
        {
            return;
        }

        puzzleCleared = true;

        if (holeButton != null)
        {
            holeButton.SetActive(true);
        }
    }
}