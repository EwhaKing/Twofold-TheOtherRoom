using TMPro;
using UnityEngine;

public class NumberSlot : MonoBehaviour
{
    public TMP_Text numberText;

    private int currentNumber = 0;
<<<<<<< HEAD
    public Answer controller;
=======
>>>>>>> origin/LAY/2D-8

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
<<<<<<< HEAD
        controller.CheckAnswer();

=======
>>>>>>> origin/LAY/2D-8
    }

    public void Decrease()
    {
        currentNumber--;

        if (currentNumber < 0)
            currentNumber = 9;
        UpdateNumber();
<<<<<<< HEAD
        controller.CheckAnswer();
=======
>>>>>>> origin/LAY/2D-8
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