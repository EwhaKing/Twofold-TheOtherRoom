using UnityEngine;

public class PuzzleChecker : MonoBehaviour
{
    [Header("색 네모칸")]
    [SerializeField] private ColorBlockController rowA;
    [SerializeField] private ColorBlockController rowB;
    [SerializeField] private ColorBlockController rowC;
    [SerializeField] private ColorBlockController rowD;

    [Header("문자 입력")]
    [SerializeField] private LetterController letter1;
    [SerializeField] private LetterController letter2;
    [SerializeField] private LetterController letter3;
    [SerializeField] private LetterController letter4;

    [Header("스테이지 진행 관리")]
    [SerializeField] private Stage2D4Controller stageController;

    private bool solved;

    public void CheckAnswer()
    {
        if (solved)
        {
            return;
        }

        if (!ReferencesAreValid())
        {
            return;
        }

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

        if (!colorCorrect || !letterCorrect)
        {
            return;
        }

        solved = true;

        Debug.Log("2D-4 퍼즐 성공");

        if (stageController != null)
        {
            stageController.NotifyPuzzleCleared();
        }
        else
        {
            Debug.LogWarning(
                "PuzzleChecker: Stage2D4Controller가 연결되지 않았습니다."
            );
        }

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(
                "2D-4",
                PuzzleDimension.TwoD
            );
        }
        else
        {
            Debug.LogWarning(
                "PuzzleChecker: PuzzleManager를 찾을 수 없습니다."
            );
        }
    }

    private bool ReferencesAreValid()
    {
        if (rowA == null ||
            rowB == null ||
            rowC == null ||
            rowD == null)
        {
            Debug.LogError(
                "PuzzleChecker: ColorBlockController 연결을 확인하세요."
            );

            return false;
        }

        if (letter1 == null ||
            letter2 == null ||
            letter3 == null ||
            letter4 == null)
        {
            Debug.LogError(
                "PuzzleChecker: LetterController 연결을 확인하세요."
            );

            return false;
        }

        return true;
    }
}