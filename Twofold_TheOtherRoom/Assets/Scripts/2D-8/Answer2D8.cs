using UnityEngine;

public class Answer2D8 : MonoBehaviour
{
    public NumberSlot2D8[] slots;

    public int[] answer = { 5, 7, 9, 3 };
    public bool _solved = false;

    public void CheckAnswer()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetNumber() != answer[i])
                return;
        }

        Debug.Log("퍼즐 성공");
        _solved =true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.CompleteCorrectBtn);
        }

        PuzzleManager.Instance.ReportSolved(
            "2D-8",
            PuzzleDimension.TwoD
        );
    }
    public bool GetSolve()
    {
        return _solved;
    }
}
