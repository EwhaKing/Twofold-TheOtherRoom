using UnityEngine;

public class ShadowBlockAnswer : PuzzleBase
{
   [Header("Blocks From Left To Right")]
    public MoveBlocks[] blocks;

    private readonly int[] answer = { 1, 3, 4, 2 };

    public void CheckClear()
    {
        if (IsCleared) return;

        if (blocks == null || blocks.Length != answer.Length)
        {
            Debug.LogWarning("블록 개수와 정답 개수가 다름");
            return;
        }

        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] == null) return;

            if (blocks[i].GetZone() != answer[i])
            {
                Debug.Log("현재"+i+"의 위치는"+ blocks[i].GetZone() + "but answer is" +answer[i] );
                return;
            }
        }
        
        Debug.Log("ClearPuzzle 전");
        ClearPuzzle();
    }
}
