using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 잠깐 떴다 사라지는 문구 한 줄. 메뉴 화면과 방찾기 화면이 각자 하나씩 씀.
/// 항상 켜져 있는 오브젝트에 붙이고, label만 캔버스 아래 텍스트를 가리키게 할 것.
/// (꺼진 오브젝트에서는 코루틴이 돌지 않아 문구가 안 사라짐)
/// </summary>
public class StatusLabel : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [Tooltip("자동으로 사라지기까지의 시간(초)")]
    [SerializeField] float duration = 4f;

    // 새 메시지가 오면 이전 타이머는 취소하고 다시 시작
    Coroutine _hideRoutine;

    void Awake()
    {
        if (label == null)
            label = GetComponent<TMP_Text>();
        Clear();
    }

    /// <param name="autoHide">
    /// false면 다음 메시지가 올 때까지 계속 떠 있음 (접속 중처럼 끝을 모르는 경우)
    /// </param>
    public void Show(string message, bool autoHide = true)
    {
        if (label == null)
            return;

        if (_hideRoutine != null)
        {
            StopCoroutine(_hideRoutine);
            _hideRoutine = null;
        }

        if (string.IsNullOrEmpty(message))
        {
            label.gameObject.SetActive(false);
            return;
        }

        label.text = message;
        label.gameObject.SetActive(true);

        if (autoHide && isActiveAndEnabled)
            _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    public void Clear() => Show(string.Empty);

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(duration);
        label.gameObject.SetActive(false);
        _hideRoutine = null;
    }
}
