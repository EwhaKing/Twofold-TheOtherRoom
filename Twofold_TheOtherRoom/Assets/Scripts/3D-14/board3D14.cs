using UnityEngine;
using System.Collections;

public class board3D14 : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class FigureAnswer
    {
        public figure3D14 figure;
        public FigurePositionName correctPosition;
    }
    
    [Header("Figures & Answers")]
    [SerializeField] private FigureAnswer[] figureAnswers;

    [Header("Clear Animation")]
    [SerializeField] private Transform shelf;
    [SerializeField] private float shelfMoveDistance = 1f;
    [SerializeField] private float shelfMoveDuration = 1f;

    private bool shelfMoved = false;

    private bool _isSolved = false;
    public bool IsSolved => _isSolved;

    public void Interact()
    {
        if (InspectionCameraRig.Instance == null)
        {
            Debug.LogWarning(
                "[board3D14] InspectionCameraRig가 없습니다."
            );

            return;
        }

        InspectionCameraRig.Instance.StartView();
    }

    public bool CanMoveToPosition(
        figure3D14 movingFigure,
        FigurePositionName targetPosition)

    {
        foreach (FigureAnswer answer in figureAnswers)
        {
            if (answer == null || answer.figure == null)
                continue;

            if (answer.figure == movingFigure)
                continue;

            if (answer.figure.GetCurrentPositionName() == targetPosition)
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator MoveShelf()
    {
        shelfMoved = true;

        Vector3 startPosition = shelf.localPosition;
        Vector3 targetPosition = startPosition + Vector3.forward * shelfMoveDistance;

        float elapsed = 0f;

        while (elapsed < shelfMoveDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / shelfMoveDuration;

            // 부드럽게 움직이기
            t = Mathf.SmoothStep(0f, 1f, t);

            shelf.localPosition =
                Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        shelf.localPosition = targetPosition;
    }
    public void CheckPuzzleSolved()
    {
        if (figureAnswers == null || figureAnswers.Length == 0)
            return;

        foreach (FigureAnswer answer in figureAnswers)
        {
            if (answer == null || answer.figure == null)
                return;

            if (answer.figure.GetCurrentPositionName() != answer.correctPosition)
            {
                return;
            }
        }

        Debug.Log("[FigureBoardPuzzle] ★ 퍼즐 성공 ★");
        _isSolved = true;

        if (shelf != null && !shelfMoved)
        {
            StartCoroutine(MoveShelf());
        }

        PuzzleManager.Instance.ReportSolved
        (
            "3D-14", PuzzleDimension.ThreeD
        );
    }
}
    