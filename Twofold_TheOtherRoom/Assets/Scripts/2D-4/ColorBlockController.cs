using UnityEngine;

public class ColorBlockController : MonoBehaviour
{
    public GameObject[] blocks;
    public PuzzleChecker checker;

    private int blockCount = 0;

    
    public int BlockCount => blockCount;

    void Start()
    {
        UpdateBlocks();
    }

    void UpdateBlocks()
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].SetActive(i < blockCount);
        }
    }

    public void IncreaseBlock()
    {
        if (blockCount < blocks.Length)
        {
            blockCount++;
            UpdateBlocks();

            if (checker != null)
                checker.CheckAnswer();
        }
    }

    public void DecreaseBlock()
    {
        if (blockCount > 0)
        {
            blockCount--;
            UpdateBlocks();

            if (checker != null)
                checker.CheckAnswer();
        }
    }
}