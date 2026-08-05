using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>퍼즐이 속한 차원</summary>
public enum PuzzleDimension { TwoD, ThreeD }

/// <summary>
/// 맵 전체 퍼즐의 성공 상태를 통합 관리하는 싱글톤
///
/// ─────────────────────────────────────────────────────────
/// [퍼즐 쪽] 성공했으면 매니저에게 보고 (public API 호출)
/// ─────────────────────────────────────────────────────────
///   PuzzleManager.Instance.ReportSolved(id, dim);
///   → 퍼즐이 풀린 순간 이 한 줄만 호출하면 됨. 중복 호출 무시.
///     id: "3D-6", "2D-1" dim: PuzzleDimension.TwoD, PuzzleDimension.ThreeD
///
/// ─────────────────────────────────────────────────────────────────────
/// [듣는 쪽] 퍼즐 상태 변화에 반응하고 싶으면 이벤트를 구독 (UI, 문, 소리 등)
/// ─────────────────────────────────────────────────────────────────────
///   구독 = 이 이벤트 발생하면 등록한 메서드 자동으로 호출
///   OnEnable에서 += 로 등록하고, OnDisable에서 -= 로 해제 (짝을 꼭 맞출 것)
///
///   • OnProgressChanged(dim, solved, total) — 진행도가 바뀔 때마다 호출. 차원별 진행도 표시용. (아래는 사용 예시)
///       void OnEnable()  => PuzzleManager.OnProgressChanged += UpdateUI;
///       void OnDisable() => PuzzleManager.OnProgressChanged -= UpdateUI;
///       void UpdateUI(PuzzleDimension dim, int solved, int total) {
///           if (dim == PuzzleDimension.TwoD) text2D.text = $"{solved} / {total}";
///           else                             text3D.text = $"{solved} / {total}";
///       }
///
///   • OnPuzzleSolved(id) — 어떤 퍼즐이 처음 풀렸을 때 그 id로 호출. 문/서랍 열기 등에 사용. (아래는 사용 예시)
///       void OnEnable()  => PuzzleManager.OnPuzzleSolved += OnSolved;
///       void OnDisable() => PuzzleManager.OnPuzzleSolved -= OnSolved;
///       void OnSolved(string id) { if (id == "3D-6") door.Open(); }
/// </summary>
/// 
[Serializable]
public class PuzzleDisplayPosition
{
    public string puzzleID;

    
    public GameObject activateObjectDisplay;

    public GameObject deactivateObjectDisplay;
}

public class PuzzleManager : MonoBehaviour
{
    #region Singleton

    public static PuzzleManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 이동해도 상태 유지
    }

    #endregion

    #region Settings
    [Header("각 물체에서 거울 위치 설정")]
    [SerializeField] private List<PuzzleDisplayPosition> displayPositions;

    [Tooltip("차원별 전체 퍼즐 개수 (진행도 표시용)")]
    public int total2DPuzzles = 5;
    public int total3DPuzzles = 5;

    readonly Dictionary<string, PuzzleDimension> _solved = new();

    public int SolvedCount => _solved.Count; // 전체 푼 퍼즐 수

    #endregion

    #region Event

    /// <summary>진행도가 바뀔 때마다 (차원, 그 차원에서 푼 개수, 그 차원 전체 개수)를 전달.</summary>
    public static event Action<PuzzleDimension, int, int> OnProgressChanged;
    /// <summary>특정 퍼즐이 처음 풀렸을 때 그 id를 전달</summary>
    public static event Action<string> OnPuzzleSolved;

    #endregion

    #region Puzzle Solved Report (public API)

    /// <summary>public API: 퍼즐이 풀렸을 때 호출. 같은 id 중복 호출은 무시.</summary>
    public void ReportSolved(string puzzleId, PuzzleDimension dimension)
    {
        if (string.IsNullOrEmpty(puzzleId))
        {
            Debug.LogWarning("[PuzzleManager] 빈 puzzleId로 ReportSolved 호출됨");
            return;
        }


        ActivateDisplayPosition(puzzleId);

        if (_solved.ContainsKey(puzzleId)) return; // 이미 풀린 퍼즐이면 무시
        _solved[puzzleId] = dimension;



        OnPuzzleSolved?.Invoke(puzzleId);
        OnProgressChanged?.Invoke(dimension, SolvedCountOf(dimension), TotalOf(dimension));
    }


    private void ActivateDisplayPosition(string puzzleId)
{
    string normalizedId = puzzleId.Trim();

    foreach (PuzzleDisplayPosition entry in displayPositions)
    {
        if (entry == null || entry.activateObjectDisplay == null)
            continue;

        if (!string.Equals(
                entry.puzzleID?.Trim(),
                normalizedId,
                StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }
        // 비어 있거나 Missing이면 비활성화를 건너뜁니다.
        if (entry.deactivateObjectDisplay != null)
        {
            entry.deactivateObjectDisplay.SetActive(false);
        }

        // gameobject를 활성화
        entry.activateObjectDisplay.SetActive(true);
        return;
    }

    Debug.LogWarning(
        $"[PuzzleManager] {puzzleId}에 해당하는 displayPosition이 없습니다.",
        this
    );
}

    #endregion

    #region State Check

    /// <summary>해당 퍼즐이 풀린 상태인지 조회</summary>
    public bool IsSolved(string puzzleId) => _solved.ContainsKey(puzzleId);

    /// <summary>모든 퍼즐이 풀렸는지 상태</summary>
    public bool AllSolved => _solved.Count >= total2DPuzzles + total3DPuzzles;

    /// <summary>특정 차원의 전체 퍼즐 개수</summary>
    public int TotalOf(PuzzleDimension dimension)
        => dimension == PuzzleDimension.TwoD ? total2DPuzzles : total3DPuzzles;

    /// <summary>특정 차원에서 푼 퍼즐 개수</summary>
    public int SolvedCountOf(PuzzleDimension dimension)
    {
        int n = 0;
        foreach (var dim in _solved.Values)
            if (dim == dimension) n++;
        return n;
    }

    public int SolvedCount2D => SolvedCountOf(PuzzleDimension.TwoD);
    public int SolvedCount3D => SolvedCountOf(PuzzleDimension.ThreeD);

    /// <summary>지금까지 푼 퍼즐 id 전체 목록</summary>
    public List<string> GetSolvedList()
    {
        return new List<string>(_solved.Keys);
    }

    /// <summary>특정 차원에서 푼 퍼즐 id 목록</summary>
    public List<string> GetSolvedListOf(PuzzleDimension dimension)
    {
        var list = new List<string>();
        foreach (var kv in _solved)
            if (kv.Value == dimension) list.Add(kv.Key);
        return list;
    }

    #endregion

    #region Debug

    /// <summary>디버깅용: 지금까지 푼 퍼즐 전체를 차원별로 콘솔에 출력</summary>
    [ContextMenu("Log Solved Puzzles")]
    public void LogSolvedPuzzles()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[PuzzleManager] 진행도 " +
                      $"2D {SolvedCount2D}/{total2DPuzzles} | 3D {SolvedCount3D}/{total3DPuzzles}");

        AppendDimension(sb, PuzzleDimension.TwoD, "2D");
        AppendDimension(sb, PuzzleDimension.ThreeD, "3D");

        Debug.Log(sb.ToString());
    }

    void AppendDimension(StringBuilder sb, PuzzleDimension dimension, string label)
    {
        var list = GetSolvedListOf(dimension);
        sb.AppendLine($"  [{label}] {list.Count}개");
        foreach (var id in list)
            sb.AppendLine($"    - {id}");
    }

    #endregion
}
