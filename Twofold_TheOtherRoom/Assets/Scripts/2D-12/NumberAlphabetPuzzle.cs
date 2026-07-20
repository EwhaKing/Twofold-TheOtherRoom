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

    [Header("PuzzleManager 설정")]
    [SerializeField] private string puzzleId = "2D-12";
    [SerializeField] private PuzzleDimension dimension = PuzzleDimension.TwoD;

    [Header("문제 화면")]
    [SerializeField] private GameObject numberPanel;
    [SerializeField] private GameObject alphabetPanel;
    [SerializeField] private GameObject clearPanel;

    [Header("입력 제한")]
    [SerializeField] private CanvasGroup puzzleCanvasGroup;
    [SerializeField] private GameObject lockPanel;
    [SerializeField] private TMP_Text lockTimerText;

    [Header("오답 제한")]
    [SerializeField] private int maxWrongAnswers = 3;
    [SerializeField] private float lockDuration = 60f;

    [Header("정답")]
    [SerializeField] private int correctNumber = 4;
    [SerializeField] private string correctAlphabet = "H";

    [Header("선택 사항")]
    [SerializeField] private TMP_Text wrongCountText;
    [SerializeField] private TMP_Text resultText;

    private PuzzleStep currentStep = PuzzleStep.Number;

    // 현재 문제에서 틀린 횟수만 저장
    private int currentWrongCount;

    private bool isLocked;
    private bool isSolved;
    private Coroutine lockCoroutine;

    private void Start()
    {
        ShowNumberQuestion();

        if (clearPanel != null)
            clearPanel.SetActive(false);

        if (lockPanel != null)
            lockPanel.SetActive(false);

        UpdateWrongCountText();
        SetResultText(string.Empty);
    }

    public void SelectNumber(int selectedNumber)
    {
        if (!CanReceiveInput())
            return;

        if (currentStep != PuzzleStep.Number)
            return;

        if (selectedNumber == correctNumber)
        {
            // 숫자 문제를 통과했으므로 오답 횟수를 초기화
            currentWrongCount = 0;

            SetResultText("정답입니다!");
            ShowAlphabetQuestion();
            UpdateWrongCountText();
        }
        else
        {
            RegisterWrongAnswer();
        }
    }

    public void SelectAlphabet(string selectedAlphabet)
    {
        if (!CanReceiveInput())
            return;

        if (currentStep != PuzzleStep.Alphabet)
            return;

        bool isCorrect = string.Equals(
            selectedAlphabet.Trim(),
            correctAlphabet.Trim(),
            System.StringComparison.OrdinalIgnoreCase
        );

        if (isCorrect)
        {
            CompletePuzzle();
        }
        else
        {
            RegisterWrongAnswer();
        }
    }

    private bool CanReceiveInput()
    {
        return !isLocked && !isSolved;
    }

    private void RegisterWrongAnswer()
    {
        currentWrongCount++;

        SetResultText("틀렸습니다.");
        UpdateWrongCountText();

        if (currentWrongCount >= maxWrongAnswers)
        {
            if (lockCoroutine != null)
                StopCoroutine(lockCoroutine);

            lockCoroutine = StartCoroutine(LockPuzzleCoroutine());
        }
    }

    private IEnumerator LockPuzzleCoroutine()
    {
        isLocked = true;
        SetPuzzleInputEnabled(false);

        if (lockPanel != null)
            lockPanel.SetActive(true);

        float remainingTime = lockDuration;

        while (remainingTime > 0f)
        {
            if (lockTimerText != null)
            {
                int seconds = Mathf.CeilToInt(remainingTime);

                lockTimerText.text =
                        ///$"현재 문제에서 3번 틀렸습니다.\n" +
                        ///$"{seconds}초 후 다시 시도할 수 있습니다.";
                        $"Wait {seconds}s";
            }

            remainingTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        // 대기가 끝나면 현재 문제의 오답 횟수만 초기화
        currentWrongCount = 0;
        isLocked = false;
        lockCoroutine = null;

        if (lockPanel != null)
            lockPanel.SetActive(false);

        SetPuzzleInputEnabled(true);
        UpdateWrongCountText();
        SetResultText("다시 시도할 수 있습니다.");
    }

    private void ShowNumberQuestion()
    {
        currentStep = PuzzleStep.Number;
        currentWrongCount = 0;

        if (numberPanel != null)
            numberPanel.SetActive(true);

        if (alphabetPanel != null)
            alphabetPanel.SetActive(false);
    }

    private void ShowAlphabetQuestion()
    {
        currentStep = PuzzleStep.Alphabet;
        currentWrongCount = 0;

        if (numberPanel != null)
            numberPanel.SetActive(false);

        if (alphabetPanel != null)
            alphabetPanel.SetActive(true);
    }

    private void CompletePuzzle()
    {
        if (isSolved)
            return;

        isSolved = true;
        SetPuzzleInputEnabled(false);

        if (numberPanel != null)
            numberPanel.SetActive(false);

        if (alphabetPanel != null)
            alphabetPanel.SetActive(false);

        if (lockPanel != null)
            lockPanel.SetActive(false);

        if (clearPanel != null)
            clearPanel.SetActive(true);

        SetResultText("스테이지 클리어!");

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(puzzleId, dimension);
        }
        else
        {
            Debug.LogError(
                $"[{name}] PuzzleManager가 씬에 없습니다."
            );
        }
    }

    private void SetPuzzleInputEnabled(bool enabled)
    {
        if (puzzleCanvasGroup == null)
            return;

        puzzleCanvasGroup.interactable = enabled;
        puzzleCanvasGroup.blocksRaycasts = enabled;
    }

    private void UpdateWrongCountText()
    {
        if (wrongCountText == null)
            return;

        string stepName =
            currentStep == PuzzleStep.Number
                ? "숫자 문제"
                : "알파벳 문제";

        wrongCountText.text =
            $"{stepName} 오답 {currentWrongCount} / {maxWrongAnswers}";
    }

    private void SetResultText(string message)
    {
        if (resultText != null)
            resultText.text = message;
    }

    [ContextMenu("Reset This Puzzle")]
    public void ResetPuzzle()
    {
        if (lockCoroutine != null)
        {
            StopCoroutine(lockCoroutine);
            lockCoroutine = null;
        }

        currentWrongCount = 0;
        isLocked = false;
        isSolved = false;

        SetPuzzleInputEnabled(true);

        if (lockPanel != null)
            lockPanel.SetActive(false);

        if (clearPanel != null)
            clearPanel.SetActive(false);

        ShowNumberQuestion();
        UpdateWrongCountText();
        SetResultText(string.Empty);
    }
}