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
        if (checker != null && checker.IsSolved)
        {
            return;
        }

        if (blockCount < blocks.Length)
        {
            blockCount++;
            UpdateBlocks();

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.UIClick);
            }

            if (checker != null)
                checker.CheckAnswer();
        }
    }

    public void DecreaseBlock()
    {
        if (checker != null && checker.IsSolved)
        {
            return;
        }

        if (blockCount > 0)
        {
            blockCount--;
            UpdateBlocks();

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.UIClick);
            }

            if (checker != null)
                checker.CheckAnswer();
        }
    }
}