using UnityEngine;

public class PuzzleResetController : MonoBehaviour
{
    [Header("리셋할 드래그 아이템들 (Item_Sharp, Item_C 등)")]
    public NetPuzzleDragItem[] allDragItems;

    [Header("게임 매니저 연결")]
    public NetPuzzleGameManager gameManager;

    private void Update()
    {
        // 깨지는 연출(breakEffect)이 켜졌는지 감시하다가, 켜지면 리셋 버튼을 스스로 숨깁니다.
        if (gameManager != null && gameManager.breakEffect != null)
        {
            if (gameManager.breakEffect.gameObject.activeSelf)
            {
                HideResetButton();
            }
        }
    }

    public void ResetPuzzle()
    {
        // 1. 씬 내의 모든 드래그 아이템 원위치 복원 및 맨 앞으로 배치
        if (allDragItems != null)
        {
            foreach (NetPuzzleDragItem item in allDragItems)
            {
                if (item != null)
                {
                    item.ResetToOriginalPosition();
                    // 발판 뒤로 숨는 현상 방지를 위해 UI 순서를 맨 앞으로 당김
                    item.transform.SetAsLastSibling(); 
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

    // 리셋 버튼 자체를 화면에서 숨기는 기능
    public void HideResetButton()
    {
        gameObject.SetActive(false);
    }
}