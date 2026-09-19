using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ThreeDCommunicationDescriptionImages : MonoBehaviour
{
    [SerializeField] private int imageIndex = 0;
    [SerializeField] private string text = "-리셋 버튼";

    public IEnumerable<PuzzleDescriptionImages.DescriptionImage> GetAdditionalDescriptionImages()
    {
        ThreeDCommunicationPuzzle puzzle = GetComponent<ThreeDCommunicationPuzzle>();
        if (puzzle != null && puzzle.HasStartedStages)
        {
            yield return new PuzzleDescriptionImages.DescriptionImage
            {
                imageIndex = this.imageIndex,
                text = this.text
            };
        }
    }
}
