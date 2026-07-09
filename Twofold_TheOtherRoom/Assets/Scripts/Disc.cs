using UnityEngine;

public class Disc : MonoBehaviour
{
    public GameObject[] wedges = new GameObject[6]; // wedge 순서 주의
    public int targetSlot;
    public int currentSlot = 0;

    const float STEP = 60f;

    public void ApplyHolePattern(int[] openSlots)
    {
        for (int i = 0; i < 6; i++)
            wedges[i].SetActive(!System.Array.Exists(openSlots, s => s == i));
    }

    public void Rotate(int dir) // dir: -1 왼쪽, +1 오른쪽
    {
        currentSlot = (currentSlot + dir + 6) % 6;
        transform.localRotation = Quaternion.Euler(0, 0, currentSlot * STEP);
    }

    public bool IsCorrect() => currentSlot == targetSlot;

    void OnMouseDown() {} // 클릭 감지는 PuzzleManager에서 처리
}