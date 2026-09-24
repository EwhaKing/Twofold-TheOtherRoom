using UnityEngine;
using UnityEngine.UI;

public class ColorBlockController : MonoBehaviour
{
    public GameObject[] blocks;
    public PuzzleChecker checker;

    [Header("버튼 색")]
    public Image upButton;
    public Image downButton;
    public Color buttonColor = Color.white;

    private int blockCount = 0;

    
    public int BlockCount => blockCount;

    void Start()
    {
        UpdateBlocks();

        if (upButton != null)
            upButton.color = buttonColor;

        if (downButton != null)
            downButton.color = buttonColor;
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