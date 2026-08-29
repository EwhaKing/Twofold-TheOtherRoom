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
       OpenDoor();
    }

    public void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;

        // 지하실 문이 열릴 때 지하실 BGM으로 변경
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(BGMType.BasementBG);
        }

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine = StartCoroutine(OpenDoorRoutine());
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
