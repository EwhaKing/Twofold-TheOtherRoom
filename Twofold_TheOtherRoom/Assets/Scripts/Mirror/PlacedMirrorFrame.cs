using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>완성 위치에 미리 배치한 거울 조각의 활성 상태를 관리합니다.</summary>
public class PlacedMirrorFrame : MonoBehaviour
{
    [Serializable]
    private class PlacedPieceEntry
    {
        public string puzzleId;
        public GameObject placedPiece;
    }

    [SerializeField] private List<PlacedPieceEntry> pieces = new();

    private void OnEnable()
    {
        MirrorManager.OnMirrorPiecePlaced += HandlePiecePlaced;
        SetAllPiecesActive(false);
        Rebuild();
    }

    private void Start()
    {
       //미러 매니저 보다 먼저 활성화 됬을 경우 return
        Rebuild();
    }

    private void OnDisable()
    {
        MirrorManager.OnMirrorPiecePlaced -= HandlePiecePlaced;
    }

    public void Rebuild()
    {
        if (MirrorManager.Instance == null) return;

        foreach (PlacedPieceEntry entry in pieces)
        {
            if (entry.placedPiece == null) continue;

            bool isPlaced = MirrorManager.Instance.IsMirrorPiecePlaced(entry.puzzleId);
            entry.placedPiece.SetActive(isPlaced);
        }
    }

    private void SetAllPiecesActive(bool active)
    {
        foreach (PlacedPieceEntry entry in pieces)
        {
            if (entry.placedPiece != null)
            {
                entry.placedPiece.SetActive(active);
            }
        }
    }

    private void HandlePiecePlaced(string puzzleId) => Rebuild();
}
