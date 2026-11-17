using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pipe17Piece : MonoBehaviour, IPointerClickHandler
{
    public enum PipeType
    {
        Straight,   // 일자 파이프
        Bend        // 꺾인 파이프
    }

    [Header("pipe type")]
    [SerializeField] private PipeType pipeType;

    [Header("answer pipe")]
    [SerializeField] private bool isAnswerPipe = true;

    [Header("answer rotation")]
    [SerializeField] private float correctRotation = 0f;

    [Header("pipe image")]
    [SerializeField] private Image pipeImage;

    [Header("water color")]
    [SerializeField]
    private Color connectedColor =
        new Color(0.2f, 0.65f, 1f, 1f);

    private Color originalColor;
    private Pipe17PuzzleManager puzzleManager;

    public bool IsAnswerPipe => isAnswerPipe;


    private void Awake()
    {
        if (pipeImage == null)
        {
            pipeImage = GetComponent<Image>();
        }

        if (pipeImage != null)
        {
            originalColor = pipeImage.color;
        }

        puzzleManager =
            GetComponentInParent<Pipe17PuzzleManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 퍼즐을 이미 풀었으면 더 이상 클릭 안되게 함
        if (puzzleManager != null && puzzleManager.IsSolved)
        {
            return;
        }

        // 시계 방향 90도 회전
        transform.Rotate(0f, 0f, -90f);

        // 회전 후 상태 확인
        puzzleManager?.RefreshPuzzle();
    }


    public void SetRandomRotation()
    {
        // 0 / 90 / 180 / 270 중 랜덤으로 (정답 방향 기준으로)
        int randomQuarter = Random.Range(0, 4);

        float randomRotation =
            correctRotation - (randomQuarter * 90f);

        transform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                randomRotation
            );
    }


    public void Rotate90()
    {
        transform.Rotate(0f, 0f, -90f);
    }


    public bool IsCorrect()
    {
        // 가짜 파이프는 정답 판정에서 제외
        if (!isAnswerPipe)
        {
            return true;
        }

        float currentRotation =
            NormalizeAngle(
                transform.localEulerAngles.z
            );

        float targetRotation =
            NormalizeAngle(correctRotation);


        if (pipeType == PipeType.Straight)
        {
            float oppositeRotation =
                NormalizeAngle(
                    targetRotation + 180f
                );

            bool normalCorrect =
                Mathf.Abs(
                    Mathf.DeltaAngle(
                        currentRotation,
                        targetRotation
                    )
                ) < 1f;

            bool oppositeCorrect =
                Mathf.Abs(
                    Mathf.DeltaAngle(
                        currentRotation,
                        oppositeRotation
                    )
                ) < 1f;

            return normalCorrect || oppositeCorrect;
        }

        return Mathf.Abs(
            Mathf.DeltaAngle(
                currentRotation,
                targetRotation
            )
        ) < 1f;
    }


    public void SetConnected(bool connected)
    {
        if (pipeImage == null)
        {
            return;
        }

        pipeImage.color =
            connected
                ? connectedColor
                : originalColor;
    }


    private float NormalizeAngle(float angle)
    {
        angle %= 360f;

        if (angle < 0f)
        {
            angle += 360f;
        }

        return angle;
    }
}