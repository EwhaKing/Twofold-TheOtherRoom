using System.Collections.Generic;
using UnityEngine;


/// 3D-11 상태별 설명. 기본 설명(입력/결정)은 같은 오브젝트의 PuzzleDescriptionImages가 담당합니다.
/// - 스테이지 진행 중: 기본 설명 뒤에 리셋 버튼 설명을 덧붙임
/// - 클리어 후: ThreeDCommunicationPuzzle이 PuzzleDescriptionImages를 끄고, Cleared Descriptions만 표시
[DisallowMultipleComponent]
public sealed class ThreeDCommunicationDescriptionImages : MonoBehaviour, IInspectionDescription
{
    [Header("스테이지 진행 중 추가 설명 (기본 설명 뒤에 덧붙임)")]
    [SerializeField] private int imageIndex = 0;
    [SerializeField] private string text = "-리셋 버튼";

    [Header("클리어 후 설명 (MirrorPanel이 나온 뒤, 위 설명 대신 이것만 표시)")]
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

    // 반환 타입은 인터페이스대로 IEnumerable, 안에서는 List로 만들어서 반환 (List도 IEnumerable)
    public IEnumerable<PuzzleDescriptionImages.DescriptionImage> GetDescriptions()
    {
        var list = new List<PuzzleDescriptionImages.DescriptionImage>();

        // 클리어 후: 클리어용 설명만
        if (Puzzle != null && Puzzle.IsSolved && clearedDescriptions.Length > 0)
        {
            list.AddRange(clearedDescriptions);
            return list;
        }


        // 스테이지 진행 중: 리셋 버튼 설명 추가
        if (Puzzle != null && Puzzle.HasStartedStages)
        {
            list.Add(new PuzzleDescriptionImages.DescriptionImage
            {
                imageIndex = imageIndex,
                text = text
            });
        }

        return list;
    }
}
