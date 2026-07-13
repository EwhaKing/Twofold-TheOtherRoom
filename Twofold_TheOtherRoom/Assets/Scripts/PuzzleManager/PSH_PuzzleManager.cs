using UnityEngine;
using System.Collections.Generic;


public class PSH_PuzzleManager : MonoBehaviour
{
    public static PSH_PuzzleManager Instance;

    private readonly List<PuzzleBase> puzzles = new List<PuzzleBase>();
    private int clearedCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterPuzzle(PuzzleBase puzzle)
    {
        if (puzzles.Contains(puzzle)) return;

        puzzles.Add(puzzle);
        Debug.Log("퍼즐 등록: " + puzzle.puzzleId);
    }

    public void NotifyPuzzleCleared(PuzzleBase puzzle)
    {
        clearedCount++;

        Debug.Log("퍼즐 진행도: " + clearedCount + " / " + puzzles.Count);

        if (clearedCount >= puzzles.Count)
        {
            Debug.Log("모든 퍼즐 클리어!");
            // 여기서 출구 열기, 다음 단계 오픈 등
        }
    }
}
