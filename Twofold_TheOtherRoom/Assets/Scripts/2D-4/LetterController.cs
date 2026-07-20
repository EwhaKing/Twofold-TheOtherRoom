using TMPro;
using UnityEngine;

public class LetterController : MonoBehaviour
{
    public TMP_Text letterText;
    public PuzzleChecker checker;

    private char currentLetter = 'A';

    public char CurrentLetter => currentLetter;

    void Start()
    {
        letterText.text = currentLetter.ToString();
    }

    public void IncreaseLetter()
    {
        if (currentLetter < 'D')
        {
            currentLetter++;
            letterText.text = currentLetter.ToString();

            if (checker != null)
                checker.CheckAnswer();
        }
    }

    public void DecreaseLetter()
    {
        if (currentLetter > 'A')
        {
            currentLetter--;
            letterText.text = currentLetter.ToString();

            if (checker != null)
                checker.CheckAnswer();
        }
    }
}