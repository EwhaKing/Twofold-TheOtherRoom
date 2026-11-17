using System.Collections.Generic;

/// <summary>상태별 설명 항목. ICloseInspection 구현 컴포넌트와 같은 GameObject.</summary>
public interface IInspectionDescription
{
    IEnumerable<PuzzleDescriptionImages.DescriptionImage> GetDescriptions();
}
