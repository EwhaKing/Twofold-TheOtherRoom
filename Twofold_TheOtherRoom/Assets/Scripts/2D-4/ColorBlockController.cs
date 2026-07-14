using UnityEngine;

public class ColorBlockController : MonoBehaviour
{
    public GameObject[] blocks;

    private int blockCount = 0;

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
        }
    }

    public void DecreaseBlock()
    {
        if (blockCount > 0)
        {
            blockCount--;
            UpdateBlocks();
        }
    }
}