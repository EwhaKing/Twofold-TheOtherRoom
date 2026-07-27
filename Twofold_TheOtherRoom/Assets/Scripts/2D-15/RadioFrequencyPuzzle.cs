using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadioFrequencyPuzzle :
    MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("퍼즐 정보")]
    public string puzzleId = "2D-15";
    public PuzzleDimension dimension = PuzzleDimension.TwoD;

    [Header("UI 연결")]
    [SerializeField] private RectTransform dialRect;
    [SerializeField] private TMP_Text frequencyText;
    [SerializeField] private Button resetButton;

    [Header("주파수 설정")]
    [SerializeField] private float minimumFrequency = 0f;
    [SerializeField] private float maximumFrequency = 180f;

    // 주파수 표시 단위
    [SerializeField] private float frequencyStep = 0.1f;

    // 다이얼 감도
    // 값이 작을수록 천천히 변함
    [SerializeField] private float frequencyPerDegree = 0.03f;

    [Header("정답 주파수")]
    [SerializeField]
    private float[] targetFrequencies =
    {
        171.2f,
        160.1f,
        163.8f,
        175.5f
    };

    [Header("알파벳 소리")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] alphabetSounds;

    private float currentFrequency;
    private float previousMouseAngle;
    private float visualDialAngle;

    private int currentTargetIndex;
    private bool isDragging;
    private bool isPuzzleComplete;

    private void Start()
    {
        // Dial Rect가 연결되지 않았을 경우
        // 이 스크립트가 붙은 오브젝트를 자동 사용
        if (dialRect == null)
        {
            dialRect = transform as RectTransform;
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetPuzzle);
        }

        ResetPuzzle();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPuzzleComplete)
        {
            return;
        }

        isDragging = true;
        previousMouseAngle = GetMouseAngle(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isPuzzleComplete)
        {
            return;
        }

        float currentMouseAngle = GetMouseAngle(eventData);

        // 359도에서 0도로 넘어가도 정상적으로 각도 차이를 계산
        float angleDifference = Mathf.DeltaAngle(
            previousMouseAngle,
            currentMouseAngle
        );

        previousMouseAngle = currentMouseAngle;

        RotateDial(angleDifference);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;

        // 마우스를 놓았을 때만 정답 판정
        CheckCorrectFrequency();
    }

    private float GetMouseAngle(PointerEventData eventData)
    {
        Camera eventCamera = eventData.pressEventCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dialRect,
            eventData.position,
            eventCamera,
            out Vector2 localMousePosition
        );

        return Mathf.Atan2(
            localMousePosition.y,
            localMousePosition.x
        ) * Mathf.Rad2Deg;
    }

    private void RotateDial(float angleDifference)
    {
        // 시계 방향 회전 시 주파수 증가
        float frequencyChange =
            -angleDifference * frequencyPerDegree;

        currentFrequency += frequencyChange;

        currentFrequency = Mathf.Clamp(
            currentFrequency,
            minimumFrequency,
            maximumFrequency
        );

        // 0.1MHz 단위로 맞춤
        currentFrequency = Mathf.Round(
            currentFrequency / frequencyStep
        ) * frequencyStep;

        currentFrequency = RoundToOneDecimal(
            currentFrequency
        );

        // 다이얼 이미지 회전
        visualDialAngle -= angleDifference;

        if (dialRect != null)
        {
            dialRect.localEulerAngles = new Vector3(
                0f,
                0f,
                visualDialAngle
            );
        }

        UpdateFrequencyText();

        // 여기서는 정답 판정을 하지 않음
        // 마우스를 놓았을 때만 검사함
    }

    private void CheckCorrectFrequency()
    {
        if (isPuzzleComplete)
        {
            return;
        }

        if (currentTargetIndex >= targetFrequencies.Length)
        {
            return;
        }

        float expectedFrequency =
            RoundToOneDecimal(
                targetFrequencies[currentTargetIndex]
            );

        if (Mathf.Approximately(
            currentFrequency,
            expectedFrequency
        ))
        {
            Debug.Log(
                $"{currentTargetIndex + 1}번째 주파수 정답: " +
                $"{expectedFrequency:F1} MHz"
            );

            PlayAlphabetSound(currentTargetIndex);

            currentTargetIndex++;

            if (currentTargetIndex >= targetFrequencies.Length)
            {
                CompletePuzzle();
            }
            else
            {
                Debug.Log(
                    $"다음 단계로 진행합니다. " +
                    $"현재 진행도: {currentTargetIndex} / " +
                    $"{targetFrequencies.Length}"
                );
            }
        }
        else
        {
            Debug.Log(
                $"현재 주파수 {currentFrequency:F1} MHz는 " +
                $"{currentTargetIndex + 1}번째 정답이 아닙니다."
            );
        }
    }

    private void PlayAlphabetSound(int soundIndex)
    {
        // 음성 파일이 아직 없어도 오류가 발생하지 않음
        if (audioSource == null)
        {
            return;
        }

        if (alphabetSounds == null)
        {
            return;
        }

        if (
            soundIndex < 0 ||
            soundIndex >= alphabetSounds.Length
        )
        {
            return;
        }

        if (alphabetSounds[soundIndex] == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            alphabetSounds[soundIndex]
        );
    }

    private void CompletePuzzle()
    {
        isPuzzleComplete = true;
        isDragging = false;

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved(
                puzzleId,
                dimension
            );
        }
        else
        {
            Debug.LogWarning(
                "씬에서 PuzzleManager를 찾을 수 없습니다."
            );
        }

        Debug.Log(
            "모든 주파수를 순서대로 맞췄습니다!"
        );
    }

    public void ResetPuzzle()
    {
        currentFrequency = minimumFrequency;
        currentTargetIndex = 0;

        isDragging = false;
        isPuzzleComplete = false;

        visualDialAngle = 0f;

        if (dialRect != null)
        {
            dialRect.localEulerAngles =
                Vector3.zero;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        UpdateFrequencyText();

        Debug.Log(
            "주파수 퍼즐이 초기화되었습니다."
        );
    }

    private void UpdateFrequencyText()
    {
        if (frequencyText != null)
        {
            frequencyText.text =
                $"{currentFrequency:F1} MHz";
        }
    }

    private float RoundToOneDecimal(float value)
    {
        return Mathf.Round(value * 10f) / 10f;
    }

    private void OnDestroy()
    {
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(
                ResetPuzzle
            );
        }
    }
}