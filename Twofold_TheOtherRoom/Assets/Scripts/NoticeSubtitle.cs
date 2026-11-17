using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 공통 안내 자막
/// Show: 잠깐 띄웠다 페이드
/// Set: 지울 때까지 유지 (Intro용)
/// </summary>
public class NoticeSubtitle : MonoBehaviour
{
    /// <summary>패널 프리팹들이 쓰는 씬 공용 자막. 씬 루트의 NoticeCanvas</summary>
    public static NoticeSubtitle Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TMP_Text text;

    [Tooltip("배경 + 텍스트 묶음. 숨길 때 끄는 대상")]
    [SerializeField] private GameObject content;

    [SerializeField] private CanvasGroup group;

    [Header("Timing")]
    [Tooltip("선명하게 유지하는 시간(초)")]
    [SerializeField] private float hold = 1f;

    [Tooltip("사라지는 시간(초)")]
    [SerializeField] private float fade = 1f;

    /// 캔버스 정렬. 인트로(500) 위, 일시정지(1000) 아래
    private const int SortingOrder = 600;

    private Coroutine _routine;

    /// 현재 보여지는 문구. 숨김 상태면 null
    private string _current;

    /// Show 페이드 단계 여부
    private bool _fading;

    /// Set으로 띄운 고정 문구 여부. 떠 있는 동안 Show 무시
    private bool _pinned;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Debug.LogWarning("[Notice] 씬에 NoticeCanvas 가 둘 이상", this);

        Instance = this;

        if (TryGetComponent(out Canvas canvas)) canvas.sortingOrder = SortingOrder;

        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    #region 외부 호출

    /// <summary>문구를 띄웠다가 페이드 (기본)</summary>
    public void Show(string message) => Show(message, hold, fade);

    /// <summary>
    /// 문구를 띄웠다가 페이드 (시간 지정 가능)
    /// Set 문구가 떠 있거나 같은 문구가 유지 중이면 무시. 페이드 중인 같은 문구는 다시 띄움
    /// </summary>
    public void Show(string message, float holdSeconds, float fadeSeconds)
    {
        if (!isActiveAndEnabled)
        {
            Debug.LogWarning("[Notice] 꺼진 자막에 Show 호출", this);
            return;
        }
        if (_pinned) return;
        if (_routine != null && !_fading && message == _current) return;   // 연타 방지

        StopRoutine();
        _routine = StartCoroutine(ShowRoutine(message, holdSeconds, fadeSeconds));
    }

    /// <summary>페이드 없이 바로 표시. 빈 문자열이면 숨김</summary>
    public void Set(string message)
    {
        StopRoutine();
        _pinned = !string.IsNullOrEmpty(message);

        if (!_pinned)
        {
            Hide();
            return;
        }

        Display(message);
    }

    #endregion

    #region 문구 표시, 페이드 처리

    private IEnumerator ShowRoutine(string message, float holdSeconds, float fadeSeconds)
    {
        Display(message);

        if (holdSeconds > 0f) yield return new WaitForSeconds(holdSeconds);

        _fading = true;

        if (group != null)
        {
            for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
            {
                group.alpha = 1f - t / fadeSeconds;
                yield return null;
            }
        }

        Hide();
        _routine = null;
    }

    private void Display(string message)
    {
        _current = message;
        _fading = false;

        if (text != null) text.text = message;
        if (group != null) group.alpha = 1f;
        if (content != null && !content.activeSelf) content.SetActive(true);
    }

    private void Hide()
    {
        _current = null;
        _fading = false;

        if (content != null && content.activeSelf) content.SetActive(false);
    }

    private void StopRoutine()
    {
        if (_routine == null) return;

        StopCoroutine(_routine);
        _routine = null;
    }

    #endregion
}
