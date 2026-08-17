using System;
using UnityEngine;

public class PuzzleSolvedObjectToggle : MonoBehaviour
{
    [SerializeField] private string puzzleId;
    [SerializeField] private GameObject activateObject;
    [SerializeField] private GameObject deactivateObject;

    private void OnEnable()
    {
        PuzzleManager.OnPuzzleSolved += HandlePuzzleSolved;
        ApplyIfAlreadySolved();
    }

    private void Start()
    {
        // PuzzleManager와의 Awake 실행 순서가 정해져 있지 않은 경우를 대비합니다.
        ApplyIfAlreadySolved();
    }

    private void OnDisable()
    {
        PuzzleManager.OnPuzzleSolved -= HandlePuzzleSolved;
    }

    private void HandlePuzzleSolved(string solvedPuzzleId)
    {
        if (!string.Equals(
                solvedPuzzleId?.Trim(),
                puzzleId?.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ApplySolvedState();
    }

    private void ApplyIfAlreadySolved()
    {
        if (PuzzleManager.Instance == null || string.IsNullOrWhiteSpace(puzzleId))
            return;

        if (PuzzleManager.Instance.IsSolved(puzzleId.Trim()))
            ApplySolvedState();
    }

    private void ApplySolvedState()
    {
        if (deactivateObject != null)
            deactivateObject.SetActive(false);

        if (activateObject != null)
            activateObject.SetActive(true);
    }
}
