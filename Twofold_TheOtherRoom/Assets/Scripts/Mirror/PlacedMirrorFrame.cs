using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>배치 완료된 조각을 결과용 거울에 표시합니다.</summary>
public class PlacedMirrorFrame : MonoBehaviour
{
    [Serializable]
    private class PlacedPieceEntry
    {
        public string puzzleId;
        public Sprite sprite;
        public RectTransform placedPosition;
    }

    [SerializeField] private GameObject commonPiecePrefab;
    [SerializeField, Min(0f)] private float placedPieceScale = 0.2f;
    [SerializeField] private List<PlacedPieceEntry> pieces = new();

    private readonly List<GameObject> spawnedPieces = new();

    private void OnEnable()
    {
        MirrorManager.OnMirrorPiecePlaced += HandlePiecePlaced;
        Rebuild();
    }

    private void OnDisable()
    {
        MirrorManager.OnMirrorPiecePlaced -= HandlePiecePlaced;
    }

    public void Rebuild()
    {
        ClearPieces();
        if (MirrorManager.Instance == null || commonPiecePrefab == null) return;

        foreach (PlacedPieceEntry entry in pieces)
        {
            if (!MirrorManager.Instance.IsMirrorPiecePlaced(entry.puzzleId)) continue;
            if (entry.sprite == null || entry.placedPosition == null) continue;

            GameObject instance = Instantiate(commonPiecePrefab, entry.placedPosition, false);
            Image image = instance.GetComponent<Image>();
            if (image == null)
            {
                Debug.LogError("[PlacedMirrorFrame] 공용 프리팹에 Image가 없습니다.", instance);
                Destroy(instance);
                continue;
            }

            image.sprite = entry.sprite;
            image.SetNativeSize();
            instance.transform.localScale = new Vector3(placedPieceScale, placedPieceScale, 1f);

            DraggableMirrorPiece draggable = instance.GetComponent<DraggableMirrorPiece>();
            if (draggable != null) draggable.enabled = false;

            CanvasGroup canvasGroup = instance.GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;

            spawnedPieces.Add(instance);
        }
    }

    private void HandlePiecePlaced(string puzzleId) => Rebuild();

    private void ClearPieces()
    {
        foreach (GameObject piece in spawnedPieces)
        {
            if (piece != null) Destroy(piece);
        }

        spawnedPieces.Clear();
    }
}
