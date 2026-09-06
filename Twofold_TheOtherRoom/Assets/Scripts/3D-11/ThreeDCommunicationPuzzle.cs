using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// 3D 플레이어용 통신 퍼즐.
/// 알파벳을 순서대로 입력하면 다음 단계의 도형이 3초 동안 표시됩니다.

public class ThreeDCommunicationPuzzle : MonoBehaviour, IInteractable, ICloseInspection
{
    [Serializable]
    public class StageData
    {
        [Tooltip("이 단계에서 3초 동안 보여줄 도형들. 1/2/3단계에 각각 1/2/3개를 넣으세요.")]
        public Sprite[] shapes;

        [Tooltip("도형이 사라진 뒤 3D 플레이어가 순서대로 입력할 알파벳 정답")]
        public string nextAlphabetAnswer;
    }

    private enum Phase
    {
        Closed,
        AlphabetInput,
        ShapeReveal,
        Cleared
    }

    [Header("Puzzle Data")]
    [SerializeField] private string puzzleId = "3D-11";
    [SerializeField] private PuzzleDimension dimension = PuzzleDimension.ThreeD;
    [Tooltip("맨 처음 입력할 알파벳 3글자. 이 입력에는 시간제한이 없습니다.")]
    [SerializeField] private string introAlphabetAnswer = "ACP";
    [Tooltip("Size는 3입니다. 단계별 도형과 그 뒤에 입력할 알파벳을 한 묶음으로 설정합니다.")]
    [SerializeField] private StageData[] stages = new StageData[3];

    [Header("Computer Inspection")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("모니터 정면에 빈 오브젝트를 만들고 카메라가 확대될 위치/회전으로 배치하세요.")]
    [SerializeField] private Transform cameraFocusPoint;
    [SerializeField] private Behaviour[] behavioursToDisable;

    [Header("UI - Common")]
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private GameObject resetButton;

    [Header("UI - Alphabet Input")]
    [SerializeField] private TMP_InputField alphabetInput;

    [Header("UI - Shape Reveal")]
    [Tooltip("도형 Image 슬롯 3개를 연결하세요.")]
    [SerializeField] private Image[] shapeImageSlots;
    [Tooltip("도형 표시 시간이 3초에서 0초로 줄어드는 Slider입니다.")]
    [SerializeField] private Slider timerSlider;
    [Tooltip("선택 사항입니다. 비워 두면 숫자는 표시하지 않고 Slider만 줄어듭니다.")]
    [SerializeField] private TMP_Text timerText;

    [Header("UI - ClearPanel")]
    [SerializeField] private GameObject MirrorPanel;
  

    private readonly PlayerControlLock playerControlLock = new PlayerControlLock();
    private Phase phase = Phase.Closed;
    private int currentStageIndex = -1;
    private float revealTimeLeft;
    private bool solved;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Coroutine beepRoutine;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (instructionText != null) instructionText.gameObject.SetActive(true);
        if (alphabetInput != null) alphabetInput.gameObject.SetActive(true);
        if (stageText != null) stageText.gameObject.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(true);
        if (timerSlider != null) timerSlider.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (resetButton != null) resetButton.SetActive(false);
         if (MirrorPanel != null) MirrorPanel.SetActive(false);


