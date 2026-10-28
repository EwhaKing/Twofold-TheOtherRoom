using System;
using UnityEngine;

[RequireComponent(typeof(MirrorPiece))]
public class MirrorObtainNoticeTrigger : MonoBehaviour
{
    [Header("자막 설정")]
    [TextArea]
    [SerializeField] private string noticeText = "거울을 얻었다. 거울 프레임에 맞춰보자";

    private MirrorPiece piece;

    private void Awake()
    {
        piece = GetComponent<MirrorPiece>();
    }

    private void OnEnable()
    {
        MirrorManager.OnMirrorPieceObtained += HandleObtained;
    }

    private void OnDisable()
    {
        MirrorManager.OnMirrorPieceObtained -= HandleObtained;
    }

    private void HandleObtained(string obtainedId)
    {
        if (piece == null || piece.PuzzleId == null) return;
        if (!string.Equals(obtainedId, piece.PuzzleId.Trim(), StringComparison.OrdinalIgnoreCase)) return;   // 다른 조각

        if (NoticeSubtitle.Instance == null)
        {
            Debug.LogWarning("[Notice] 씬에 NoticeCanvas 없음", this);
            return;
        }

        NoticeSubtitle.Instance.Show(noticeText);
    }
}
