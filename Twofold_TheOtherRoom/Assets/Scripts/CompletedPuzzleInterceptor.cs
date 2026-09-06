using UnityEngine;
using UnityEngine.EventSystems;

public class CompletedPuzzleInterceptor : MonoBehaviour, IPointerClickHandler
{
    [Header("해당 오브젝트 퍼즐 ID")]
    public string puzzleID = "2D-10";

    [SerializeField] private DetailView detailView;

    private void Awake()
    {
        if (detailView == null)
        {
            detailView = GetComponent<DetailView>();
        }
    }

    private void OnEnable()
    {
        PuzzleManager.OnPuzzleSolved += HandlePuzzleSolved;
    }

    private void Start()
    {
        if (PuzzleManager.Instance != null &&
            PuzzleManager.Instance.IsSolved(puzzleID))
        {
            DisableDetailView();
        }
    }

    private void OnDisable()
    {
        PuzzleManager.OnPuzzleSolved -= HandlePuzzleSolved;
    }

    private void HandlePuzzleSolved(string solvedPuzzleID)
    {
        if (solvedPuzzleID == puzzleID)
        {
            DisableDetailView();
        }
    }

    private void DisableDetailView()
    {
        if (detailView != null)
        {
            detailView.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 퍼즐이 완료되었는지 확인
        if (PuzzleManager.Instance != null && PuzzleManager.Instance.IsSolved(puzzleID))
        {
            DisableDetailView();
            // 기본값("이미 완료한 퍼즐입니다")으로 자막 연출 호출
            if (CompletedNoticeManager.Instance != null)
            {
                CompletedNoticeManager.Instance.ShowNotice();
            }
        }
    }
}
