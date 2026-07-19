using UnityEngine;

public class Answer : MonoBehaviour
{
    public NumberSlot[] slots;

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
    }
}
