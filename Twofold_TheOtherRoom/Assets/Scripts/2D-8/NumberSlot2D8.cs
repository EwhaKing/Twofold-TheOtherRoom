using TMPro;
using UnityEngine;

public class NumberSlot2D8 : MonoBehaviour
{
    public TMP_Text numberText;

    private int currentNumber = 0;
    public Answer2D8 controller;

    void Start()
    {
        UpdateNumber();
    }

    public void Increase()
    {
        currentNumber++;
        SoundManager.Instance.PlaySFX(SFXType.UIClick);

        if (currentNumber > 9)
        {
            currentNumber = 0; 
        }       
        UpdateNumber();
        controller.CheckAnswer();

    }

    public void Decrease()
    {
        currentNumber--;
        SoundManager.Instance.PlaySFX(SFXType.UIClick);

        if (currentNumber < 0)
        {
            currentNumber = 9;
        }
        UpdateNumber();
        controller.CheckAnswer();
    }

    void UpdateNumber()
    {
        if (controller.GetSolve())
        {
            return;
        }
        else
        {
        numberText.text = currentNumber.ToString();
        }
    }

    public int GetNumber()
    {
        return currentNumber;
    }
}