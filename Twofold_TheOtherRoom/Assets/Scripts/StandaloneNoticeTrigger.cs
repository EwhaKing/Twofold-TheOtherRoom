using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class StandaloneNoticeTrigger : MonoBehaviour, IPointerClickHandler
{
    [Header("띄울 문구 설정")]
    [TextArea]
    [SerializeField] private string noticeText = "이것은 작동하지 않는 것 같다";

    [Header("자립형 UI 연결 (프리팹 안의 UI)")]
    [SerializeField] private GameObject noticeCanvasOrPanel; // 켜고 끌 UI 패널/캔버스
    [SerializeField] private TMP_Text subtitleText;          // 글씨 바꿀 TMP
    [SerializeField] private CanvasGroup canvasGroup;        // 페이드용 (없으면 자동 추가)

    private Coroutine currentRoutine;

    private void Awake()
    {
        // 시작 시 UI 컴포넌트 자동 탐색 및 세팅
        if (noticeCanvasOrPanel != null)
        {
            if (canvasGroup == null)
            {
                canvasGroup = noticeCanvasOrPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = noticeCanvasOrPanel.AddComponent<CanvasGroup>();
            }

            // 시작할 때는 UI 전체 숨김
            noticeCanvasOrPanel.SetActive(false);
        }
    }

    // UI Image 클릭 시
    public void OnPointerClick(PointerEventData eventData)
    {
        ShowNotice();
    }

    // 2D 콜라이더 클릭 시
    private void OnMouseDown()
    {
        ShowNotice();
    }

    public void ShowNotice()
    {
        Debug.Log("시계 클릭 감지 성공!");

        if (noticeCanvasOrPanel == null)
        {
            Debug.LogError("Notice Canvas/Panel이 연결되지 않았습니다!", this);
            return;
        }

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(NoticeFadeRoutine());
    }

    private IEnumerator NoticeFadeRoutine()
    {
        // 1. 패널 활성화
        noticeCanvasOrPanel.SetActive(true);

        // 2. 혹시 꺼져있을 자식 오브젝트들 전부 강제로 켜기
        for (int i = 0; i < noticeCanvasOrPanel.transform.childCount; i++)
        {
            noticeCanvasOrPanel.transform.GetChild(i).gameObject.SetActive(true);
        }

        // 3. 자막 텍스트 갱신 및 활성화
        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(true);
            subtitleText.text = noticeText;
        }

        // 4. 투명도 제어 (1초간 선명하게 머무른 뒤 1초간 페이드아웃)
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            // 글자를 편하게 읽을 수 있도록 1초간 그대로 정지 유지
            yield return new WaitForSeconds(1.0f);

            // 이후 1초 동안 부드럽게 페이드아웃
            float duration = 1.0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        // 5. 완료 후 패널 끄기
        noticeCanvasOrPanel.SetActive(false);
        currentRoutine = null;
    }
}