using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlantMove : MonoBehaviour
{
    [Header("식물")]
    [SerializeField] private RectTransform plantRect;
    [SerializeField] private Button plantButton;
    [SerializeField] private Image plantImage;

    // [Header("퍼즐 화면")]
    // [SerializeField] private Button displayButton;
    // [SerializeField] private GameObject puzzlePanel;
    // [SerializeField] private Button puzzleBackButton;

    [Header("식물 이동 설정")]
    [SerializeField] private float moveDistance = 150f;
    [SerializeField] private float moveDuration = 0.4f;

    // [Header("벽 구멍")]
     [SerializeField] private GameObject holeButton;
    // [SerializeField] private Button holeButtonComponent;

    // [Header("벽 구멍 확대 화면")]
    // [SerializeField] private GameObject holeZoomPanel;
    // [SerializeField] private Button holeBackButton;

    private bool plantMoved;
    private bool isMoving;
    private bool puzzleCleared;

    public bool PuzzleCleared => puzzleCleared;

    private void Start()
    {
        //SetInitialState();
        RegisterButtonEvents();
    }

    // private void SetInitialState()
    // {
    //     if (puzzlePanel != null)
    //     {
    //         puzzlePanel.SetActive(false);
    //     }

    //     if (displayButton != null)
    //     {
    //         displayButton.interactable = false;
    //     }

    //     if (holeButton != null)
    //     {
    //         holeButton.SetActive(false);
    //     }

    //     if (holeZoomPanel != null)
    //     {
    //         holeZoomPanel.SetActive(false);
    //     }
    // }

    private void RegisterButtonEvents()
    {
        if (plantButton != null)
        {
            plantButton.onClick.AddListener(MovePlant);
        }

        // if (displayButton != null)
        // {
        //     displayButton.onClick.AddListener(OpenPuzzle);
        // }

        // if (puzzleBackButton != null)
        // {
        //     puzzleBackButton.onClick.AddListener(ClosePuzzle);
        // }

        // if (holeButtonComponent != null)
        // {
        //     holeButtonComponent.onClick.AddListener(OpenHoleZoom);
        // }

        // if (holeBackButton != null)
        // {
        //     holeBackButton.onClick.AddListener(CloseHoleZoom);
        // }
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

        // if (displayButton != null)
        // {
        //     displayButton.interactable = true;
        // }
    }

    // public void OpenPuzzle()
    // {
    //     if (!plantMoved)
    //     {
    //         return;
    //     }

    //     if (puzzleCleared)
    //     {
    //         return;
    //     }

    //     if (puzzlePanel != null)
    //     {
    //         puzzlePanel.SetActive(true);
    //     }
    // }

    // public void ClosePuzzle()
    // {
    //     if (puzzlePanel != null)
    //     {
    //         puzzlePanel.SetActive(false);
    //     }

    //     if (!puzzleCleared)
    //     {
    //         return;
    //     }

    //     if (holeButton != null)
    //     {
    //         holeButton.SetActive(true);
    //     }

    //     if (displayButton != null)
    //     {
    //         displayButton.interactable = false;
    //     }
    // }

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

    // public void OpenHoleZoom()
    // {
    //     if (!puzzleCleared)
    //     {
    //         return;
    //     }

        // if (holeZoomPanel != null)
        // {
        //     holeZoomPanel.SetActive(true);
        // }
    // }

    // public void CloseHoleZoom()
    // {
    //     if (holeZoomPanel != null)
    //     {
    //         holeZoomPanel.SetActive(false);
    //     }
    // }

    // private void OnDestroy()
    // {
    //     if (plantButton != null)
    //     {
    //         plantButton.onClick.RemoveListener(MovePlant);
    //     }

    //     if (displayButton != null)
    //     {
    //         displayButton.onClick.RemoveListener(OpenPuzzle);
    //     }

    //     if (puzzleBackButton != null)
    //     {
    //         puzzleBackButton.onClick.RemoveListener(ClosePuzzle);
    //     }

    //     if (holeButtonComponent != null)
    //     {
    //         holeButtonComponent.onClick.RemoveListener(OpenHoleZoom);
    //     }

    //     if (holeBackButton != null)
    //     {
    //         holeBackButton.onClick.RemoveListener(CloseHoleZoom);
    //     }
    // }
}