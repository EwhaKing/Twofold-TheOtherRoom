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
        // A → B → C → D → A
        if (currentLetter == 'D')
            currentLetter = 'A';
        else
            currentLetter++;
            SoundManager.Instance.PlaySFX(SFXType.UIClick);

        UpdateLetter();
    }

    public void DecreaseLetter()
    {
        // A → D → C → B → A
        if (currentLetter == 'A')
            currentLetter = 'D';
        else
            currentLetter--;
            SoundManager.Instance.PlaySFX(SFXType.UIClick);

        UpdateLetter();
    }

    private void UpdateLetter()
    {
        letterText.text = currentLetter.ToString();

        if (checker != null)
            checker.CheckAnswer();
    }
}