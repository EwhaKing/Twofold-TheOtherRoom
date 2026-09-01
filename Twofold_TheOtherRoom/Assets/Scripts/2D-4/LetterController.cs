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
        UpdateLetter();
    }

    public void IncreaseLetter()
    {
        if (checker != null && checker.IsSolved)
        {
            return;
        }

        if (currentLetter == 'D')
            currentLetter = 'A';
        else
            currentLetter++;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.UIClick);
        }

        UpdateLetter();
    }

    public void DecreaseLetter()
    {
        if (checker != null && checker.IsSolved)
        {
            return;
        }

        if (currentLetter == 'A')
            currentLetter = 'D';
        else
            currentLetter--;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.UIClick);
        }

        UpdateLetter();
    }

    private void UpdateLetter()
    {
        letterText.text = currentLetter.ToString();

        if (checker != null)
            checker.CheckAnswer();
    }
}