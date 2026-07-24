using UnityEngine;
using UnityEngine.InputSystem;

public class DiscPuzzleManager : MonoBehaviour
{
    public Disc[] discs; // 3개
    public Camera puzzleCamera;

    [Tooltip("이 퍼즐의 고유 id (유일해야 함)")]
    public string puzzleId = "3D-6";

    [Tooltip("퍼즐 해당 차원")]
    public PuzzleDimension dimension = PuzzleDimension.ThreeD;

    bool _solved;

    void Update()
    {
        if (_solved) return; // 회전 잠금

        // 퍼즐에 진입했을 때만 입력 처리해 탐험 중 클릭 무시
        if (puzzleCamera == null || !puzzleCamera.enabled) return;

        // Side 뷰에서만 원판 회전 가능
        if (ViewSwitcher.CurrentView != ViewSwitcher.ViewMode.Side) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame) TryRotate(1);  // 왼쪽 클릭 = 왼쪽 회전
        if (mouse.rightButton.wasPressedThisFrame) TryRotate(-1);  // 오른쪽 클릭 = 오른쪽 회전
    }

    void TryRotate(int dir)
    {
        Ray ray = puzzleCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Disc disc = hit.collider.GetComponentInParent<Disc>();
            if (disc != null)
            {
                disc.Rotate(dir);
                CheckSolved();
            }
        }
    }

    void CheckSolved()
    {
        if (discs == null || discs.Length == 0) return;

        bool solved = true;
        foreach (var d in discs)
            if (!d.IsCorrect()) solved = false;

        if (solved && !_solved)
        {
            _solved = true; // 회전 잠금
            if (PuzzleManager.Instance != null)
                PuzzleManager.Instance.ReportSolved(puzzleId, dimension);
        }
    }
}