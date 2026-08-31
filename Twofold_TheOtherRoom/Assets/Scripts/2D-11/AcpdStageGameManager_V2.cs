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
    private int subStepIndex = 0;        
    private bool isShowingWrongFeedback = false;

    private Coroutine activeTimerCoroutine;

    private void Awake()
    {
        for (int i = 0; i < shapeButtons.Length; i++)
        {
            int index = i;
            if (shapeButtons[i] != null)
            {
                shapeButtons[i].onClick.RemoveAllListeners();
                shapeButtons[i].onClick.AddListener(() => OnShapeButtonClicked(index));
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
        ResetToFirstStep();
    }

    private void SetupExactPhotoPatterns()
    {
        patternSteps.Clear();

        patternSteps.Add(new PatternStep { 
            alphabetText = "A C P", 
            correctSequence = new int[] { 5 } 
        });

        patternSteps.Add(new PatternStep { 
            alphabetText = "Z L O", 
            correctSequence = new int[] { 3, 7 } 
        });

        patternSteps.Add(new PatternStep { 
            alphabetText = "W P M D", 
            correctSequence = new int[] { 8, 1, 6 } 
        });

        patternSteps.Add(new PatternStep { 
            alphabetText = "R B H X T", 
            correctSequence = new int[] { } 
        });
    }

    public void ResetToFirstStep()
    {
        StopAllCoroutines();
        activeTimerCoroutine = null;
        isShowingWrongFeedback = false;
        currentStepIndex = 0;
        subStepIndex = 0;

        StartStepSequence();
    }

    public void InitPuzzleState()
    {
        ResetToFirstStep();
    }

    private void StartStepSequence()
    {
        if (activeTimerCoroutine != null)
        {
            StopCoroutine(activeTimerCoroutine);
            activeTimerCoroutine = null;
        }

        if (currentStepIndex < patternSteps.Count)
        {
            displayAlphabetText.text = $"<size=90><cspace=-0.05em>{patternSteps[currentStepIndex].alphabetText}</cspace></size>";

            if (currentStepIndex == 0)
            {
                if (timerBarImage != null)
                {
                    timerBarImage.fillAmount = 1f;
                }
            }
            else
            {
                if (timerBarImage != null)
                {
                    timerBarImage.fillAmount = 1f;
                }

                activeTimerCoroutine = StartCoroutine(TimerAndHideRoutine(3.0f));
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
                timerBarImage.fillAmount = Mathf.Clamp01(1f - (elapsed / duration));
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
        if (timerBarImage != null) timerBarImage.fillAmount = 0f;

        yield return new WaitForSeconds(0.8f);

        subStepIndex = 0;
        isShowingWrongFeedback = false;

        displayAlphabetText.text = "";
    }

    public void OnShapeButtonClicked(int buttonIndex)
    {
        if (isShowingWrongFeedback) return; 

        if (currentStepIndex < patternSteps.Count - 1)
        {
            int[] targetSequence = patternSteps[currentStepIndex].correctSequence;

            if (buttonIndex == targetSequence[subStepIndex])
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.CorrectBtn);
                }

                subStepIndex++;

                if (subStepIndex >= targetSequence.Length)
                {
                    if (activeTimerCoroutine != null)
                    {
                        StopCoroutine(activeTimerCoroutine);
                        activeTimerCoroutine = null;
                    }

                    currentStepIndex++;
                    subStepIndex = 0;

                    if (currentStepIndex >= patternSteps.Count - 1)
                    {
                        OnPuzzleSuccess();
                    }
                    else
                    {
                        StartStepSequence(); 
                    }
                }
            }
            else
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.WrongBtn);
                }

                StartCoroutine(WrongAnswerFeedbackRoutine()); 
            }
        }
    }

    private void OnPuzzleSuccess()
    {
        StopAllCoroutines();
        activeTimerCoroutine = null;
        isShowingWrongFeedback = false;

        StartCoroutine(FinalClearRoutine(3.0f));
    }

    private IEnumerator FinalClearRoutine(float duration)
    {
        displayAlphabetText.text = $"<size=90><cspace=-0.05em>{patternSteps[patternSteps.Count - 1].alphabetText}</cspace></size>";

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
                timerBarImage.fillAmount = Mathf.Clamp01(1f - (elapsed / duration));
            }

            yield return null;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Beep2);
        }

        if (timerBarImage != null)
        {
            timerBarImage.fillAmount = 0f;
        }

        // 🌟 코드상에서만 위치 하향(voffset=-0.25em) 및 크기 확대(size=68) 적용
        displayAlphabetText.text = "<size=62><cspace=-0.03em><voffset=-0.25em>3D 거울 획득 확인</voffset></cspace></size>";

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(puzzleID, dimension);
        }
    }
}