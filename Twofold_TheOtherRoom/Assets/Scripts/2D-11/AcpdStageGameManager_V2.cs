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
    private bool isShowingWrongFeedback = false; // 틀림 연출 중 클릭 방지

    private Coroutine activeTimerCoroutine;

    private void Awake()
    {
        // 1. 9개 도형 버튼 이벤트 미리 연결
        for (int i = 0; i < shapeButtons.Length; i++)
        {
            int index = i;
            if (shapeButtons[i] != null)
            {
                shapeButtons[i].onClick.RemoveAllListeners();
                shapeButtons[i].onClick.AddListener(() => OnShapeButtonClicked(index));
            }
        }

        // 2. 리셋 버튼 연결
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetToFirstStep);
        }

        // 3. 정답 패턴 세팅
        SetupExactPhotoPatterns();
    }

    // 🌟 외부 연결 없이도 이 퍼즐이 켜질 때마다 즉시 1단계 타이머 자동 시작!
    private void OnEnable()
    {
        ResetToFirstStep();
    }

    // 📸 정답 패턴 세팅
    private void SetupExactPhotoPatterns()
    {
        patternSteps.Clear();

        // 1단계: "A C P" -> 대각선 네모(5)
        patternSteps.Add(new PatternStep { 
            alphabetText = "A C P", 
            correctSequence = new int[] { 5 } 
        });

        // 2단계: "Z L O" -> 오른쪽 차있는 반원(3) -> 직각세모(7)
        patternSteps.Add(new PatternStep { 
            alphabetText = "Z L O", 
            correctSequence = new int[] { 3, 7 } 
        });

        // 3단계: "W P M D" -> 오른쪽 아래 채워진 네모(8) -> 세모(1) -> 왼쪽 맨 아래 반원(6)
        patternSteps.Add(new PatternStep { 
            alphabetText = "W P M D", 
            correctSequence = new int[] { 8, 1, 6 } 
        });

        // 4단계: "R B H X T" (최종 클리어 알파벳)
        patternSteps.Add(new PatternStep { 
            alphabetText = "R B H X T", 
            correctSequence = new int[] { } 
        });
    }

    // 💥 [리셋 버튼용] 무조건 처음(1단계 A C P)으로 전체 초기화
    public void ResetToFirstStep()
    {
        StopAllCoroutines();
        activeTimerCoroutine = null;
        isShowingWrongFeedback = false;
        currentStepIndex = 0;
        subStepIndex = 0;

        StartStepSequence();
    }

    // 🌟 줌 연출 완료 시 퍼즐 시작 로직
    public void InitPuzzleState()
    {
        ResetToFirstStep();
    }

    // 🌟 단계 시작: 알파벳 표시 + 타임바 리셋 + 3초 타임바 연출
    private void StartStepSequence()
    {
        if (activeTimerCoroutine != null)
        {
            StopCoroutine(activeTimerCoroutine);
            activeTimerCoroutine = null;
        }

        if (currentStepIndex < patternSteps.Count)
        {
            // 타임바 게이지를 1f(100%)로 채운 후 시작
            if (timerBarImage != null)
            {
                timerBarImage.fillAmount = 1f;
            }

            displayAlphabetText.text = patternSteps[currentStepIndex].alphabetText;
            activeTimerCoroutine = StartCoroutine(TimerAndHideRoutine(3.0f));
        }
    }

    // 🌟 3초 동안 바가 줄어들고 글자만 사라짐
    private IEnumerator TimerAndHideRoutine(float duration)
    {
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

        // 3초 후 글자와 타임바만 끄고 대기
        displayAlphabetText.text = "";
        if (timerBarImage != null)
        {
            timerBarImage.fillAmount = 0f;
        }

        activeTimerCoroutine = null;
    }

    // ❌ 오답 반응 연출
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

    // 🎯 도형 버튼 클릭 처리
    public void OnShapeButtonClicked(int buttonIndex)
    {
        if (isShowingWrongFeedback) return; 

        if (currentStepIndex < patternSteps.Count - 1)
        {
            int[] targetSequence = patternSteps[currentStepIndex].correctSequence;

            if (buttonIndex == targetSequence[subStepIndex])
            {
                Debug.Log($"⭕ [{currentStepIndex + 1}단계] {subStepIndex + 1}번째 도형 성공!");
                subStepIndex++;

                if (subStepIndex >= targetSequence.Length)
                {
                    // 해당 단계를 완전히 클리어했을 때 잔여 타이머 멈춤
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
                Debug.Log($"❌ [{currentStepIndex + 1}단계] 틀린 도형 클릭!");
                StartCoroutine(WrongAnswerFeedbackRoutine()); 
            }
        }
    }

    private void OnPuzzleSuccess()
    {
        StopAllCoroutines();
        activeTimerCoroutine = null;
        isShowingWrongFeedback = false;

        displayAlphabetText.text = patternSteps[patternSteps.Count - 1].alphabetText; 

        if (timerBarImage != null)
        {
            timerBarImage.fillAmount = 1f;
        }

        Debug.Log("🎉 Stage 2D-11 퍼즐 최종 성공!");

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(puzzleID, dimension);
        }
    }
}