using UnityEngine;

public class Disc : MonoBehaviour
{
    public GameObject[] wedges = new GameObject[6];
    public int targetSlot;
    public int currentSlot = 0;

    const float STEP = 60f;

    public void ApplyHolePattern(int[] openSlots)
    {
        for (int i = 0; i < 6; i++)
        {
            bool isHole = System.Array.Exists(openSlots, s => s == i);
            wedges[i].SetActive(!isHole);
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