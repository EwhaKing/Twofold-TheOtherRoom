using UnityEngine;

public class Answer : MonoBehaviour
{
    public NumberSlot[] slots;

    void CheckAnswer()
    {
        if (slots[0].GetNumber() == 5 &&
            slots[1].GetNumber() == 7 &&
            slots[2].GetNumber() == 9 &&
            slots[3].GetNumber() == 3)
        {
            PuzzleManager.Instance.ReportSolved(
                "2D-8",
                PuzzleDimension.TwoD
            );
        }
    }
}
