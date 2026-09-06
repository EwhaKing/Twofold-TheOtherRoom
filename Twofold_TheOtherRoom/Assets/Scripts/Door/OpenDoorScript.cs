using System.Collections;
using UnityEngine;

public class OpenDoorScript : MonoBehaviour, IInteractable
{

    [Header("문 회전")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private Vector3 openLocalEulerAngles = new Vector3(0f, 90f, 0f);
    [SerializeField, Min(0.01f)] private float openDuration = 1f;

    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine openRoutine;

        private void Awake()
    {
        if (doorPivot == null)
            doorPivot = transform;

        closedRotation = doorPivot.localRotation;
        openRotation = closedRotation * Quaternion.Euler(openLocalEulerAngles);
    }

    
    public void Interact()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    public void CloseDoor()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine = StartCoroutine(RotateDoorRoutine(closedRotation));
    }

    public void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine = StartCoroutine(RotateDoorRoutine(openRotation));
    }

    private IEnumerator RotateDoorRoutine(Quaternion targetRotation)
    {
        Quaternion startRotation = doorPivot.localRotation;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            doorPivot.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        doorPivot.localRotation = targetRotation;
        openRoutine = null;
    }

}
