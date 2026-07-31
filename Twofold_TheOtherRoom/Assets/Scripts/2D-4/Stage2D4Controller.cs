using UnityEngine;
using UnityEngine.UI;

public class Stage2D4Controller : MonoBehaviour
{
    [Header("퍼즐 패널")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private Button puzzleBackButton;

    [Header("벽에 구멍")]
    [SerializeField] private GameObject holeButton;
    [SerializeField] private Button holeButtonComponent;

    [Header("전체 화면 거울 조각")]
    [SerializeField] private GameObject mirrorDisplay;

    [Header("구멍 확대 화면")]
    [SerializeField] private GameObject holeZoomPanel;
    [SerializeField] private GameObject zoomMirrorPiece;
    [SerializeField] private Button zoomMirrorPieceButton;
    [SerializeField] private Button holeBackButton;

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
        if (puzzleBackButton != null)
        {
            puzzleBackButton.onClick.AddListener(
                ClosePuzzlePanel
            );
        }

        if (holeButtonComponent != null)
        {
            holeButtonComponent.onClick.AddListener(
                OpenHoleZoomPanel
            );
        }

        if (zoomMirrorPieceButton != null)
        {
            zoomMirrorPieceButton.onClick.AddListener(
                CollectMirrorPiece
            );
        }

        if (holeBackButton != null)
        {
            holeBackButton.onClick.AddListener(
                CloseHoleZoomPanel
            );
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
            "2D-4 퍼즐 완료: 뒤로가기를 누르면 구멍이 나타납니다."
        );
    }


    public void ClosePuzzlePanel()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

   
        if (!puzzleCleared)
        {
            return;
        }

        if (holeButton != null)
        {
            holeButton.SetActive(true);
        }

   
        if (mirrorDisplay != null)
        {
            mirrorDisplay.SetActive(!mirrorCollected);
        }
    }

  
    public void OpenHoleZoomPanel()
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


    public void CloseHoleZoomPanel()
    {
        if (holeZoomPanel != null)
        {
            holeZoomPanel.SetActive(false);
        }


        if (mirrorDisplay != null)
        {
            mirrorDisplay.SetActive(
                puzzleCleared && !mirrorCollected
            );
        }
    }

    private void OnDestroy()
    {
        if (puzzleBackButton != null)
        {
            puzzleBackButton.onClick.RemoveListener(
                ClosePuzzlePanel
            );
        }

        if (holeButtonComponent != null)
        {
            holeButtonComponent.onClick.RemoveListener(
                OpenHoleZoomPanel
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
                CloseHoleZoomPanel
            );
        }
    }
}
