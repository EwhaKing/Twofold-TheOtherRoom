using UnityEngine;
using System.Collections;


public class NavigateRoom2D : MonoBehaviour
{
    [Header("Rooms")]
    public Transform roomsRoot;
    public int total_rooms = 4;

    [Header("UI Arrows")]
    public GameObject leftArrow;
    public GameObject rightArrow;

    [Header("Move Settings")]
    public float roomWidth = 19.2f;
    public float moveDuration;
    

    private int currentRoomIndex = 0;
    private bool isMoving = false;

    private void Start()
    {
        UpdateArrows();
        MoveInstant();
    }

    public void MoveLeft()
    {
        if (isMoving) return;
        if (currentRoomIndex <= 0) return;

        currentRoomIndex--;
        StartCoroutine(MoveToRoom());
    }

    public void MoveRight()
    {
        if (isMoving) return;
        if (currentRoomIndex >= total_rooms - 1) return;

        currentRoomIndex++;
        StartCoroutine(MoveToRoom());
    }

    private IEnumerator MoveToRoom()
    {
        isMoving = true;
        UpdateArrows();

        Vector3 startPos = roomsRoot.position;
        Vector3 targetPos = new Vector3(-roomWidth * currentRoomIndex, roomsRoot.position.y, roomsRoot.position.z);

        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = time / moveDuration;

            // 부드러운 이동
            t = Mathf.SmoothStep(0f, 1f, t);

            roomsRoot.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        roomsRoot.position = targetPos;
        isMoving = false;
        UpdateArrows();
    }

    private void MoveInstant()
    {
        roomsRoot.position = new Vector3(-roomWidth * currentRoomIndex, roomsRoot.position.y, roomsRoot.position.z);
    }

    private void UpdateArrows()
    {
        leftArrow.SetActive(currentRoomIndex > 0);
        rightArrow.SetActive(currentRoomIndex < total_rooms - 1);
    }
}
