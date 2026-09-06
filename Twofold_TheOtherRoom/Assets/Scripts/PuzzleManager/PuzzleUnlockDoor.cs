using System.Collections;
using UnityEngine;

/// <summary>
/// 지정한 퍼즐들이 모두 해결되면 문을 한 번만 엽니다.
/// PuzzleManager가 보관한 해결 상태를 사용하므로 별도의 bool 배열은 필요하지 않습니다.
/// </summary>

public class PuzzleUnlockDoor : MonoBehaviour
{
    [Header("1번 방에서 반드시 풀어야 하는 퍼즐 ID")]
    [SerializeField] private string[] requiredPuzzleIds;

    [Header("문 회전")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private Vector3 openLocalEulerAngles = new Vector3(0f, 90f, 0f);
    [SerializeField, Min(0.01f)] private float openDuration = 1f;

    // [Header("문 열림 소리 (선택)")]
    // [SerializeField] private AudioClip openDoorClip;


    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine openRoutine;
    private bool isOpen;

    private void Awake()
    {
        if (doorPivot == null)
            doorPivot = transform;

        closedRotation = doorPivot.localRotation;
        openRotation = closedRotation * Quaternion.Euler(openLocalEulerAngles);
    }

    private void OnEnable()
    {
        PuzzleManager.OnPuzzleSolved += HandlePuzzleSolved;

    }

    private void Start()
    {
        // PuzzleUnlockDoor가 PuzzleManager보다 먼저 활성화된 경우를 대비합니다.
        CheckAllRequiredPuzzles();
    }

    private void OnDisable()
    {
        PuzzleManager.OnPuzzleSolved -= HandlePuzzleSolved;
    }

    private void HandlePuzzleSolved(string solvedId)
    {
        if (isOpen || !IsRequiredPuzzle(solvedId))
            return;

        CheckAllRequiredPuzzles();
    }

    private bool IsRequiredPuzzle(string puzzleId)
    {
        if (requiredPuzzleIds == null)
            return false;

        foreach (string requiredId in requiredPuzzleIds)
        {
            if (string.Equals(requiredId?.Trim(), puzzleId?.Trim(),
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void CheckAllRequiredPuzzles()
    {
        if (isOpen || PuzzleManager.Instance == null ||
            requiredPuzzleIds == null || requiredPuzzleIds.Length == 0)
        {
            return;
        }

        foreach (string requiredId in requiredPuzzleIds)
        {
            if (string.IsNullOrWhiteSpace(requiredId) ||
                !PuzzleManager.Instance.IsSolved(requiredId.Trim()))
            {
                return;
            }
        }

        OpenDoor();
    }

    public void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;

        // 문 열림 효과음
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.DoorOpen);
        }

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine = StartCoroutine(OpenDoorRoutine());
    }

    [ContextMenu("TEST - Open Door")]
    private void TestOpenDoor()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("문 열림 테스트는 Play Mode에서 실행해 주세요.", this);
            return;
        }

        OpenDoor();
    }

    private IEnumerator OpenDoorRoutine()
    {
        Quaternion startRotation = doorPivot.localRotation;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            doorPivot.localRotation = Quaternion.Slerp(startRotation, openRotation, t);
            yield return null;
        }

        doorPivot.localRotation = openRotation;
        openRoutine = null;
    }
}
