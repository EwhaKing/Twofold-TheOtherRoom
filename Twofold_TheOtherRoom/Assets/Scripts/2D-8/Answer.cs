using UnityEngine;

public class Answer : MonoBehaviour
{
    public NumberSlot[] slots;

<<<<<<< HEAD
    public int[] answer = { 3, 5, 1, 8 };

    public void CheckAnswer()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetNumber() != answer[i])
                return;
        }

        Debug.Log("퍼즐 성공!");

        PuzzleManager.Instance.ReportSolved(
            "2D-1",
            PuzzleDimension.TwoD
        );
=======
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
>>>>>>> origin/LAY/2D-8
    }
}
