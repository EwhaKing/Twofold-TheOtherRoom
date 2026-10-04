using System.Collections;
using UnityEngine;
using TMPro;

public class MirrorObtainNoticeTrigger : MonoBehaviour
{
    [Header("자막 설정")]
    [TextArea]
    [SerializeField] private string noticeText = "거울을 얻었다. 거울 프레임에 맞춰보자";

    [Header("자막 UI 연결 (상위 패널에 얹은 UI)")]
    [SerializeField] private GameObject noticeCanvasOrPanel; // 띄울 안내 패널
    [SerializeField] private TMP_Text subtitleText;          // 문구 TMP
    [SerializeField] private CanvasGroup canvasGroup;        // 페이드용 CanvasGroup

    private bool isQuitting = false;

    private void Awake()
    {
        // 시작 시 UI 컴포넌트 자동 연결 및 초기화
        if (noticeCanvasOrPanel != null)
        {
            if (canvasGroup == null)
            {
                canvasGroup = noticeCanvasOrPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = noticeCanvasOrPanel.AddComponent<CanvasGroup>();
            }

            // 시작할 때는 패널을 숨김
            noticeCanvasOrPanel.SetActive(false);
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    // MirrorPiece가 거울을 끄는(SetActive false) 순간 자동 발동
    private void OnDisable()
    {
        if (isQuitting) return;

        // 거울이 꺼질 때 UI가 지정되어 있고 부모가 살아있다면 실행
        if (noticeCanvasOrPanel != null && transform.parent != null && transform.parent.gameObject.activeInHierarchy)
        {
            // 자신이 꺼지면 자체 코루틴이 멈추므로, 살아있는 부모의 MonoBehaviour를 빌려서 실행
            MonoBehaviour runner = transform.parent.GetComponentInParent<MonoBehaviour>();
            if (runner != null && runner.gameObject.activeInHierarchy)
            {
                runner.StartCoroutine(ShowNoticeRoutine());
            }
            else
            {
                // 러너를 못 찾을 경우 최소한 즉시 화면에 띄움
                noticeCanvasOrPanel.SetActive(true);
                if (subtitleText != null) subtitleText.text = noticeText;
                if (canvasGroup != null) canvasGroup.alpha = 1f;
            }
        }
    }

    private IEnumerator ShowNoticeRoutine()
    {
        noticeCanvasOrPanel.SetActive(true);

        // 혹시 비활성화되어 있을 자식들(Background, Subtitle 등) 전부 켜기
        for (int i = 0; i < noticeCanvasOrPanel.transform.childCount; i++)
        {
            noticeCanvasOrPanel.transform.GetChild(i).gameObject.SetActive(true);
        }

        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(true);
            subtitleText.text = noticeText;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            // 1초 동안 선명하게 유지
            yield return new WaitForSeconds(1.0f);

            // 0.5초 동안 서서히 투명해지며 사라짐
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        noticeCanvasOrPanel.SetActive(false);
    }
}