using System.Collections;
using UnityEngine;

public class NetPuzzleGameManager : MonoBehaviour
{
    [Header("퍼즐 설정")]
    [Tooltip("팀원과 약속한 이 퍼즐의 고유 ID (예: 2D-1, 2D-3 등)")]
    public string puzzleID = "2D-3"; 
    public PuzzleDimension dimension = PuzzleDimension.TwoD;

    [Header("검사할 전개도 슬롯들 (5개)")]
    public NetPuzzleSlot[] puzzleSlots;

    [Header("퍼즐 판 전체 (Puzzle_Net)")]
    public GameObject puzzleContainer;

    [Header("깨지는 연출 스크립트 연결")]
    public PuzzleBreakEffect breakEffect; // 깨지는 연출 그룹 스크립트

    // [Header("줌 컨트롤러 연결")]
    // public NetPuzzleZoomController zoomController; // 줌 컨트롤러 추가

    public void CheckPuzzleComplete()
    {
        StartCoroutine(DelayedCheck());
    }

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
        // 1. PuzzleManager에 퍼즐 해결 보고 
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(puzzleID, dimension);
        }
        else
        {
            Debug.LogWarning("PuzzleManager가 씬에 배치되지 않았습니다!");
        }

        // 2. 기존 퍼즐 슬롯(조각들) 비활성화
        if (puzzleSlots != null)
        {
            foreach (NetPuzzleSlot slot in puzzleSlots)
            {
                if (slot != null) slot.gameObject.SetActive(false);
            }
        }

        // // 3. 줌 컨트롤러에 확대 상태 전달
        // if (zoomController != null)
        // {
        //     zoomController.ZoomInToPuzzle();
        // }

        // 4. 깨지는 연출 오브젝트 활성화 및 시작
        if (breakEffect != null)
        {
            breakEffect.gameObject.SetActive(true);
            breakEffect.PrepareEffect();
        }
    }
}