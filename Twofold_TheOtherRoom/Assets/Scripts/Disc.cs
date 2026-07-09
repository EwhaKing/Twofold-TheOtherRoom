using UnityEngine;

public class Disc : MonoBehaviour
{
    public GameObject[] wedges = new GameObject[6];
    public int targetSlot;
    public int currentSlot = 0;

    const float STEP = 60f;
    // 마커는 Disc의 자식으로 로컬 0°(반경 r)에 배치 → 원판 회전 시 자동으로 따라 돎.
    // wedge 배열엔 포함하지 않으므로 ApplyHolePattern이 마커를 건드리지 않음.

    public void ApplyHolePattern(int[] openSlots)
    {
        for (int i = 0; i < 6; i++)
        {
            bool isHole = System.Array.Exists(openSlots, s => s == i);
            wedges[i].SetActive(!isHole);   // 마커는 독립 자식이라 안전
        }
    }

    public void Rotate(int dir)
    {
        currentSlot = (currentSlot + dir + 6) % 6;
        ApplyRotation();
    }

    public void SetSlot(int slot)
    {
        currentSlot = ((slot % 6) + 6) % 6;
        ApplyRotation();
    }

    public void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, currentSlot * STEP, 0f);
    }

    public void ResetDisc()
    {
        currentSlot = 0;
        ApplyRotation();
        for (int i = 0; i < 6; i++)
            wedges[i].SetActive(true);
    }

    public bool IsCorrect() => currentSlot == targetSlot;
}