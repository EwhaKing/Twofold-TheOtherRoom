using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ThreeDCommunicationPuzzle3D15 : MonoBehaviour, IInteractable
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
    [SerializeField] private GameObject backButton;

    [Header("UI - Alphabet Input")]
    [SerializeField] private TMP_InputField alphabetInput;

    private readonly List<Behaviour> disabledBehaviours = new List<Behaviour>();
    private Phase phase = Phase.Closed;
    private bool solved;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (instructionText != null) instructionText.gameObject.SetActive(true);
        if (alphabetInput != null) alphabetInput.gameObject.SetActive(true);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (backButton != null) backButton.SetActive(false);

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
        originalCursorLockMode = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        DisablePlayerControl();
        playerCamera.transform.SetPositionAndRotation(
            cameraFocusPoint.position,
            cameraFocusPoint.rotation);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (feedbackText != null) feedbackText.gameObject.SetActive(true);
        if (backButton != null) backButton.SetActive(true);

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

        if (instructionText != null) instructionText.gameObject.SetActive(true);
        if (alphabetInput != null) alphabetInput.gameObject.SetActive(true);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (backButton != null) backButton.SetActive(true);

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

}