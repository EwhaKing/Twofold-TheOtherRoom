using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ThreeDCommunicationDescriptionImages : MonoBehaviour
{
    [Header("스테이지 진행 중 추가 설명 (기존 설명 뒤에 덧붙임)")]
    [SerializeField] private int imageIndex = 0;
    [SerializeField] private string text = "-리셋 버튼";

    [Header("클리어 후 설명 (MirrorPanel이 나온 뒤, 기존 설명을 통째로 대체)")]
    [Tooltip("비워 두면 클리어 후에도 기존 설명을 그대로 보여 줍니다.")]
    [SerializeField] private PuzzleDescriptionImages.DescriptionImage[] clearedDescriptions =
        new PuzzleDescriptionImages.DescriptionImage[0];

    private ThreeDCommunicationPuzzle puzzle;

    private ThreeDCommunicationPuzzle Puzzle
    {
        get
        {
            if (puzzle == null)
                puzzle = GetComponent<ThreeDCommunicationPuzzle>();
            return puzzle;
        }
    }

    public IEnumerable<PuzzleDescriptionImages.DescriptionImage> GetAdditionalDescriptionImages()
    {
        if (Puzzle != null && Puzzle.HasStartedStages)
        {
            yield return new PuzzleDescriptionImages.DescriptionImage
            {
                imageIndex = this.imageIndex,
                text = this.text
            };
        }
    }

    /// <summary>
    /// 클리어 후라면 기존 설명 대신 보여 줄 설명 목록을 돌려줍니다.
    /// true면 InspectionUIController가 PuzzleDescriptionImages + 추가 설명을 무시하고 이 목록만 표시합니다.
    /// </summary>
    public bool TryGetReplacementDescriptionImages(
        out IEnumerable<PuzzleDescriptionImages.DescriptionImage> descriptions)
    {
        descriptions = clearedDescriptions;
        return Puzzle != null
            && Puzzle.IsSolved
            && clearedDescriptions != null
            && clearedDescriptions.Length > 0;
    }
}
