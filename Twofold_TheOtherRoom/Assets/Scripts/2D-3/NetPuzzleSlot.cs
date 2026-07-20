using UnityEngine;
using UnityEngine.EventSystems;

public class NetPuzzleSlot : MonoBehaviour, IDropHandler
{
    [Header("이 자리에 들어와야 하는 정답 아이템 ID")]
    public int correctItemID;

    [HideInInspector] public bool isCorrect = false;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        NetPuzzleDragItem dragItem = droppedObj.GetComponent<NetPuzzleDragItem>();
        if (dragItem != null)
        {
            // [핵심 해결] 드래그가 끝난 뒤 이 슬롯을 새 부모로 지정!
            dragItem.parentAfterDrag = transform;

            // 정답 여부 체크
            isCorrect = (dragItem.itemID == correctItemID);

            // 전체 정답 상태 체크 요청
            NetPuzzleGameManager manager = FindAnyObjectByType<NetPuzzleGameManager>();
            if (manager != null)
            {
                // 드롭 직후 자식 연결 타이밍을 위해 매니저 검사 호출
                manager.CheckPuzzleComplete();
            }
        }
    }
}
