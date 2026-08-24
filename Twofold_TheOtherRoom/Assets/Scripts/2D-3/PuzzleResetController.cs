using UnityEngine;

public class PuzzleResetController : MonoBehaviour
{
    [Header("리셋할 드래그 아이템들 (Item_Sharp, Item_C 등)")]
    public NetPuzzleDragItem[] allDragItems;

    [Header("게임 매니저 연결")]
    public NetPuzzleGameManager gameManager;

    public void ResetPuzzle()
    {
        // 1. 씬 내의 모든 드래그 아이템 원위치 복원
        if (allDragItems != null)
        {
            foreach (NetPuzzleDragItem item in allDragItems)
            {
                if (item != null)
                {
                    item.ResetToOriginalPosition();
                }
            }
        }

        // 2. 슬롯의 정답 플래그 초기화
        if (gameManager != null && gameManager.puzzleSlots != null)
        {
            foreach (NetPuzzleSlot slot in gameManager.puzzleSlots)
            {
                if (slot != null)
                {
                    slot.isCorrect = false;
                }
            }
        }

        Debug.Log("모든 퍼즐 아이템이 원위치로 리셋되었습니다.");
    }
}