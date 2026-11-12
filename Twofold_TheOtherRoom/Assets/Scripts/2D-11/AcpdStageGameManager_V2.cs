using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AcpdStageGameManager_V2 : MonoBehaviour
{
    [System.Serializable]
    public class PatternStep
    {
        public string alphabetText;
        public int[] correctSequence;
    }

    [Header("퍼즐 정보 설정")]
    public string puzzleID = "2D-11";
    public PuzzleDimension dimension = PuzzleDimension.TwoD;

    [Header("UI 요소 연결")]
    public TMP_Text displayAlphabetText;
    public Image timerBarImage;
    public Button[] shapeButtons;
    public Button resetButton;

    [Header("패턴 데이터")]
    public List<PatternStep> patternSteps = new List<PatternStep>();

    private int currentStepIndex = 0;
    private bool isShowingWrongFeedback = false;
    private bool isCleared = false;
    private bool isFinalClearing = false;

    private List<int> playerSequence = new List<int>();
    private Coroutine activeTimerCoroutine;

    private const string ClearMessage =
        "<size=62><cspace=-0.03em><voffset=-0.25em>3D 거울 획득 확인</voffset></cspace></size>";

    private void Awake()
    {
        for (int i = 0; i < shapeButtons.Length; i++)
        {
            int index = i;

            if (shapeButtons[i] != null)
            {
                shapeButtons[i].onClick.RemoveAllListeners();
                shapeButtons[i].onClick.AddListener(
                    () => OnShapeButtonClicked(index)
                );
            }
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetToFirstStep);
        }

        SetupExactPhotoPatterns();
    }

    private void OnEnable()
    {
        PuzzleManager.OnPuzzleSolved += OnPuzzleSolved;
        ResetToFirstStep();
    }

    private void Start()
    {
        // 다른 오브젝트의 Awake보다 OnEnable이 먼저 실행된 경우에도
        // PuzzleManager의 클리어 상태를 다시 확인
        if (HasBeenSolved())
        {
            RestoreClearedState();
        }
    }

    private void OnDisable()
    {
        PuzzleManager.OnPuzzleSolved -= OnPuzzleSolved;

        StopAllCoroutines();
        activeTimerCoroutine = null;
        isShowingWrongFeedback = false;
        isFinalClearing = false;
    }

    private bool HasBeenSolved()
    {
        if (isCleared)
        {
            return true;
        }

        return PuzzleManager.Instance != null &&
               PuzzleManager.Instance.IsSolved(puzzleID);
    }

    private void OnPuzzleSolved(string solvedPuzzleID)
    {
        if (solvedPuzzleID != puzzleID)
        {
            return;
        }

        RestoreClearedState();
    }

    private void RestoreClearedState()
    {
        StopAllCoroutines();
        activeTimerCoroutine = null;

        isCleared = true;
        isShowingWrongFeedback = false;
        isFinalClearing = false;

        playerSequence.Clear();

        if (displayAlphabetText != null)
        {
            displayAlphabetText.text = ClearMessage;
        }

        if (timerBarImage != null)
        {
            timerBarImage.fillAmount = 0f;
        }
    }

    private void SetupExactPhotoPatterns()
    {
        patternSteps.Clear();

        patternSteps.Add(new PatternStep
        {
            alphabetText = "A C P",
            correctSequence = new int[] { 5 }
        });

        patternSteps.Add(new PatternStep
        {
            alphabetText = "Z L O",
            correctSequence = new int[] { 3, 7 }
        });

        patternSteps.Add(new PatternStep
        {
            alphabetText = "W P M D",
            correctSequence = new int[] { 8, 1, 6 }
        });

        patternSteps.Add(new PatternStep
        {
            alphabetText = "R B H X T",
            correctSequence = new int[] { }
        });
    }

    public void ResetToFirstStep()
    {
        // 새로 생성된 퍼즐이어도 PuzzleManager에 기록이 있으면 복원
        if (HasBeenSolved())
        {
            RestoreClearedState();
            return;
        }

        StopAllCoroutines();
        activeTimerCoroutine = null;
        isShowingWrongFeedback = false;
        isFinalClearing = false;

        currentStepIndex = 0;
        playerSequence.Clear();

        StartStepSequence();
    }

    public void InitPuzzleState()
    {
        ResetToFirstStep();
    }

    private void StartStepSequence()
    {
        if (HasBeenSolved())
        {
            RestoreClearedState();
            return;
        }

        if (activeTimerCoroutine != null)
        {
            StopCoroutine(activeTimerCoroutine);
            activeTimerCoroutine = null;
        }

        if (currentStepIndex < patternSteps.Count)
        {
            displayAlphabetText.text =
                $"<size=90><cspace=-0.05em>{patternSteps[currentStepIndex].alphabetText}</cspace></size>";

            if (timerBarImage != null)
            {
                timerBarImage.fillAmount = 1f;
            }

            if (currentStepIndex != 0)
            {
                activeTimerCoroutine =
                    StartCoroutine(TimerAndHideRoutine(3.0f));
            }
        }
    }

    private IEnumerator TimerAndHideRoutine(float duration)
    {
        float elapsed = 0f;
        bool beep1PlayedAt1Sec = false;
        bool beep1PlayedAt2Sec = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (!beep1PlayedAt1Sec && elapsed >= 1f)
            {
                beep1PlayedAt1Sec = true;

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Beep1);
                }
            }

            if (!beep1PlayedAt2Sec && elapsed >= 2f)
            {
                beep1PlayedAt2Sec = true;

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Beep1);
                }
            }

            if (timerBarImage != null)
            {
                timerBarImage.fillAmount =
                    Mathf.Clamp01(1f - (elapsed / duration));
            }

            yield return null;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Beep2);
        }

        displayAlphabetText.text = "";

        if (timerBarImage != null)
        {
            timerBarImage.fillAmount = 0f;
        }

        activeTimerCoroutine = null;
    }

    private IEnumerator WrongAnswerFeedbackRoutine()
    {
        isShowingWrongFeedback = true;

        if (activeTimerCoroutine != null)
        {
            StopCoroutine(activeTimerCoroutine);
            activeTimerCoroutine = null;
        }

        displayAlphabetText.text = "<size=90>X</size>";

        if (timerBarImage != null)
        {
            timerBarImage.fillAmount = 0f;
        }

        yield return new WaitForSeconds(0.8f);

        playerSequence.Clear();
        isShowingWrongFeedback = false;

        displayAlphabetText.text = "";
    }

    public void OnShapeButtonClicked(int buttonIndex)
    {
        if (HasBeenSolved())
        {
            RestoreClearedState();
            return;
        }

        if (isShowingWrongFeedback || isFinalClearing)
        {
            return;
        }

        if (currentStepIndex < patternSteps.Count - 1)
        {
            int[] targetSequence =
                patternSteps[currentStepIndex].correctSequence;

            playerSequence.Add(buttonIndex);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
            }

            if (playerSequence.Count < targetSequence.Length)
            {
                return;
            }

            bool isCorrect = true;

            for (int i = 0; i < targetSequence.Length; i++)
            {
                if (playerSequence[i] != targetSequence[i])
                {
                    isCorrect = false;
                    break;
                }
            }

            if (isCorrect)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.CorrectBtn);
                }

                playerSequence.Clear();

                if (activeTimerCoroutine != null)
                {
                    StopCoroutine(activeTimerCoroutine);
                    activeTimerCoroutine = null;
                }

                currentStepIndex++;

                if (currentStepIndex >= patternSteps.Count - 1)
                {
                    OnPuzzleSuccess();
                }
                else
                {
                    StartStepSequence();
                }
            }
            else
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.WrongBtn);
                }

                playerSequence.Clear();

                StartCoroutine(WrongAnswerFeedbackRoutine());
            }
        }
    }

    private void OnPuzzleSuccess()
    {
        StopAllCoroutines();
        activeTimerCoroutine = null;
        isShowingWrongFeedback = false;
        isFinalClearing = true;

        StartCoroutine(FinalClearRoutine(3.0f));
    }

    private IEnumerator FinalClearRoutine(float duration)
    {
        displayAlphabetText.text =
            $"<size=90><cspace=-0.05em>{patternSteps[patternSteps.Count - 1].alphabetText}</cspace></size>";

        if (timerBarImage != null)
        {
            timerBarImage.fillAmount = 1f;
        }

        float elapsed = 0f;
        bool beep1PlayedAt1Sec = false;
        bool beep1PlayedAt2Sec = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (!beep1PlayedAt1Sec && elapsed >= 1f)
            {
                beep1PlayedAt1Sec = true;

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Beep1);
                }
            }

            if (!beep1PlayedAt2Sec && elapsed >= 2f)
            {
                beep1PlayedAt2Sec = true;

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Beep1);
                }
            }

            if (timerBarImage != null)
            {
                timerBarImage.fillAmount =
                    Mathf.Clamp01(1f - (elapsed / duration));
            }

            yield return null;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Beep2);
        }

        isCleared = true;
        isFinalClearing = false;

        displayAlphabetText.text = ClearMessage;

        if (timerBarImage != null)
        {
            timerBarImage.fillAmount = 0f;
        }

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(puzzleID, dimension);
        }
    }
}