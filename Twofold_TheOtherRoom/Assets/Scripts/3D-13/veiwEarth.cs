using UnityEngine;
using System.Collections;

public class veiwEarth : MonoBehaviour, IInteractable
{
    [SerializeField] private ObjectCameraManager cameraManager;

    public void Interact()
    {
        if (cameraManager != null)
        {
            cameraManager.StartView();
        }
    }

    [Header("이동할 Key")]
    [SerializeField] private Transform key;

    [Header("Key 이동 위치")]
    [SerializeField] private Transform[] keyTargets;

    private int currentTargetIndex = 0;
    private Coroutine moveCoroutine;
    private int moveDirection = 1;


    [Header("이동 설정")]
    [SerializeField] private float moveDuration = 0.5f;

    private void OnMouseDown()
    {
        if (!cameraManager.isViewing)
        {
            return;
        }
        
        if (key == null)
        {
            Debug.LogWarning("[GlobeClick] Key가 연결되지 않았습니다.");
            return;
        }

        if (keyTargets == null || keyTargets.Length == 0)
        {
            Debug.LogWarning("[GlobeClick] Key Target이 없습니다.");
            return;
        }

        if (moveCoroutine != null)
            return;

        moveCoroutine = StartCoroutine(
            MoveKey(keyTargets[currentTargetIndex])
        );

        Debug.Log(
            $"[GlobeClick] 지구본 클릭 → Key Target {currentTargetIndex + 1} 이동"
        );

        currentTargetIndex += moveDirection;

        if (currentTargetIndex >= keyTargets.Length)
        {
            currentTargetIndex = keyTargets.Length - 2;
            moveDirection = -1;
        }
        else if (currentTargetIndex < 0)
        {
            currentTargetIndex = 1;
            moveDirection = 1;
        }
    }
    private IEnumerator MoveKey(Transform target)
    {
        Vector3 startPosition = key.position;
        Quaternion startRotation = key.rotation;

        Vector3 targetPosition = target.position;
        Quaternion targetRotation = target.rotation;

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;

            float t = timer / moveDuration;

            t = Mathf.SmoothStep(0f, 1f, t);


            key.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );


            key.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        key.position = targetPosition;
        key.rotation = targetRotation;
        moveCoroutine = null;
    }
}
