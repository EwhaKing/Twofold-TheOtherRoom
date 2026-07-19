using TMPro;
using UnityEngine;

public class NumberSlot : MonoBehaviour
{
    public TMP_Text numberText;

    private int currentNumber = 0;

    void Start()
    {
        UpdateNumber();
    }

    public void Increase()
    {
        currentNumber++;

        if (currentNumber > 9)
            currentNumber = 0;        
        UpdateNumber();
    }

    public void Decrease()
    {
        currentNumber--;

        if (currentNumber < 0)
            currentNumber = 9;
        UpdateNumber();
    }

    void UpdateNumber()
    {
        numberText.text = currentNumber.ToString();
    }

    public int GetNumber()
    {
        return currentNumber;
    }
}