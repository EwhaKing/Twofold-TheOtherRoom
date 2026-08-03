using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlantMove : MonoBehaviour
{
    [Header("식물")]
    [SerializeField] private RectTransform plantRect;
    [SerializeField] private Button plantButton;
    [SerializeField] private Image plantImage;

    [Header("퍼즐 화면")]
    [SerializeField] private Button displayButton;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private Button puzzleBackButton;

    [Header("식물 이동 설정")]
    [SerializeField] private float moveDistance = 150f;
    [SerializeField] private float moveDuration = 0.4f;

    [Header("벽에 구멍")]
    [SerializeField] private GameObject holeButton;
    [SerializeField] private Button holeButtonComponent;
    [SerializeField] private GameObject mirrorDisplay;

    [Header("벽 구멍 확대 화면")]
    [SerializeField] private GameObject holeZoomPanel;
    [SerializeField] private GameObject zoomMirrorPiece;
    [SerializeField] private Button zoomMirrorPieceButton;
    [SerializeField] private Button holeBackButton;

    private bool plantMoved;
    private bool isMoving;
    private bool puzzleCleared;
    private bool mirrorCollected;

    public bool PuzzleCleared => puzzleCleared;
    public bool MirrorCollected => mirrorCollected;

    private void Start()
    {
        SetInitialState();
        RegisterButtonEvents();
    }

    private void SetInitialState()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (displayButton != null)
        {
            displayButton.interactable = false;
        }

        if (holeButton != null)
        {
            holeButton.SetActive(false);
        }

        if (mirrorDisplay != null)
        {
            mirrorDisplay.SetActive(false);
        }

        if (holeZoomPanel != null)
        {
            holeZoomPanel.SetActive(false);
        }
    }

    private void RegisterButtonEvents()
    {
        if (plantButton != null)
        {
            plantButton.onClick.AddListener(MovePlant);
        }

        if (displayButton != null)
        {
            displayButton.onClick.AddListener(OpenPuzzle);
        }

        if (puzzleBackButton != null)
        {
            puzzleBackButton.onClick.AddListener(ClosePuzzle);
        }

        if (holeButtonComponent != null)
        {
            holeButtonComponent.onClick.AddListener(OpenHoleZoom);
        }

        if (zoomMirrorPieceButton != null)
        {
            zoomMirrorPieceButton.onClick.AddListener(
                CollectMirrorPiece
            );
        }

        if (holeBackButton != null)
        {
            holeBackButton.onClick.AddListener(CloseHoleZoom);
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

        // 이동한 식물이 다른 UI 클릭을 방해하지 않도록 설정
        if (plantImage != null)
        {
            plantImage.raycastTarget = false;
        }

        // 식물이 이동한 뒤 Display 클릭 허용
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

        // 퍼즐을 완료한 뒤에는 다시 열지 않도록 처리
        if (puzzleCleared)
        {
            return;
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }
    }

    public void ClosePuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        // 정답을 풀지 않았다면 Display를 다시 눌러
        // PuzzlePanel을 열 수 있도록 그대로 유지
        if (!puzzleCleared)
        {
            return;
        }

        // 퍼즐 완료 후 뒤로가기를 눌렀을 때 벽에 구멍 생김
        if (holeButton != null)
        {
            holeButton.SetActive(true);
        }

        if (mirrorDisplay != null)
        {
            mirrorDisplay.SetActive(!mirrorCollected);
        }

        // 퍼즐 완료 후에는 Display를 다시 누를 필요 없음
        if (displayButton != null)
        {
            displayButton.interactable = false;
        }
    }

    public void NotifyPuzzleCleared()
    {
        if (puzzleCleared)
        {
            return;
        }

        puzzleCleared = true;

        Debug.Log(
            "2D-4 퍼즐 완료"
        );
    }

    public void OpenHoleZoom()
    {
        if (!puzzleCleared)
        {
            return;
        }

        if (holeZoomPanel != null)
        {
            holeZoomPanel.SetActive(true);
        }

        if (zoomMirrorPiece != null)
        {
            zoomMirrorPiece.SetActive(!mirrorCollected);
        }
    }

    public void CollectMirrorPiece()
    {
        if (mirrorCollected)
        {
            return;
        }

        mirrorCollected = true;

        if (zoomMirrorPiece != null)
        {
            zoomMirrorPiece.SetActive(false);
        }

        if (mirrorDisplay != null)
        {
            mirrorDisplay.SetActive(false);
        }

        Debug.Log("거울 조각을 획득했습니다.");

    }

    public void CloseHoleZoom()
    {
        if (holeZoomPanel != null)
        {
            holeZoomPanel.SetActive(false);
        }

        // 거울을 획득했다면 전체 화면에서도 다시 나타나지 않음
        if (mirrorDisplay != null)
        {
            mirrorDisplay.SetActive(
                puzzleCleared && !mirrorCollected
            );
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

        if (puzzleBackButton != null)
        {
            puzzleBackButton.onClick.RemoveListener(ClosePuzzle);
        }

        if (holeButtonComponent != null)
        {
            holeButtonComponent.onClick.RemoveListener(
                OpenHoleZoom
            );
        }

        if (zoomMirrorPieceButton != null)
        {
            zoomMirrorPieceButton.onClick.RemoveListener(
                CollectMirrorPiece
            );
        }

        if (holeBackButton != null)
        {
            holeBackButton.onClick.RemoveListener(
                CloseHoleZoom
            );
        }
    }
}