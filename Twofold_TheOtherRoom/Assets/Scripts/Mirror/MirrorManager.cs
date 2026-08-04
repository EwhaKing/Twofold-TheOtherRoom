using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D/3D 거울 조각의 획득 및 배치 상태를 통합 관리합니다.
/// 기존 PuzzleManager는 변경하지 않습니다.
/// </summary>
public class MirrorManager : MonoBehaviour
{
    public static MirrorManager Instance { get; private set; }

    [Header("Required Mirror Piece IDs")]
    [SerializeField] private List<string> required2DPieceIds = new();
    [SerializeField] private List<string> required3DPieceIds = new();

    private readonly HashSet<string> obtainedMirrorPieces = new(StringComparer.OrdinalIgnoreCase); //획득한 거울 조각 ID를 실제로 보관하는 데이터 저장소
    private readonly HashSet<string> placedMirrorPieces = new(StringComparer.OrdinalIgnoreCase); // 배치완료된 거울 조각 
    public enum PuzzleDimension { TwoD, ThreeD }

    public static event Action<string> OnMirrorPieceObtained; //“조각을 획득했다”고 다른 코드에 알리는 이벤트
    public static event Action<string> OnMirrorPiecePlaced;
    public static event Action<PuzzleDimension> OnDimensionMirrorCompleted;
    public bool All2DMirrorPiecesPlaced => AreAllMirrorPiecesPlaced(PuzzleDimension.TwoD);
    public bool All3DMirrorPiecesPlaced => AreAllMirrorPiecesPlaced(PuzzleDimension.ThreeD);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GetMirrorPiece(string puzzleId)
    {
        string id = NormalizeId(puzzleId);
        if (!ValidateId(id)) return;

        if (obtainedMirrorPieces.Add(id)) OnMirrorPieceObtained?.Invoke(id);
    }

    public void MirrorPiecePlaced(string puzzleId)
    {
        string id = NormalizeId(puzzleId);
        if (!ValidateId(id)) return;

        if (!obtainedMirrorPieces.Contains(id))
        {
            Debug.LogWarning($"[MirrorManager] 획득하지 않은 거울 조각입니다: {id}", this);
            return;
        }

        if (!placedMirrorPieces.Add(id)) return;

        OnMirrorPiecePlaced?.Invoke(id);

        PuzzleDimension dimension = GetDimension(id);
        if (AreAllMirrorPiecesPlaced(dimension)) OnDimensionMirrorCompleted?.Invoke(dimension);
    }

    public bool HasMirrorPiece(string puzzleId)
        => obtainedMirrorPieces.Contains(NormalizeId(puzzleId));

    public bool IsMirrorPiecePlaced(string puzzleId)
        => placedMirrorPieces.Contains(NormalizeId(puzzleId));

    public bool AreAllMirrorPiecesPlaced(PuzzleDimension dimension)
    {
        List<string> requiredIds = dimension == PuzzleDimension.TwoD
            ? required2DPieceIds
            : required3DPieceIds;

        if (requiredIds == null || requiredIds.Count == 0) return false;

        foreach (string rawId in requiredIds)
        {
            string id = NormalizeId(rawId);
            if (!ValidateId(id) || !placedMirrorPieces.Contains(id)) return false;
        }

        return true;
    }

    public List<string> GetObtainedMirrorPieceIds() => new(obtainedMirrorPieces);
    public List<string> GetPlacedMirrorPieceIds() => new(placedMirrorPieces);

    private static string NormalizeId(string puzzleId)
        => string.IsNullOrWhiteSpace(puzzleId) ? string.Empty : puzzleId.Trim().ToUpperInvariant();

    private bool ValidateId(string puzzleId)
    {
        if (!string.IsNullOrEmpty(puzzleId)) return true;
        Debug.LogWarning("[MirrorManager] puzzleId가 비어 있습니다.", this);
        return false;
    }

    private static PuzzleDimension GetDimension(string puzzleId)
        => puzzleId.StartsWith("3D-", StringComparison.OrdinalIgnoreCase)
            ? PuzzleDimension.ThreeD
            : PuzzleDimension.TwoD;
}