        HideAllShapeSlots();
    }

    private void Update()
    {
        if (phase == Phase.Closed ||phase == Phase.Cleared)
            return;

        if ((phase == Phase.AlphabetInput || phase == Phase.ShapeReveal) &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            SubmitAlphabet();
            return;
        }

        if (phase != Phase.ShapeReveal)
            return;

        revealTimeLeft -= Time.unscaledDeltaTime;
        UpdateTimerUI();

        if (revealTimeLeft <= 0f)
            FinishShapeReveal();
    }

    public void Interact()
    {
        if (phase == Phase.Closed && !solved)
            OpenPuzzle();

        if (phase == Phase.Cleared && solved)
            BasicCameraControl();
    }

    private void BasicCameraControl()
    {
        
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null || cameraFocusPoint == null)
        {
            Debug.LogWarning("[ThreeDCommunicationPuzzle] Player Camera와 Camera Focus Point를 연결하세요.", this);
            return;
        }

        originalCameraPosition = playerCamera.transform.position;
        originalCameraRotation = playerCamera.transform.rotation;

        playerControlLock.Lock(this, behavioursToDisable, true);

        playerCamera.transform.SetPositionAndRotation(
            cameraFocusPoint.position,
            cameraFocusPoint.rotation);

        if (InspectionUIController.Instance != null)
            InspectionUIController.Instance.Show(this);
    }

    private void OpenPuzzle()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
        }

        BasicCameraControl();

        RestartFromBeginning();
        StartCoroutine(ActivateAlphabetInputAfterInteractKeyReleased());
    }

    private IEnumerator ActivateAlphabetInputAfterInteractKeyReleased()
    {
        if (alphabetInput == null)
            yield break;

        // 퍼즐을 연 E 입력이 같은 프레임에 InputField 문자로 들어가는 것을 막습니다.
        alphabetInput.DeactivateInputField();
        while (Input.GetKey(KeyCode.E))
            yield return null;
        yield return null;

        if (phase == Phase.AlphabetInput)
        {
            alphabetInput.text = string.Empty;
            alphabetInput.ActivateInputField();
        }
    }


    /// Enter를 눌렀을 때 알파벳을 순서대로 검사합니다.
    private void SubmitAlphabet()
    {
        if (phase != Phase.AlphabetInput && phase != Phase.ShapeReveal)
            return;

        string entered = NormalizeAlphabet(alphabetInput != null ? alphabetInput.text : string.Empty);
        string expected = CurrentExpectedAlphabet();

        if (!string.Equals(entered, expected, StringComparison.Ordinal))
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.WrongBtn);
            }
            SetFeedback("정답이 아닙니다.");
            if (alphabetInput != null)
            {
                alphabetInput.text = string.Empty;
                alphabetInput.ActivateInputField();
            }
            return;
        }
        SetFeedback("정답입니다.");

        // 마지막(3단계 뒤) 알파벳까지 맞히면 7번째 화면 완료 후 Clear.
        if (currentStageIndex == stages.Length - 1)
        {
            CompletePuzzle();
            return;
        }

        StartStage(currentStageIndex + 1);
    }

    //Reset Button의 OnClick에 연결합니다. 최초 알파벳 입력부터 다시 시작합니다.
    public void ResetPuzzle()
    {
        if (phase != Phase.Closed && phase != Phase.Cleared)
            RestartFromBeginning();
    }

    /// CommonCanvas 뒤로가기 버튼이 부름. E를 누르기 전 상태로 돌아감.
    public void CloseInspection()
    {
        if (phase == Phase.Closed)
            return;

        if (InspectionUIController.Instance != null)
            InspectionUIController.Instance.Hide(this);

        playerCamera.transform.SetPositionAndRotation(
            originalCameraPosition,
            originalCameraRotation);
         //playerControlunLock
        playerControlLock.Unlock();

        if (instructionText != null) instructionText.gameObject.SetActive(true);
        if (alphabetInput != null) alphabetInput.gameObject.SetActive(true);
        if (stageText != null) stageText.gameObject.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (timerSlider != null) timerSlider.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (resetButton != null) resetButton.SetActive(false);
        HideAllShapeSlots();

        phase = solved ? Phase.Cleared : Phase.Closed;
    }

    private void RestartFromBeginning()
    {
        currentStageIndex = -1;
        revealTimeLeft = 0f;
        phase = Phase.AlphabetInput;

        HideAllShapeSlots();
        if (timerSlider != null) timerSlider.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (resetButton != null) resetButton.SetActive(false);
        if (alphabetInput != null) alphabetInput.gameObject.SetActive(true);

        SetStageLabel(string.Empty);
        if (instructionText != null)
            instructionText.text = "상대를 통해 알파벳 3자리를 입력하세요";
        SetFeedback(string.Empty);
        if (alphabetInput != null)
        {
            alphabetInput.text = string.Empty;
            alphabetInput.characterLimit = 8;
        }
    }

    private void StartStage(int stageIndex)
    {
        if (!HasValidStage(stageIndex))
        {
            Debug.LogWarning($"[ThreeDCommunicationPuzzle] {stageIndex + 1}단계 데이터가 없습니다.", this);
            return;
        }
        if (alphabetInput != null)
            alphabetInput.text = string.Empty;

        currentStageIndex = stageIndex;
        phase = Phase.ShapeReveal;
        revealTimeLeft = 3f;

        if (beepRoutine != null)
            StopCoroutine(beepRoutine);

        beepRoutine = StartCoroutine(PlayCountdownBeep());

        if (alphabetInput != null)
        {
            alphabetInput.characterLimit = 8;
            alphabetInput.ActivateInputField();
        }

        if (timerSlider != null) timerSlider.gameObject.SetActive(true);
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (resetButton != null) resetButton.SetActive(true);
        if (stageText != null) stageText.gameObject.SetActive(true);
        if (feedbackText != null) feedbackText.gameObject.SetActive(true);
        SetStageLabel($"{stageIndex + 1}단계");
        SetFeedback(string.Empty);
        if (instructionText != null)
            instructionText.text = "3초 안에 도형을 기억해서 상대에게 알려주세요.";

        PopulateShapeImages(stages[stageIndex]);
        UpdateTimerUI();
    }

    private void FinishShapeReveal()
    {
        revealTimeLeft = 0f;
        HideAllShapeSlots();
        phase = Phase.AlphabetInput;



        if (instructionText != null)
            instructionText.text = "상대가 알려준 알파벳을 순서대로 입력하세요.";
        SetFeedback(string.Empty);
        if (alphabetInput != null)
        {
            alphabetInput.characterLimit = 8;
            alphabetInput.ActivateInputField();
        }
    }

    private void CompletePuzzle()
    {
        solved = true;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.SteppingCorrect);
        }
        if (alphabetInput != null) alphabetInput.gameObject.SetActive(false);
        if (stageText != null) stageText.gameObject.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (timerSlider != null) timerSlider.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (resetButton != null) resetButton.SetActive(false);
        if (instructionText != null) instructionText.gameObject.SetActive(false);


        if (MirrorPanel != null) MirrorPanel.gameObject.SetActive(true);


