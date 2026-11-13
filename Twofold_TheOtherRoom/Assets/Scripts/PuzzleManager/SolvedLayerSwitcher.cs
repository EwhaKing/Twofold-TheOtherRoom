using UnityEngine;

/// <summary>
/// 지정한 퍼즐이 풀리면 이 오브젝트와 모든 자식 중 Interactable 레이어인 것만 Obstacle 레이어로 바꾼다.
/// 퍼즐 오브젝트들을 묶는 부모에 붙이고 puzzleId 만 맞춰주면 됨 (ReportSolved 에 넘기는 id 와 동일).
/// 다른 레이어(Ground 등)인 자식은 건드리지 않음.
/// </summary>
public class SolvedLayerSwitcher : MonoBehaviour
{
    [Tooltip("PuzzleManager.ReportSolved 에 넘기는 id 와 같아야 함. 예: 3D-2")]
    [SerializeField] private string puzzleId = "3D-2";

    [SerializeField] private string fromLayer = "Interactable";
    [SerializeField] private string toLayer = "Obstacle";

    private void OnEnable()
    {
        PuzzleManager.OnPuzzleSolved += HandlePuzzleSolved;

        // 이미 풀린 뒤에 켜진 경우 (씬 재진입, 늦게 활성화 등)
        if (PuzzleManager.Instance != null && PuzzleManager.Instance.IsSolved(puzzleId))
            SwitchLayers();
    }

    private void OnDisable()
    {
        PuzzleManager.OnPuzzleSolved -= HandlePuzzleSolved;
    }

    private void HandlePuzzleSolved(string solvedId)
    {
        if (solvedId == puzzleId) SwitchLayers();
    }

    private void SwitchLayers()
    {
        int from = LayerMask.NameToLayer(fromLayer);
        int to = LayerMask.NameToLayer(toLayer);
        if (from < 0 || to < 0)
        {
            Debug.LogWarning($"[SolvedLayerSwitcher] 레이어 이름 확인 필요: {fromLayer} → {toLayer}", this);
            return;
        }

        // 비활성 자식까지 포함
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t.gameObject.layer == from)
                t.gameObject.layer = to;
        }
    }
}
