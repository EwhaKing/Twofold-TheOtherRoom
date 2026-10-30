using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면이 켜질 때 두 이미지를 가운데(x=0)에서 좌우로 벌리며 페이드 인
/// y는 각 이미지의 현재 값을 그대로 씀
/// </summary>
public class SplitReveal : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] Image left;
    [SerializeField] Image right;

    [Header("Motion")]
    [Tooltip("x=0에서 좌우로 벌어질 거리. 왼쪽은 -distance, 오른쪽은 +distance")]
    [SerializeField] float distance = 200f;
    [SerializeField] float duration = 0.4f;
    [SerializeField] AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Fade")]
    [Tooltip("투명도 0→1에 걸리는 시간. 움직임과 따로 일정한 속도로 올라감")]
    [SerializeField] float fadeDuration = 0.5f;

    Coroutine routine;

    void OnEnable() => Play();

    void OnDisable()
    {
        routine = null;
    }

    public void Play()
    {
        if (left == null || right == null) return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(RevealRoutine());
    }

    // 일시정지 중에는 멈추도록 deltaTime 사용 (한 프레임 최대 0.333초로 잘려 로딩 끊김에도 안 튐)
    IEnumerator RevealRoutine()
    {
        // 시작 상태로 맞춘 뒤, 씬 로딩으로 길어진 첫 프레임은 건너뜀
        Apply(left, 0f, 0f);
        Apply(right, 0f, 0f);
        yield return null;

        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float t = ease.Evaluate(elapsed / duration);

            Apply(left, -distance * t, t);
            Apply(right, distance * t, t);
            yield return null;
        }

        // 커브 끝값과 무관하게 목표치로 정확히 스냅
        Apply(left, -distance, 1f);
        Apply(right, distance, 1f);

        routine = null;
    }

    static void Apply(Image image, float x, float alpha)
    {
        RectTransform rect = image.rectTransform;
        rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
