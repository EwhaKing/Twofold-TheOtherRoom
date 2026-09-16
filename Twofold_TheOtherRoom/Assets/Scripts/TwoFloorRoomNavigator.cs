using System.Collections;
using UnityEngine;

/// <summary>Two rooms per floor. The initial root position must show the upper-left room.</summary>
public class TwoFloorRoomNavigator : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField] private RectTransform roomsRoot;
    [SerializeField] private bool isLowerFloor;
    [SerializeField, Range(0, 1)] private int startRoomIndex;

    [Header("UI Arrows")]
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;

    [Header("Layout")]
    [SerializeField, Min(1f)] private float roomWidth = 1920f;
    [SerializeField, Min(1f)] private float floorHeight = 1080f;
    [Tooltip("Lower-left room X relative to the upper-left room. Use -1920 for the pictured layout.")]
    [SerializeField] private float lowerFloorLeftX = -1920f;
    [SerializeField, Range(0, 1)] private int lowerFloorEntryIndex = 0;
    [SerializeField, Range(0, 1)] private int upperFloorEntryIndex;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveDuration = 0.45f;

    [Header("Floor Movement Sound")]
    [SerializeField] private bool playFloorMoveSound = false;
    // [SerializeField] private SFXType downstairsSound = SFXType.FloorHole;
    // [SerializeField] private SFXType upstairsSound = SFXType.FloorHole;

    public bool IsLowerFloor => isLowerFloor;
    public int CurrentRoomIndex => currentRoomIndex;

    private Vector2 upperFloorOrigin;
    private int currentRoomIndex;
    private Coroutine moveRoutine;
    private bool isMoving;
    private bool initialized;

    private void Awake()
    {
        if (roomsRoot == null)
        {
            Debug.LogError("TwoFloorRoomNavigator: Assign Rooms Root.", this);
            enabled = false;
            return;
        }

        upperFloorOrigin = roomsRoot.anchoredPosition;
        currentRoomIndex = Mathf.Clamp(startRoomIndex, 0, 1);
        initialized = true;
        roomsRoot.anchoredPosition = GetTargetPosition();
        UpdateArrows();
    }

    public void MoveLeft() => MoveToRoom(currentRoomIndex - 1);
    public void MoveRight() => MoveToRoom(currentRoomIndex + 1);

    public void MoveToRoom(int roomIndex)
    {
        if (!initialized || !isActiveAndEnabled || isMoving ||
            roomIndex < 0 || roomIndex > 1 || roomIndex == currentRoomIndex)
            return;

        currentRoomIndex = roomIndex;
        PlayMoveSound();
        BeginMovement();
    }

    private void BeginMovement()
    {
        if (moveDuration <= 0f)
        {
            roomsRoot.anchoredPosition = GetTargetPosition();
            UpdateArrows();
            return;
        }

        moveRoutine = StartCoroutine(AnimateMove());
    }

    /// <summary>Switch floors smoothly. True enters the lower floor; false returns upstairs.</summary>
    public void SetFloor(bool lowerFloor)
    {
        if (!initialized || !isActiveAndEnabled || isMoving || isLowerFloor == lowerFloor)
            return;

        isLowerFloor = lowerFloor;
        currentRoomIndex = Mathf.Clamp(lowerFloor ? lowerFloorEntryIndex : upperFloorEntryIndex, 0, 1);
        // if (playFloorMoveSound && SoundManager.Instance != null)
        //     SoundManager.Instance.PlaySFX(lowerFloor ? downstairsSound : upstairsSound);
        BeginMovement();
    }

    private Vector2 GetTargetPosition()
    {
        float roomX =  roomWidth * currentRoomIndex;
        return upperFloorOrigin + new Vector2(-roomX, isLowerFloor ? floorHeight : 0f);
    }

    private IEnumerator AnimateMove()
    {
        isMoving = true;
        UpdateArrows();
        Vector2 from = roomsRoot.anchoredPosition;
        Vector2 target = GetTargetPosition();
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / moveDuration));
            roomsRoot.anchoredPosition = Vector2.LerpUnclamped(from, target, t);
            yield return null;
        }

        roomsRoot.anchoredPosition = target;
        isMoving = false;
        moveRoutine = null;
        UpdateArrows();
    }

    private void StopMovement()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        moveRoutine = null;
        isMoving = false;
    }

    private void OnDisable()
    {
        if (!initialized)
            return;
        StopMovement();
        roomsRoot.anchoredPosition = GetTargetPosition();
        UpdateArrows();
    }

    private void UpdateArrows()
    {
        if (leftArrow != null)
            leftArrow.SetActive(!isMoving && currentRoomIndex > 0);
        if (rightArrow != null)
            rightArrow.SetActive(!isMoving && currentRoomIndex < 1);
    }

    private static void PlayMoveSound()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SFXType.RoomMove);
    }
}