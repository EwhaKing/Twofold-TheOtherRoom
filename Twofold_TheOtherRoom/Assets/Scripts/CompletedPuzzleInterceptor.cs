using UnityEngine;

public class CompletedPuzzleInterceptor : MonoBehaviour
{
    [Header("해당 오브젝트 퍼즐 ID")]
    public string puzzleID = "2D-10";

    private void OnMouseDown()
    {
        // 퍼즐이 완료되었는지 확인
        if (PuzzleManager.Instance != null && PuzzleManager.Instance.IsSolved(puzzleID))
        {
            // 기본값("이미 완료한 퍼즐입니다")으로 자막 연출 호출
            if (CompletedNoticeManager.Instance != null)
            {
                CompletedNoticeManager.Instance.ShowNotice();
            }
        }
    }
}
