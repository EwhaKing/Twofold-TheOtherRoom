using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ThreeDCommunicationDescriptionImages : MonoBehaviour, IInspectionDescription
{
    [Header("스테이지 진행 중 추가 설명 (기존 설명 뒤에 덧붙임)")]
    [SerializeField] private int imageIndex = 0;
    [SerializeField] private string text = "-리셋 버튼";

    public IEnumerable<PuzzleDescriptionImages.DescriptionImage> GetDescriptions()
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
