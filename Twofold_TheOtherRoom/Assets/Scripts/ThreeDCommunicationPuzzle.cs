using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// 3D 플레이어용 통신 퍼즐.
/// 알파벳을 순서대로 입력하면 다음 단계의 도형이 3초 동안 표시됩니다.

public class ThreeDCommunicationPuzzle : MonoBehaviour, IInteractable
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
    [SerializeField] private string puzzleId = "3D-4";
    [SerializeField] private PuzzleDimension dimension = PuzzleDimension.ThreeD;
    [Tooltip("맨 처음 입력할 알파벳 3글자. 이 입력에는 시간제한이 없습니다.")]
    [SerializeField] private string introAlphabetAnswer = "ACF";
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
    [SerializeField] private GameObject backButton;

    [Header("UI - Alphabet Input")]
    [SerializeField] private GameObject alphabetInputPanel;
    [SerializeField] private TMP_InputField alphabetInput;

    [Header("UI - Shape Reveal")]
    [SerializeField] private GameObject shapeRevealPanel;
    [Tooltip("도형 Image 슬롯 3개를 연결하세요.")]
    [SerializeField] private Image[] shapeImageSlots;
    [Tooltip("도형 표시 시간이 3초에서 0초로 줄어드는 Slider입니다.")]
    [SerializeField] private Slider timerSlider;
    [Tooltip("선택 사항입니다. 비워 두면 숫자는 표시하지 않고 Slider만 줄어듭니다.")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField, Min(0.1f)] private float shapeRevealSeconds = 3f;

    private readonly List<Behaviour> disabledBehaviours = new List<Behaviour>();
    private Phase phase = Phase.Closed;
    private int currentStageIndex = -1;
    private float revealTimeLeft;
    private bool solved;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

    }

    private void Update()
    {
        if (phase == Phase.Closed || phase == Phase.Cleared)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePuzzle();
            return;
        }

        if (phase == Phase.AlphabetInput &&
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
    }

    private void OpenPuzzle()
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
        originalCursorLockMode = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        DisablePlayerControl();
        playerCamera.transform.SetPositionAndRotation(
            cameraFocusPoint.position,
            cameraFocusPoint.rotation);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (backButton != null) backButton.SetActive(true);
        if (resetButton != null) resetButton.SetActive(true);

        RestartFromBeginning();
    }


    /// Enter를 눌렀을 때 알파벳을 순서대로 검사합니다.
    private void SubmitAlphabet()
    {
        if (phase != Phase.AlphabetInput)
            return;

        string entered = NormalizeAlphabet(alphabetInput != null ? alphabetInput.text : string.Empty);
        string expected = CurrentExpectedAlphabet();

        if (!string.Equals(entered, expected, StringComparison.Ordinal))
        {
            SetFeedback("정답이 아닙니다. 알파벳 순서를 확인하세요.");
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

        ShowShapes(currentStageIndex + 1);
    }

    //Reset Button의 OnClick에 연결합니다. 최초 알파벳 입력부터 다시 시작합니다.
    public void ResetPuzzle()
    {
        if (phase != Phase.Closed && phase != Phase.Cleared)
            RestartFromBeginning();
    }

    ///Back Button의 OnClick에 연결합니다. E를 누르기 전 상태로 돌아갑니다.
    public void ClosePuzzle()
    {
        if (phase == Phase.Closed)
            return;

        playerCamera.transform.SetPositionAndRotation(
            originalCameraPosition,
            originalCameraRotation);
        RestorePlayerControl();

        Cursor.lockState = originalCursorLockMode;
        Cursor.visible = originalCursorVisible;

        phase = solved ? Phase.Cleared : Phase.Closed;
    }

    private void RestartFromBeginning()
    {
        currentStageIndex = -1; // -1은 최초 알파벳 입력 화면
        revealTimeLeft = 0f;
        ShowAlphabetInput();
    }

    private void ShowAlphabetInput()
    {
        phase = Phase.AlphabetInput;
        if (alphabetInputPanel != null) alphabetInputPanel.SetActive(true);
        if (shapeRevealPanel != null) shapeRevealPanel.SetActive(false);

        if (currentStageIndex < 0)
        {
            SetStageLabel("시작");
            if (instructionText != null)
                instructionText.text = "상대가 알려준 첫 알파벳을 순서대로 입력하세요. (시간제한 없음)";
        }
        else
        {
            SetStageLabel($"{currentStageIndex + 1}단계");
            if (instructionText != null)
                instructionText.text = "상대가 알려준 알파벳을 순서대로 입력하세요.";
        }

        SetFeedback(string.Empty);
        if (alphabetInput != null)
        {
            alphabetInput.text = string.Empty;
            alphabetInput.characterLimit = CurrentExpectedAlphabet().Length;
            alphabetInput.ActivateInputField();
        }
    }

    private void ShowShapes(int stageIndex)
    {
        if (!HasValidStage(stageIndex))
        {
            Debug.LogWarning($"[ThreeDCommunicationPuzzle] {stageIndex + 1}단계 데이터가 없습니다.", this);
            return;
        }

        currentStageIndex = stageIndex;
        phase = Phase.ShapeReveal;
        revealTimeLeft = shapeRevealSeconds;

        if (alphabetInputPanel != null) alphabetInputPanel.SetActive(false);
        if (shapeRevealPanel != null) shapeRevealPanel.SetActive(true);
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
        ShowAlphabetInput();
    }

    private void CompletePuzzle()
    {
        solved = true;
        SetFeedback("CLEAR!");

        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.ReportSolved(puzzleId, dimension);
        else
            Debug.LogWarning("[ThreeDCommunicationPuzzle] PuzzleManager.Instance가 없습니다.", this);

        phase = Phase.Cleared;
        ClosePuzzle();
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
            timerSlider.maxValue = shapeRevealSeconds;
            timerSlider.value = time;
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

    private void DisablePlayerControl()
    {
        disabledBehaviours.Clear();
        if (behavioursToDisable == null || behavioursToDisable.Length == 0)
        {
            TryDisable(FindAnyObjectByType<PlayerController>());
            TryDisable(FindAnyObjectByType<PlayerLocomotionInput>());
            TryDisable(FindAnyObjectByType<PlayerInteractor>());
            return;
        }

        foreach (Behaviour behaviour in behavioursToDisable)
            TryDisable(behaviour);
    }

    private void TryDisable(Behaviour behaviour)
    {
        if (behaviour == null || behaviour == this || !behaviour.enabled)
            return;

        behaviour.enabled = false;
        disabledBehaviours.Add(behaviour);
    }

    private void RestorePlayerControl()
    {
        foreach (Behaviour behaviour in disabledBehaviours)
        {
            if (behaviour != null)
                behaviour.enabled = true;
        }
        disabledBehaviours.Clear();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (stages == null || stages.Length != 3)
            Array.Resize(ref stages, 3);
    }
#endif
}
