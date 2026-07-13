using UnityEngine;

public abstract class PuzzleBase : MonoBehaviour
{
    [Header("Puzzle Info")]
    public string puzzleId;
    public bool IsCleared { get; private set; }

    protected virtual void Start()
    {
        
        PSH_PuzzleManager.Instance.RegisterPuzzle(this);
    }

    protected void ClearPuzzle()
    {
        if (IsCleared) return;

        IsCleared = true;
        Debug.Log(puzzleId + " 클리어! RegisterPuzzle 호출");
        PSH_PuzzleManager.Instance.NotifyPuzzleCleared(this);
    }
}