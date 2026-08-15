using System.Collections;
using UnityEngine;

public class ShadowQuadScaler : MonoBehaviour
{
    [Header("Block To Follow")]
    [SerializeField] private MoveBlocks moveBlock;

    [Header("Scale Settings")]
    [Tooltip("최종 Local Scale Y = 이 값 x currentZone")]
    [SerializeField, Min(0f)] private float scalePerZone = 5f;

    private Coroutine scaleCoroutine;
    private float bottomLocalY;
    private float meshHeight = 1f;

    private void OnEnable()
    {
        if (moveBlock != null)
            moveBlock.ZoneChanged += HandleZoneChanged;
    }

    private void Start()
    {
        if (moveBlock == null)
        {
            Debug.LogWarning("[ShadowQuadScaler] MoveBlocks가 연결되지 않았습니다.", this);
            return;
        }
        SetScaleImmediately(moveBlock.GetZone());
    }

    private void OnDisable()
    {
        if (moveBlock != null)
            moveBlock.ZoneChanged -= HandleZoneChanged;
    }

    private void HandleZoneChanged(int zone)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleToZone(zone));
    }

    private IEnumerator ScaleToZone(int zone)
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale;
        targetScale.y = scalePerZone * zone;
        Vector3 startPosition = transform.localPosition;
        Vector3 targetPosition = PositionForScale(targetScale.y);

        float duration = moveBlock.moveDuration;

        if (duration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
        }

        transform.localScale = targetScale;
        transform.localPosition = targetPosition;
        scaleCoroutine = null;
    }

    private void SetScaleImmediately(int zone)
    {
        Vector3 scale = transform.localScale;
        scale.y = scalePerZone * zone;
        transform.localScale = scale;
        transform.localPosition = PositionForScale(scale.y);
    }

    private Vector3 PositionForScale(float scaleY)
    {
        Vector3 position = transform.localPosition;
        position.y = bottomLocalY + meshHeight * scaleY * 0.5f;
        return position;
    }
}
