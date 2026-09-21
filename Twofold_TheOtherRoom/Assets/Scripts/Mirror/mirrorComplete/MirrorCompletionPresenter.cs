using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 거울 완성 상태를 매 프레임 다시 적용하고 자막을 띄운다. 게임플레이 씬마다 하나.
///
/// 전이 이벤트가 아니라 상태 재적용이라 이미 완성된 채로 진입해도 스스로 맞춰짐.
/// 조각 배치는 MirrorManager 담당이고, 여기는 "양쪽 다 완성했는가"만 본다.
///
/// 양쪽 완성 뒤 거울을 보면 스테이지 전환을 요청. 먼저 누른 쪽은 대기 자막.
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

    // 거울이 화면 밖에 있어도 보이는 유일한 신호. 여기서 거울로 부르지 못하면 전환이 영영 안 시작된다
    [Tooltip("양쪽 완성 순간. 방 어디에 있든 뜸")]
    [SerializeField]
    private string bothClearedMessage = "상대도 거울을 완성했다. 거울을 확인해보자.";

    [Tooltip("내가 먼저 거울을 본 뒤 상대를 기다리는 동안. 사라지지 않고 계속 떠 있음")]
    [SerializeField]
    private string peerWaitMessage = "상대가 거울을 확인하기를 기다리는 중…";

    [Header("Debug")]
    [Tooltip("네트워크 없이 단독 실행할 때 내 완성만으로 양쪽 완성 취급. 씬에는 꺼둔 채로 저장할 것")]
    [SerializeField] private bool treatMineAsBothLocally = false;

    /// 직전에 적용한 상태. null 이면 아직 한 번도 적용 안 함
    private bool? applied;

    /// 잠깐 떴다 사라지는 자막. 대기 안내와 양쪽 완성 알림이 같이 씀
    private Coroutine transientRoutine;

    /// 상대 대기 자막이 떠 있는지. 같은 UI 를 쓰므로 잠깐 자막보다 이쪽이 우선
    private bool peerWaitShown;

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

    private void Update()
    {
        Apply(BothDone);
        ApplyPeerWait();
    }

    /// <summary>
    /// 완성 거울을 볼 때(2D 클릭 · 3D E키).
    /// 상대가 아직이면 안내 문구, 양쪽 완성 뒤면 스테이지 전환 요청.
    /// </summary>
    public void RequestInspect()
    {
        if (!MineDone || noticeRoot == null) return;

        if (!BothDone)
        {
            ShowTransient(waitingMessage);
            return;
        }

        // 단독 실행
        if (GameSession.Instance == null || RoomService.Instance == null) return;

        // 전환 요청
        GameSession.Instance.RpcRequestStageChange(RoomService.Instance.IsHost);
    }

    /// 내가 요청한 뒤 상대를 기다리는 동안 안내 UI.
    private void ApplyPeerWait()
    {
        if (noticeRoot == null) return;

        GameSession session = GameSession.Instance;
        bool waiting = session != null
                       && RoomService.Instance != null
                       && session.HasRequestedStageChange(RoomService.Instance.IsHost)
                       && !session.BothStageReady;

        if (waiting == peerWaitShown) return;
        peerWaitShown = waiting;

        // 양쪽 준비 시 없앰
        HideTransient();
        if (!waiting) return;

        if (noticeText != null) noticeText.text = peerWaitMessage;
        noticeRoot.gameObject.SetActive(true);
        noticeRoot.alpha = 1f;
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

    /// <summary>잠깐 떴다 사라지는 자막. 상대 대기 자막이 떠 있으면 무시.</summary>
    private void ShowTransient(string message)
    {
        if (peerWaitShown) return;

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

    /// <summary>떠 있는 자막을 즉시 없앰.</summary>
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
