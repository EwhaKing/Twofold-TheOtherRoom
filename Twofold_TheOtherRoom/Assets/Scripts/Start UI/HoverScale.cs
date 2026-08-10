using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// hover 시 버튼을 살짝 확대
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class HoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale")]
    [SerializeField] float hoverScale = 1.08f;

    [Header("Motion")]
    [SerializeField] float duration = 0.12f;
    [SerializeField] AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    RectTransform rect;
    Coroutine routine;

    void Awake() => rect = (RectTransform)transform;

    // 확대된 채로 화면이 꺼지면 코루틴이 죽어 커진 상태로 남으므로 되돌림
    void OnDisable()
    {
        routine = null;
        rect.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData e) => Play(hoverScale);

    public void OnPointerExit(PointerEventData e) => Play(1f);

    void Play(float to)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ScaleRoutine(to));
    }

    IEnumerator ScaleRoutine(float to)
    {
        Vector3 from = rect.localScale;
        Vector3 target = Vector3.one * to;

        yield return Animate(duration, ease,
            t => rect.localScale = Vector3.Lerp(from, target, t));

        routine = null;
    }

    // 일시정지 중에도 UI는 반응해야 하므로 unscaled 사용
    IEnumerator Animate(float duration, AnimationCurve ease, Action<float> apply)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            apply(ease.Evaluate(elapsed / duration));
            yield return null;
        }

        // 커브 끝값과 무관하게 목표치로 정확히 스냅
        apply(1f);
    }
}