/**
        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.ReportSolved(puzzleId, dimension);
        else
            Debug.LogWarning("[ThreeDCommunicationPuzzle] PuzzleManager.Instance가 없습니다.", this);
**/
        phase = Phase.Cleared;
    }

    private string CurrentExpectedAlphabet()
    {
        if (currentStageIndex < 0)
            return NormalizeAlphabet(introAlphabetAnswer);

        return HasValidStage(currentStageIndex)
            ? NormalizeAlphabet(stages[currentStageIndex].nextAlphabetAnswer)
            : string.Empty;
    }

    private void PopulateShapeImages(StageData stage)
    {
        if (shapeImageSlots == null)
            return;

        for (int i = 0; i < shapeImageSlots.Length; i++)
        {
            Image slot = shapeImageSlots[i];
            if (slot == null) continue;

            bool hasSprite = stage.shapes != null &&
                             i < stage.shapes.Length &&
                             stage.shapes[i] != null;
            slot.gameObject.SetActive(hasSprite);
            if (hasSprite)
                slot.sprite = stage.shapes[i];
        }
    }

    private void HideAllShapeSlots()
    {
        if (shapeImageSlots == null)
            return;

        foreach (Image slot in shapeImageSlots)
        {
            if (slot != null)
                slot.gameObject.SetActive(false);
        }
    }

    private void UpdateTimerUI()
    {
        float time = Mathf.Max(0f, revealTimeLeft);
        if (timerSlider != null)
        {
           

            timerSlider.minValue = 0f;
            timerSlider.maxValue = 3f;
            timerSlider.value = time;

            if (timerSlider.fillRect != null)
            {
                timerSlider.fillRect.gameObject.SetActive(time > 0.001f);
            }
        }
        if (timerText != null)
            timerText.text = $"{time:0.0}";
    }

    private static string NormalizeAlphabet(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var result = new StringBuilder();
        foreach (char character in value)
        {
            if (char.IsLetter(character))
                result.Append(char.ToUpperInvariant(character));
        }
        return result.ToString();
    }

    private bool HasValidStage(int index)
    {
        return stages != null && index >= 0 && index < stages.Length && stages[index] != null;
    }

    private void SetStageLabel(string value)
    {
        if (stageText != null) stageText.text = value;
       
    }

    private void SetFeedback(string value)
    {
        if (feedbackText != null) feedbackText.text = value;
         
    }

    private IEnumerator PlayCountdownBeep()
    {
        // 3.0초
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.CorrectBtn);
        }

        yield return new WaitForSecondsRealtime(1f);

        // 2.0초
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Beep1);
        }

        yield return new WaitForSecondsRealtime(1f);

        // 1.0초
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Beep1);
        }

        yield return new WaitForSecondsRealtime(1f);

        // 0.0초
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Beep2);
        }

        beepRoutine = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (stages == null || stages.Length != 3)
            Array.Resize(ref stages, 3);
    }
#endif
}
