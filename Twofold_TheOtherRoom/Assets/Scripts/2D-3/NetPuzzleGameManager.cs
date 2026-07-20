using System.Collections;
using UnityEngine;

public class NetPuzzleGameManager : MonoBehaviour
{
    [Header("퍼즐 설정")]
    [Tooltip("팀원과 약속한 이 퍼즐의 고유 ID (예: 2D-1, 2D-3 등)")]
    public string puzzleID = "2D-1"; 
    public PuzzleDimension dimension = PuzzleDimension.TwoD;

    [Header("검사할 전개도 슬롯들 (5개)")]
    public NetPuzzleSlot[] puzzleSlots;

    [Header("퍼즐 판 전체 (Puzzle_Net)")]
    public GameObject puzzleContainer;

    public void CheckPuzzleComplete()
    {
        StartCoroutine(DelayedCheck());
    }

    // OnDrop 처리 직후 자식 계층구조가 완료된 한 프레임 뒤 검사
    private IEnumerator DelayedCheck()
    {
        yield return null; 

        if (puzzleSlots == null || puzzleSlots.Length == 0) yield break;

        int filledCount = 0;
        bool hasWrongItem = false;

        foreach (NetPuzzleSlot slot in puzzleSlots)
        {
            NetPuzzleDragItem itemInSlot = slot.GetComponentInChildren<NetPuzzleDragItem>();

            if (itemInSlot != null)
            {
                filledCount++;
                if (itemInSlot.itemID != slot.correctItemID)
                {
                    hasWrongItem = true;
                }
            }
        }

        // 5개 슬롯이 전부 채워졌다면!
        if (filledCount == puzzleSlots.Length)
        {
            if (!hasWrongItem)
            {
                Debug.Log("🎉 축하합니다! 퍼즐 성공!");
                OnPuzzleSuccess();
            }
            else
            {
                Debug.Log("❌ 틀린 위치가 있습니다! 0.3초 뒤 원위치로 리셋됩니다.");
                StartCoroutine(ResetPuzzleWithDelay(0.3f));
            }
        }
    }

    private IEnumerator ResetPuzzleWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (NetPuzzleSlot slot in puzzleSlots)
        {
            NetPuzzleDragItem item = slot.GetComponentInChildren<NetPuzzleDragItem>();
            if (item != null)
            {
                item.ResetToOriginalPosition();
                slot.isCorrect = false;
            }
        }
    }

    private void OnPuzzleSuccess()
    {
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(puzzleID, dimension);
        }
        else
        {
            Debug.LogWarning("PuzzleManager가 씬에 배치되지 않았습니다!");
        }

        // 추가 성공 연출(소리, 이펙트 등)이 있다면 여기에 작성하시면 됩니다.
    }
}
