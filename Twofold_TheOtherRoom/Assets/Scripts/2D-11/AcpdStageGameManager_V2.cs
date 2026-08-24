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

    // [리셋 버튼용] 무조건 처음으로 초기화
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
            if (timerBarImage != null)
            {
                timerBarImage.fillAmount = 1f;
            }

            displayAlphabetText.text = patternSteps[currentStepIndex].alphabetText;
            activeTimerCoroutine = StartCoroutine(TimerAndHideRoutine(3.0f));
        }
    }

    private IEnumerator TimerAndHideRoutine(float duration)
    {
        float elapsed = 0f;
        // 1초, 2초 Beep1 재생 여부
        bool beep1PlayedAt1Sec = false; 
        bool beep1PlayedAt2Sec = false;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            //1초
            if (!beep1PlayedAt1Sec && elapsed >= 1f)
            {
                beep1PlayedAt1Sec = true;

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFXType.Beep1);
                }
            }

            //2초
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

        SoundManager.Instance.PlaySFX(SFXType.Beep2);

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

        displayAlphabetText.text = "X";
        if (timerBarImage != null) timerBarImage.fillAmount = 0f;

        yield return new WaitForSeconds(0.8f);

        subStepIndex = 0;
        isShowingWrongFeedback = false;
        StartStepSequence();
    }

    public void OnShapeButtonClicked(int buttonIndex)
    {
        if (isShowingWrongFeedback) return; 

        if (currentStepIndex < patternSteps.Count - 1)
        {
            int[] targetSequence = patternSteps[currentStepIndex].correctSequence;

            if (buttonIndex == targetSequence[subStepIndex])
            {
                SoundManager.Instance.PlaySFX(SFXType.CorrectBtn);
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
                SoundManager.Instance.PlaySFX(SFXType.WrongBtn);
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

    // 🌟 최종 연출: size=52로 크기 업 + voffset으로 살짝 아래 배치
    private IEnumerator FinalClearRoutine(float duration)
    {
        displayAlphabetText.text = patternSteps[patternSteps.Count - 1].alphabetText;

        if (timerBarImage != null)
        {
            timerBarImage.fillAmount = 1f;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (timerBarImage != null)
            {
                timerBarImage.fillAmount = Mathf.Clamp01(1f - (elapsed / duration));
            }

            yield return null;
        }

        if (timerBarImage != null)
        {
            timerBarImage.fillAmount = 0f;
        }

        // 🌟 size=52 (크기 약간 확대) / voffset=-10em (아래로 위치 이동)
        displayAlphabetText.text = "<size=44><voffset=-0.5em>3D 거울 획득 확인</voffset></size>";

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(puzzleID, dimension);
        }
    }
}