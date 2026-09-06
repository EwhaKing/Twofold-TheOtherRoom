using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DownloadPopOut : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject Mirror4;
    [SerializeField] private GameObject MirrorIcon;

    [Header("Pop Out Motion")]
    [SerializeField, Min(0.01f)] private float popDuration;
    [SerializeField, Min(0f)] private float popDistance ;
    [SerializeField, Min(0f)] private float arcHeight;
    [SerializeField, Min(0f)] private float releaseForwardSpeed = 1.5f;

    private Coroutine popOutCoroutine;
    private bool hasPlayed;

    public void PlayPopOut()
    {
        if (hasPlayed)
            return;

        if (Mirror4 == null)
        {
            Debug.LogWarning("DownloadPopOut: Mirror4가 지정되지 않았습니다.", this);
            return;
        }

        hasPlayed = true;

        if (TryGetComponent(out Button downloadButton))
            downloadButton.interactable = false;

        MirrorIcon.SetActive(false);

        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.ReportSolved("3D-11", PuzzleDimension.ThreeD);
        else
            Debug.LogWarning("[ThreeDCommunicationPuzzle] PuzzleManager.Instance가 없습니다.", this);

        BeginScriptedMotion();

        if (popOutCoroutine != null)
            StopCoroutine(popOutCoroutine);

        popOutCoroutine = StartCoroutine(PopOutRoutine());
    }

    private void BeginScriptedMotion()
    {
        // Transform 애니메이션 중 컴퓨터나 Player를 밀지 않도록 물리 반응을 끈다.
        foreach (Rigidbody body in Mirror4.GetComponentsInChildren<Rigidbody>(true))
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        foreach (Collider targetCollider in Mirror4.GetComponentsInChildren<Collider>(true))
            targetCollider.enabled = false;
    }

    private void BeginFalling(Vector3 releaseDirection)
    {
        // 팝아웃이 끝나면 충돌과 중력을 복구해 바닥으로 자연스럽게 떨어뜨린다.
        foreach (Collider targetCollider in Mirror4.GetComponentsInChildren<Collider>(true))
            targetCollider.enabled = true;

        // 마지막 Transform 위치와 새 Collider를 물리 월드에 먼저 반영한다.
        Physics.SyncTransforms();

        foreach (Rigidbody body in Mirror4.GetComponentsInChildren<Rigidbody>(true))
        {
            body.detectCollisions = true;
            body.isKinematic = false;
            body.useGravity = true;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearVelocity = releaseDirection.normalized * releaseForwardSpeed;
            body.WakeUp();
        }
    }

    private IEnumerator PopOutRoutine()
    {
        Transform target = Mirror4.transform;
        Vector3 startPosition = target.position;

        // 좌우로 꺾지 않고 Mirror4의 정면으로 곧게 튀어나온다.
        // 로컬축이 x 축이라 right이 정면. 
        Vector3 releaseDirection = target.right.normalized;
        Vector3 endPosition = startPosition + releaseDirection * popDistance;
        Vector3 up = Vector3.up;

        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);
            // 처음과 끝을 천천히 만들어 튀어나오는 과정이 눈에 보이게 한다.
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            // 정면 이동에 높이를 더해 수직 평면 안에서 포물선을 그린다.
            float arc = Mathf.Sin(Mathf.PI * t) * arcHeight;
            target.position = Vector3.LerpUnclamped(startPosition, endPosition, easedT) + up * arc;

            yield return null;
        }

        target.position = endPosition;
        BeginFalling(releaseDirection);
        popOutCoroutine = null;
    }
}
