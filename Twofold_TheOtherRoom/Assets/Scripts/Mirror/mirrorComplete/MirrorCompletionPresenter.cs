using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 거울 완성 상태를 매 프레임 다시 적용하고 자막을 띄운다. 게임플레이 씬마다 하나.
///
/// 전이 이벤트가 아니라 상태 재적용이라 이미 완성된 채로 진입해도 스스로 맞춰짐.
/// 조각 배치는 MirrorManager 담당이고, 여기는 "양쪽 다 완성했는가"만 본다.
///
/// 종료 문구는 거울을 봤을 때만 뜬다. 먼저 끝낸 쪽은 상대에게 힌트를 주러 방을 돌아다니므로
/// 양쪽 완성 순간에 거울 앞에 있으리라 기대할 수 없다. 대신 그 순간 알림 자막을 띄워 거울로 부른다.
/// 두 사람이 동시에 볼 필요는 없다 — 서로 다른 방에서 각자 화면을 보고 있으므로.
///
/// 종료 패널은 시간 종료와 공용(TimeoutPresenter). 한 번 뜨면 조작이 잠기므로 다시 보기는 없다.
/// </summary>
public class MirrorCompletionPresenter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("이 씬이 담당하는 차원")]
    [SerializeField] private MirrorManager.PuzzleDimension dimension = MirrorManager.PuzzleDimension.TwoD;

    [Tooltip("완성 거울의 발광")]
    [SerializeField] private MirrorGlow glow;

    [Tooltip("양쪽 완성 시 켤 이펙트. 씬에는 꺼둔 채로 저장할 것")]
    [SerializeField] private GameObject bothClearedEffect;

    [Header("Subtitle")]
    [Tooltip("자막 루트. 배경과 텍스트를 자식으로 두면 같이 페이드됨. 씬에는 꺼둔 채로 저장할 것.\n" +
             "공용 Canvas 루트를 물리면 안 됨 — 여기를 SetActive(false) 하므로 UI 전체가 꺼짐")]
    [SerializeField] private CanvasGroup noticeRoot;

    [SerializeField] private TMP_Text noticeText;

    [Tooltip("잠깐 뜨는 자막을 선명하게 유지하는 시간(초). 문구를 다 읽을 만큼")]
    [SerializeField] private float holdSeconds = 2.5f;

    [SerializeField] private float fadeSeconds = 1f;

    [Header("Messages")]
    [Tooltip("내 거울만 완성했을 때 거울을 볼 경우")]
    [SerializeField]
    private string waitingMessage = "거울이 완성되었다. 상대도 완성했는지 확인해보자.";

    // 거울이 화면 밖에 있어도 보이는 유일한 신호. 여기서 거울로 부르지 못하면 엔딩을 놓친다
    [Tooltip("양쪽 완성 순간. 방 어디에 있든 뜸")]
    [SerializeField]
    private string bothClearedMessage = "상대도 거울을 완성했다. 거울을 확인해보자.";

    [Header("Debug")]
    [Tooltip("네트워크 없이 단독 실행할 때 내 완성만으로 양쪽 완성 취급. 씬에는 꺼둔 채로 저장할 것")]
    [SerializeField] private bool treatMineAsBothLocally = false;

    /// 직전에 적용한 상태. null 이면 아직 한 번도 적용 안 함
    private bool? applied;

    /// 잠깐 떴다 사라지는 자막. 대기 안내와 양쪽 완성 알림이 같이 씀
    private Coroutine transientRoutine;

    /// <summary>내 차원의 거울 조각이 전부 배치됐는지</summary>
    public bool MineDone =>
        MirrorManager.Instance != null && MirrorManager.Instance.AreAllMirrorPiecesPlaced(dimension);

    /// 한 번 완성이면 계속 완성. 방장이 나가면 GameSession 이 사라져 값이 false 로 돌아감
    private bool cachedBothDone;

    /// <summary>양쪽 다 완성했는지. GameSession 이 없으면(단독 실행) 디버그 옵션대로</summary>
    public bool BothDone
    {
        get
        {
            if (cachedBothDone) return true;

            GameSession session = GameSession.Instance;
            if (session == null) return treatMineAsBothLocally && MineDone;

            cachedBothDone = session.BothCleared;
            return cachedBothDone;
        }
    }

    private void Awake()
    {
        if (noticeRoot == null) return;

        noticeRoot.alpha = 0f;
        noticeRoot.gameObject.SetActive(false);
    }

    private void Update() => Apply(BothDone);

    /// <summary>
    /// 완성 거울을 볼 때(2D 클릭 · 3D E키).
    /// 상대가 아직이면 안내 문구, 양쪽 완성 뒤면 종료 문구.
    /// </summary>
    public void RequestInspect()
    {
        if (!MineDone || noticeRoot == null) return;

        if (!BothDone)
        {
            ShowTransient(waitingMessage);
            return;
        }

        HideTransient();

        // 시간 종료와 같은 패널. 문구만 다름
        TimeoutPresenter panel = FindAnyObjectByType<TimeoutPresenter>();
        if (panel != null) panel.ShowEnding();
        else Debug.LogWarning("[Mirror] 씬에 TimeoutPresenter 없음 — 종료 패널을 못 띄움", this);
    }

    private void Apply(bool bothDone)
    {
        bool first = applied == null;
        if (applied == bothDone) return;
        applied = bothDone;

        if (bothClearedEffect != null) bothClearedEffect.SetActive(bothDone);
        if (!bothDone) return;

        // 첫 적용에 이미 완성이면 연출 없이 최종 상태로. 지나간 순간의 알림도 띄우지 않음
        if (glow != null) glow.Apply(first);
        if (!first) ShowTransient(bothClearedMessage);
    }

    /// <summary>잠깐 떴다 사라지는 자막.</summary>
    private void ShowTransient(string message)
    {
        if (transientRoutine != null) StopCoroutine(transientRoutine);
        transientRoutine = StartCoroutine(TransientRoutine(message));
    }

    private IEnumerator TransientRoutine(string message)
    {
        if (noticeText != null) noticeText.text = message;

        noticeRoot.gameObject.SetActive(true);
        noticeRoot.alpha = 1f;

        yield return new WaitForSeconds(holdSeconds);

        float elapsed = 0f;
        while (elapsed < fadeSeconds)
        {
            elapsed += Time.deltaTime;
            noticeRoot.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeSeconds);
            yield return null;
        }

        noticeRoot.alpha = 0f;
        noticeRoot.gameObject.SetActive(false);
        transientRoutine = null;
    }

    /// <summary>떠 있는 자막을 즉시 치움. 종료 패널 아래에 남지 않게</summary>
    private void HideTransient()
    {
        if (transientRoutine != null)
        {
            StopCoroutine(transientRoutine);
            transientRoutine = null;
        }

        noticeRoot.alpha = 0f;
        noticeRoot.gameObject.SetActive(false);
    }
}
