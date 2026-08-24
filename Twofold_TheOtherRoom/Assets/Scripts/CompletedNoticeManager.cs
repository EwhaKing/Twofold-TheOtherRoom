using System.Collections;
using UnityEngine;
using TMPro;

public class CompletedNoticeManager : MonoBehaviour
{
    public static CompletedNoticeManager Instance { get; private set; }

    public GameObject introCanvas;       // IntroCanvas (전체)
    public GameObject background;        // IntroCanvas 하위의 Background
    public TMP_Text subtitle;            // IntroCanvas 하위의 Subtitle (TMP)

    private CanvasGroup canvasGroup;
    private Coroutine noticeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // CanvasGroup 없으면 코드에서 자동으로 붙여줌
        if (introCanvas != null)
        {
            canvasGroup = introCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = introCanvas.AddComponent<CanvasGroup>();
            }
            
            // 처음 씬이 시작할 때는 캔버스 전체를 꺼둠
            introCanvas.SetActive(false);
        }
    }

    // 이미 완료된 퍼즐 클릭 시 호출
    public void ShowNotice(string message = "이미 완료한 퍼즐입니다")
    {
        if (introCanvas == null) return;

        if (noticeCoroutine != null) StopCoroutine(noticeCoroutine);
        noticeCoroutine = StartCoroutine(NoticeRoutine(message));
    }

    private IEnumerator NoticeRoutine(string message)
    {
        //IntroCanvas와 Background만 SetActive(true)
        introCanvas.SetActive(true);
        if (background != null) background.SetActive(true);

        // 2. Subtitle 텍스트 교체
        if (subtitle != null)
        {
            subtitle.text = message;
        }

        // 3. 알파 1 (바로 선명하게 등장)
        canvasGroup.alpha = 1f;

        // 4. 1초 동안 스스륵 사라짐 (알파 0)
        float duration = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        // 5. 알파 0 완료 후 다시 끄기 (SetActive false)
        canvasGroup.alpha = 0f;
        introCanvas.SetActive(false);
        noticeCoroutine = null;
    }
}