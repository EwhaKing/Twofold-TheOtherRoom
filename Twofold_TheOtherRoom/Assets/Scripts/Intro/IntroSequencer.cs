using UnityEngine;

/// <summary>
/// 공유 시계를 읽어 인트로 상태를 매 프레임 다시 적용.
///
/// 전이 이벤트가 아니라 상태 재적용이라 한 프레임 놓쳐도 다음 프레임에 스스로 복구.
/// 게임 상태에 따라 연출을 그림.
///
/// 게임플레이 씬마다 하나. GameSession 이 없으면(단독 실행) 인트로 없이 바로 시작.
/// </summary>
public class IntroSequencer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BlinkIntroPlayer blink;

    [Tooltip("인트로 동안 끌 게임 UI. 씬에는 켜둔 채로 저장할 것")]
    [SerializeField] private GameObject[] gameplayUI;

    [Tooltip("인트로 동안만 켤 오브젝트. 클릭 차단막 · 자막.\n" +
             "씬에는 꺼둔 채로 저장할 것 — 시퀀서가 죽어도 씬이 안 잠기도록")]
    [SerializeField] private GameObject[] introOnly;

    [Header("Timing")]
    [Tooltip("눈 깜빡임 구간 길이(초). GameSession.IntroSeconds 예산 안에 들어가야 함")]
    [SerializeField] private float blinkSeconds = 7f;

    [Header("Debug")]
    [Tooltip("네트워크 없이 단독 실행할 때도 연출 재생. 미리보기용이므로 씬에는 꺼둔 채로 저장할 것")]
    [SerializeField] private bool playLocallyWithoutNetwork = false;

    /// 프레임당 로컬 진행 상한. 스톨 한 번에 연출이 통째로 건너뛰지 않도록
    private const float MaxLocalStep = 0.1f;

    /// 네트워크 없는 단독 실행
    private bool _local;

    /// 로컬 재생 진행 시간. 미리보기라 시계가 아니라 누적으로 감
    private float _localElapsed;

    /// 직전에 적용한 단계. 구간 진입 시 한 번만 해야 할 일 구분용
    private IntroPhase? _applied;

    private void Start()   // Awake 아님 — BlinkIntroPlayer.Awake 가 머티리얼 복사본을 먼저 만들어야 함
    {
        if (blink == null)
        {
            Debug.LogError("[Intro] blink 인스펙터 참조 연결할 것", this);
            enabled = false;
            return;
        }

        _local = GameSession.Instance == null;

        // 단독 실행은 스킵. 퍼즐 작업자가 씬을 그냥 열어 테스트할 수 있어야 함
        if (_local && !playLocallyWithoutNetwork)
        {
            Apply(IntroPhase.Done);
            enabled = false;
            return;
        }

        if (blinkSeconds > GameSession.IntroSeconds)
            Debug.LogError($"[Intro] 블링크 {blinkSeconds}초가 인트로 예산 {GameSession.IntroSeconds}초를 넘음 " +
                           "— 연출이 끝나기 전에 타이머가 시작됨", this);

        Apply(Phase);
    }

    private void Update()
    {
        if (_local) _localElapsed += Mathf.Min(Time.unscaledDeltaTime, MaxLocalStep);
        Apply(Phase);
    }

    /// 시계로 판정한 현재 단계
    private IntroPhase Phase
    {
        get
        {
            if (_local) return Elapsed < blinkSeconds ? IntroPhase.Running : IntroPhase.Done;

            var session = GameSession.Instance;
            return session == null ? IntroPhase.Done : session.Intro;   // 방을 나가면 Instance 가 사라짐
        }
    }

    /// 인트로 시작부터 흐른 시간. 일시정지 구간은 빠져 있음
    private float Elapsed
    {
        get
        {
            if (_local) return _localElapsed;

            var session = GameSession.Instance;
            return session == null ? 0f : session.SinceStartSeconds;
        }
    }

    private void Apply(IntroPhase phase)
    {
        bool entered = _applied != phase;
        _applied = phase;

        switch (phase)
        {
            // 상대 로드 대기. 먼저 로드한 쪽이 씬을 돌아다니지 못하게 눈 감긴 채로 잡아둠
            case IntroPhase.Waiting:
                if (entered) SetIntroActive(true);
                blink.ApplyNormalizedTime(0f);
                break;

            case IntroPhase.Running:
                if (entered) SetIntroActive(true);
                blink.ApplyNormalizedTime(Elapsed / blinkSeconds);
                break;

            case IntroPhase.Done:
                if (!entered) return;
                blink.Finish();
                SetIntroActive(false);
                break;
        }
    }

    /// 인트로 화면 상태. 씬이 잘못 저장돼 있어도 여기서 바로잡힘
    private void SetIntroActive(bool active)
    {
        if (active) blink.Begin();

        foreach (var go in gameplayUI)
            if (go != null) go.SetActive(!active);

        foreach (var go in introOnly)
            if (go != null) go.SetActive(active);
    }
}
