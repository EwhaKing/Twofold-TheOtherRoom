using System.Collections;
using UnityEngine;

/// <summary>
/// Canvas 안에 가로로 배치된 1920x1080 방들을 버튼으로 이동시킵니다.
/// Room 0, 1, 2...는 각각 X = 0, 1920, 3840... 위치에 배치합니다.
/// </summary>
public class NavigateRoom2DCanvas : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField] private RectTransform roomsRoot;
    [SerializeField, Min(1)] private int totalRooms = 4;
    [SerializeField, Min(0)] private int startRoom = 1; 

    [Header("UI Arrows")]
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;

    [Header("Move Settings")]
    [SerializeField, Min(1f)] private float roomWidth = 1920f;
    [SerializeField, Min(0f)] private float moveDuration = 0.45f;

    private int currentRoomIndex;
    private bool isMoving;
    private float initialY;

    private void Awake()
    {
        if (roomsRoot == null)
        {
            Debug.LogError("NavigateRoom2DCanvas: Rooms Root가 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        initialY = roomsRoot.anchoredPosition.y;
        currentRoomIndex = Mathf.Clamp(startRoom, 0, totalRooms - 1);
    }

    private void Start()
    {
        MoveInstant();
        UpdateArrows();
    }

    public void MoveLeft()
    {
        if (isMoving || currentRoomIndex <= 0)
        {
            return;
        }

        SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
        currentRoomIndex--;
        StartCoroutine(MoveToRoom());
    }

    public void MoveRight()
    {
        if (isMoving || currentRoomIndex >= totalRooms - 1)
        {
            return;
        }

        SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
        currentRoomIndex++;
        StartCoroutine(MoveToRoom());
    }

    public void MoveToRoom(int roomIndex)
    {
        if (isMoving)
        {
            return;
        }

        int clampedIndex = Mathf.Clamp(roomIndex, 0, totalRooms - 1);
        if (clampedIndex == currentRoomIndex)
        {
            return;
        }

        currentRoomIndex = clampedIndex;
        StartCoroutine(MoveToRoom());
    }

    private IEnumerator MoveToRoom()
    {
        isMoving = true;
        UpdateArrows();

        Vector2 startPosition = roomsRoot.anchoredPosition;
        Vector2 targetPosition = GetRoomPosition(currentRoomIndex);

        if (moveDuration <= 0f)
        {
            roomsRoot.anchoredPosition = targetPosition;
        }
        else
        {
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                t = Mathf.SmoothStep(0f, 1f, t);

                roomsRoot.anchoredPosition =
                    Vector2.LerpUnclamped(startPosition, targetPosition, t);

                yield return null;
            }

            roomsRoot.anchoredPosition = targetPosition;
        }

        isMoving = false;
        UpdateArrows();
    }

    private void MoveInstant()
    {
        roomsRoot.anchoredPosition = GetRoomPosition(currentRoomIndex);
    }

    private Vector2 GetRoomPosition(int roomIndex)
    {
        return new Vector2(-roomWidth * roomIndex, initialY);
    }

    private void UpdateArrows()
    {
        if (leftArrow != null)
        {
            leftArrow.SetActive(currentRoomIndex > 0);
        }

        if (rightArrow != null)
        {
            rightArrow.SetActive(currentRoomIndex < totalRooms - 1);
        }
    }
}
