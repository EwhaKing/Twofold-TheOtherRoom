using UnityEngine;

public class DisplayMirrorPiece : MonoBehaviour
{
    [SerializeField] private string puzzleId;

    // OnEnable()의 HasMirrorPiece() 확인: 이미 전에 획득한 상태 처리
    // HandleObtained(): 현재 화면에서 새로 획득하는 순간 처리

    private void OnEnable()
    {
        PuzzleManager2.OnMirrorPieceObtained += HandleObtained;

        if (PuzzleManager2.Instance != null &&
            PuzzleManager2.Instance.HasMirrorPiece(puzzleId))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        PuzzleManager2.OnMirrorPieceObtained -= HandleObtained;
    }

    private void HandleObtained(string obtainedId)
    {
        if (string.Equals(
            obtainedId.Trim(),
            puzzleId.Trim(),
            System.StringComparison.OrdinalIgnoreCase))
        {
            gameObject.SetActive(false);
        }
    }
}
