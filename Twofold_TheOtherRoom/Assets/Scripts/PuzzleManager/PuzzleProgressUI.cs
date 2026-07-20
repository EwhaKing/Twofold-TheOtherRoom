using UnityEngine;
using TMPro;

/// <summary>
/// 디버그용 진행도 UI 표시
/// 인스펙터에서 차원(2D/3D)을 고르면 해당 차원의 [푼 퍼즐 수 / 전체]를 텍스트로 보여줌
/// PuzzleManager.OnProgressChanged를 구독해 값이 바뀔 때 자동 갱신
/// </summary>
public class PuzzleProgressUI : MonoBehaviour
{
    [Tooltip("차원")]
    public PuzzleDimension dimension = PuzzleDimension.TwoD;

    [Tooltip("진행도 출력 텍스트")]
    public TMP_Text text;

    [Tooltip("차원 라벨 표시 여부")]
    public bool showLabel = true;

    void OnEnable()
    {
        PuzzleManager.OnProgressChanged += OnProgressChanged;
        Refresh();
    }

    void Start()
    {
        Refresh();
    }

    void OnDisable()
    {
        PuzzleManager.OnProgressChanged -= OnProgressChanged;
    }

    void OnProgressChanged(PuzzleDimension changed, int solved, int total)
    {
        if (changed == dimension) Refresh();
    }

    void Refresh()
    {
        if (text == null) return;

        int solved = 0, total = 0;
        if (PuzzleManager.Instance != null)
        {
            solved = PuzzleManager.Instance.SolvedCountOf(dimension);
            total = PuzzleManager.Instance.TotalOf(dimension);
        }

        string label = showLabel ? (dimension == PuzzleDimension.TwoD ? "2D " : "3D ") : "";
        text.text = $"{label}{solved} / {total}";
    }

#if UNITY_EDITOR
    // 플레이 중 인스펙터에서 차원/옵션을 바꾸면 바로 반영
    void OnValidate()
    {
        if (Application.isPlaying) Refresh();
    }
#endif
}
