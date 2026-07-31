using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlantMove : MonoBehaviour
{
    [SerializeField] private RectTransform plantRect;
    [SerializeField] private Button plantButton;
    [SerializeField] private Image plantImage;

    [SerializeField] private Button displayButton;
    [SerializeField] private GameObject puzzlePanel;

    [SerializeField] private float moveDistance = 150f;
    [SerializeField] private float moveDuration = 0.4f;

    private bool plantMoved;
    private bool isMoving;

    private void Start()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (displayButton != null)
        {
            displayButton.interactable = false;
            displayButton.onClick.AddListener(OpenPuzzle);
        }

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
            Debug.LogError("Plant Rect가 연결되지 않았습니다.");
            return;
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

        // 식물은 더 이상 클릭되지 않도록 설정
        if (plantButton != null)
        {
            plantButton.interactable = false;
        }

        // 식물 이미지가 display 클릭을 가로채지 않도록 설정
        if (plantImage != null)
        {
            plantImage.raycastTarget = false;
        }

        // 식물이 이동한 뒤에만 display 클릭 가능
        if (displayButton != null)
        {
            displayButton.interactable = true;
        }
    }

    public void OpenPuzzle()
    {
        if (!plantMoved)
        {
            return;
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }

    }

    private void OnDestroy()
    {
        if (plantButton != null)
        {
            plantButton.onClick.RemoveListener(MovePlant);
        }

        if (displayButton != null)
        {
            displayButton.onClick.RemoveListener(OpenPuzzle);
        }
    }
}