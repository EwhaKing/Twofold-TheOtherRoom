using UnityEngine;

public class ColorRowController : MonoBehaviour
{
    public GameObject[] blocks;

    private int count = 0;

    void Start()
    {
        foreach (GameObject block in blocks)
        {
            block.SetActive(false);
        }
    }
}