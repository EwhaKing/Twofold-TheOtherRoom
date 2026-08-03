using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>수집한 조각을 공용 프리팹으로 생성하고 드래그 퍼즐을 구성합니다.</summary>
public class MirrorFrame : MonoBehaviour
{
    [Serializable]
    private class PieceEntry
    {
        public string puzzleId;
        public Sprite sprite;
        public RectTransform spawnPoint;
        public RectTransform correctPosition;
    }

    [SerializeField] private GameObject commonPiecePrefab;
    [SerializeField] private List<PieceEntry> pieces = new();

    private readonly List<GameObject> spawnedPieces = new();

    private void OnEnable()
    {
        PuzzleManager2.OnMirrorPieceObtained += HandleStateChanged;
        PuzzleManager2.OnMirrorPiecePlaced += HandleStateChanged;
        Rebuild();
    }

    private void OnDisable()
    {
        PuzzleManager2.OnMirrorPieceObtained -= HandleStateChanged;
        PuzzleManager2.OnMirrorPiecePlaced -= HandleStateChanged;
    }

    public void Rebuild()
    {
        ClearPieces();
        if (PuzzleManager2.Instance == null || commonPiecePrefab == null) return;

        foreach (PieceEntry entry in pieces)
        {
            if (!PuzzleManager2.Instance.HasMirrorPiece(entry.puzzleId)) continue;
            if (entry.sprite == null) continue;

            bool isPlaced = PuzzleManager2.Instance.IsMirrorPiecePlaced(entry.puzzleId);
            RectTransform parent = isPlaced ? entry.correctPosition : entry.spawnPoint;
            if (parent == null) continue;

            GameObject instance = Instantiate(commonPiecePrefab, parent, false);
            DraggableMirrorPiece piece = instance.GetComponent<DraggableMirrorPiece>();
            if (piece == null)
            {
                Debug.LogError("[MirrorFrame] 공용 프리팹에 DraggableMirrorPiece가 없습니다.", instance);
                Destroy(instance);
                continue;
            }

            piece.Initialize(entry.puzzleId, entry.sprite, entry.correctPosition, isPlaced);
            spawnedPieces.Add(instance);
        }
    }

    private void HandleStateChanged(string puzzleId) => Rebuild();

    private void ClearPieces()
    {
        foreach (GameObject piece in spawnedPieces)
        {
            if (piece != null) Destroy(piece);
        }

        spawnedPieces.Clear();
    }
}
