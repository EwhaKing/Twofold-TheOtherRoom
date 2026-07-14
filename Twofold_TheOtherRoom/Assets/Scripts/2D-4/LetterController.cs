using TMPro;
using UnityEngine;

public class LetterController : MonoBehaviour
{
    public TMP_Text letterText;

    private char currentLetter = 'A';

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
        }
    }

    public void DecreaseLetter()
    {
        if (currentLetter > 'A')
        {
            currentLetter--;
            letterText.text = currentLetter.ToString();
        }
    }
}