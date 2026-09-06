using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ThreeDCommunicationPuzzle3D15 : MonoBehaviour, IInteractable, ICloseInspection
{
    private enum Phase
    {
        Closed,
        AlphabetInput,
        Cleared
    }

    [Header("Puzzle Data")]
    [SerializeField] private string puzzleId = "3D-15";
    [SerializeField] private PuzzleDimension dimension = PuzzleDimension.ThreeD;
    [Tooltip("정답")]
    [SerializeField] private string Answer = "SELF";

    [Header("Computer Inspection")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("모니터 정면에 빈 오브젝트를 만들고 카메라가 확대될 위치/회전으로 배치하세요.")]
    [SerializeField] private Transform cameraFocusPoint;
    [SerializeField] private Behaviour[] behavioursToDisable;

    [Header("UI - Common")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("UI - Alphabet Input")]
    [SerializeField] private TMP_InputField alphabetInput;

    private readonly PlayerControlLock playerControlLock = new PlayerControlLock();
    private Phase phase = Phase.Closed;
    private bool solved;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (instructionText != null) instructionText.gameObject.SetActive(true);
        if (alphabetInput != null) alphabetInput.gameObject.SetActive(true);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (phase == Phase.Closed || phase == Phase.Cleared)
            return;

        if ( Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitAlphabet();
            return;
        }
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

        // 커서 상태 저장,해제까지 Lock이 담당
        playerControlLock.Lock(this, behavioursToDisable);
        playerCamera.transform.SetPositionAndRotation(
            cameraFocusPoint.position,
            cameraFocusPoint.rotation);

        if (feedbackText != null) feedbackText.gameObject.SetActive(true);

        // CommonCanvas 뒤로가기 버튼
        if (InspectionUIController.Instance != null)
            InspectionUIController.Instance.Show(this);

        phase = Phase.AlphabetInput;

        if (alphabetInput != null)
        {
            alphabetInput.gameObject.SetActive(true);
            alphabetInput.text = "";
            alphabetInput.ActivateInputField();
        }

        SetFeedback("");
    }

    private void SubmitAlphabet()
    {
        if (phase != Phase.AlphabetInput)
            return;

        string entered = NormalizeAlphabet(
            alphabetInput != null ? alphabetInput.text : string.Empty);

        string answer = NormalizeAlphabet(Answer);

        if (entered == answer)
        {
            SetFeedback("정답입니다!");
            CompletePuzzle();
        }
        else
        {
            SetFeedback("정답이 아닙니다.");

            if (alphabetInput != null)
            {
                alphabetInput.text = "";
                alphabetInput.ActivateInputField();
            }
        }
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
        playerControlLock.Unlock();

        if (instructionText != null) instructionText.gameObject.SetActive(true);
        if (alphabetInput != null) alphabetInput.gameObject.SetActive(true);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        phase = solved ? Phase.Cleared : Phase.Closed;
    }

    private void CompletePuzzle()
    {
        solved = true;
        instructionText.text = "CLEAR!";
        instructionText.fontSize = 35f;
        
        if (alphabetInput != null) alphabetInput.gameObject.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.ReportSolved(puzzleId, dimension);
        else
            Debug.LogWarning("[ThreeDCommunicationPuzzle] PuzzleManager.Instance가 없습니다.", this);

        phase = Phase.Cleared;
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



    private void SetFeedback(string value)
    {
        if (feedbackText != null) feedbackText.text = value;
    }

}