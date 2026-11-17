using System;
using UnityEngine;

/// <summary>
/// 퍼즐에서 사용할 공통 이미지 인덱스와 이미지 안쪽에 표시할 텍스트를 설정합니다.
/// ICloseInspection 구현 컴포넌트와 같은 GameObject에 붙여 사용합니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class PuzzleDescriptionImages : MonoBehaviour
{
    [Serializable]
    public sealed class DescriptionImage
    {
        [Tooltip("InspectionUIController의 Description Images 배열 인덱스")]
        public int imageIndex;

        public string text;
    }

    [SerializeField] private DescriptionImage[] imageIndexes = new DescriptionImage[0];

    public DescriptionImage[] ImageIndexes => imageIndexes;
}
