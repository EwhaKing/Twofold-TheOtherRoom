using UnityEngine;

public class PuzzleChecker : MonoBehaviour
{
    public ColorBlockController rowA;
    public ColorBlockController rowB;
    public ColorBlockController rowC;
    public ColorBlockController rowD;

    public LetterController letter1;
    public LetterController letter2;
    public LetterController letter3;
    public LetterController letter4;

    private bool solved = false;

    public void CheckAnswer()
    {
        if (solved) return;

        bool colorCorrect =
            rowA.BlockCount == 3 &&
            rowB.BlockCount == 2 &&
            rowC.BlockCount == 4 &&
            rowD.BlockCount == 5;

        bool letterCorrect =
            letter1.CurrentLetter == 'D' &&
            letter2.CurrentLetter == 'A' &&
            letter3.CurrentLetter == 'C' &&
            letter4.CurrentLetter == 'B';

        if (colorCorrect && letterCorrect)
        {
            solved = true;

            Debug.Log("퍼즐 성공!");

            PuzzleManager.Instance.ReportSolved(
                "2D-4",
                PuzzleDimension.TwoD);
        }
    }
}