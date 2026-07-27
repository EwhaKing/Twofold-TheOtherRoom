using System.Collections;
using TMPro;
using UnityEngine;

public class NumberAlphabetPuzzle : MonoBehaviour
{
    private enum PuzzleStep
    {
        Number,
        Alphabet
    }

    [Header("Puzzle 정보")]
    [SerializeField] private string puzzleId = "2D-12";
    [SerializeField] private PuzzleDimension dimension = PuzzleDimension.TwoD;

    [Header("UI")]
    [SerializeField] private GameObject numberPanel;
    [SerializeField] private GameObject alphabetPanel;
    [SerializeField] private GameObject clearPanel;

    [Header("잠금 UI")]
    [SerializeField] private CanvasGroup puzzleCanvasGroup;
    [SerializeField] private GameObject lockPanel;
    [SerializeField] private TMP_Text lockTimerText;

    [Header("정답")]
    [SerializeField] private int correctNumber = 4;
    [SerializeField] private string correctAlphabet = "H";

    [Header("오답 설정")]
    [SerializeField] private int maxWrongAnswers = 3;
    [SerializeField] private float lockTime = 60f;

    [Header("텍스트")]
    [SerializeField] private TMP_Text wrongCountText;
    [SerializeField] private TMP_Text resultText;

    private PuzzleStep currentStep;
    private int wrongCount;

    private bool isLocked;
    private bool isSolved;

    private Coroutine lockRoutine;

    private void Start()
    {
        ResetPuzzle();
    }

    public void SelectNumber(int number)
    {
        if (!CanInput()) return;
        if (currentStep != PuzzleStep.Number) return;

        if (number == correctNumber)
        {
            wrongCount = 0;
            UpdateWrongCount();

            ShowAlphabetQuestion();
            SetResult("정답입니다!");
        }
        else
        {
            WrongAnswer();
        }
    }

    public void SelectAlphabet(string alphabet)
    {
        if (!CanInput()) return;
        if (currentStep != PuzzleStep.Alphabet) return;

        if (alphabet.Trim().ToUpper() == correctAlphabet.Trim().ToUpper())
        {
            PuzzleClear();
        }
        else
        {
            WrongAnswer();
        }
    }

    private bool CanInput()
    {
        return !isLocked && !isSolved;
    }

    private void WrongAnswer()
    {
        wrongCount++;

        UpdateWrongCount();
        SetResult("틀렸습니다.");

        if (wrongCount >= maxWrongAnswers)
        {
            if (lockRoutine != null)
                StopCoroutine(lockRoutine);

            lockRoutine = StartCoroutine(LockCoroutine());
        }
    }

    private IEnumerator LockCoroutine()
    {
        isLocked = true;
        SetInput(false);

        if (lockPanel != null)
            lockPanel.SetActive(true);

        float time = lockTime;

        while (time > 0)
        {
            if (lockTimerText != null)
            {
                lockTimerText.text =
                    $"3번 연속 틀렸습니다.\n{Mathf.CeilToInt(time)}초 후 다시 시도";
            }

            time -= Time.unscaledDeltaTime;
            yield return null;
        }

        isLocked = false;
        wrongCount = 0;

        SetInput(true);

        if (lockPanel != null)
            lockPanel.SetActive(false);

        UpdateWrongCount();
        SetResult("다시 시도할 수 있습니다.");

        lockRoutine = null;
    }

    private void ShowNumberQuestion()
    {
        currentStep = PuzzleStep.Number;

        if (numberPanel != null)
            numberPanel.SetActive(true);

        if (alphabetPanel != null)
            alphabetPanel.SetActive(false);
    }

    private void ShowAlphabetQuestion()
    {
        currentStep = PuzzleStep.Alphabet;

        if (numberPanel != null)
            numberPanel.SetActive(false);

        if (alphabetPanel != null)
            alphabetPanel.SetActive(true);
    }

    private void PuzzleClear()
    {
        if (isSolved) return;

        isSolved = true;

        Debug.Log("스테이지 클리어!");

        SetInput(false);

        if (numberPanel != null)
            numberPanel.SetActive(false);

        if (alphabetPanel != null)
            alphabetPanel.SetActive(false);

        if (lockPanel != null)
            lockPanel.SetActive(false);

        if (clearPanel != null)
            clearPanel.SetActive(true);

        SetResult("스테이지 클리어!");

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(puzzleId, dimension);
        }
        else
        {
            Debug.LogError("PuzzleManager를 찾을 수 없습니다.");
        }
    }

    private void SetInput(bool enable)
    {
        if (puzzleCanvasGroup == null)
            return;

        puzzleCanvasGroup.interactable = enable;
        puzzleCanvasGroup.blocksRaycasts = enable;
    }

    private void UpdateWrongCount()
    {
        if (wrongCountText == null)
            return;

        string name = currentStep == PuzzleStep.Number ? "숫자 문제" : "알파벳 문제";

        wrongCountText.text = $"{name} 오답 {wrongCount}/{maxWrongAnswers}";
    }

    private void SetResult(string message)
    {
        if (resultText != null)
            resultText.text = message;
    }

    [ContextMenu("Reset Puzzle")]
    public void ResetPuzzle()
    {
        if (lockRoutine != null)
        {
            StopCoroutine(lockRoutine);
            lockRoutine = null;
        }

        isSolved = false;
        isLocked = false;
        wrongCount = 0;

        if (clearPanel != null)
            clearPanel.SetActive(false);

        if (lockPanel != null)
            lockPanel.SetActive(false);

        SetInput(true);

        ShowNumberQuestion();

        UpdateWrongCount();
        SetResult("");
    }
}