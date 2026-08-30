using System.Collections;
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

    [Tooltip("더빙 · 자막. 비워두면 블링크만 재생")]
    [SerializeField] private IntroNarration narration;

    [Tooltip("게임플레이 BGM. 씬 진입과 동시에 재생")]
    [SerializeField] private BGMType gameplayBGM = BGMType.WhiteNoise;

    [Tooltip("인트로 동안 끌 게임 UI. 씬에는 켜둔 채로 저장할 것")]
    [SerializeField] private GameObject[] gameplayUI;

    [Tooltip("인트로 동안만 켤 오브젝트. 클릭 차단막 · 자막.\n" +
             "씬에는 꺼둔 채로 저장할 것 — 시퀀서가 죽어도 씬이 안 잠기도록")]
    [SerializeField] private GameObject[] introOnly;

    [Tooltip("인트로 동안 끌 입력 컴포넌트. 씬에는 켜둔 채로 저장할 것 — 꺼진 채면 복구 안 됨.\n" +
        "비워두면 PlayerController · PlayerLocomotionInput · PlayerInteractor 를 자동으로 찾음")]
    [SerializeField] private Behaviour[] inputToLock;

    [Header("Timing")]
    [Tooltip("눈 깜빡임 구간 길이(초). GameSession.IntroSeconds 예산 안에 들어가야 함")]
    [SerializeField] private float blinkSeconds = 7f;

    [Tooltip("나레이션 시작 시각(초). 인트로 시작 기준. 블링크와 겹치려면 blinkSeconds 보다 작게")]
    [SerializeField] private float narrationStartSeconds = 7f;

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

    private bool heartBeatPlayed = false;
    private bool finalTickTokPlayed = false;

    private readonly PlayerControlLock playerControlLock = new PlayerControlLock();

    private void Start()   // Awake 아님 — BlinkIntroPlayer.Awake 가 머티리얼 복사본을 먼저 만들어야 함
    {
        if (blink == null)
        {
            Debug.LogError("[Intro] blink 인스펙터 참조 연결할 것", this);
            enabled = false;
            return;
        }

        SoundManager.Instance?.StopBGM();              // TODO: Room BGM 정해지면 이 줄 삭제
        _local = GameSession.Instance == null;

        // 단독 실행은 스킵. 퍼즐 작업자가 씬을 그냥 열어 테스트할 수 있어야 함
        if (_local && !playLocallyWithoutNetwork)
        {
            Apply(IntroPhase.Done);
            enabled = false;
            return;
        }

        if (PlaybackSeconds > GameSession.IntroSeconds)
            Debug.LogError($"[Intro] 연출 {PlaybackSeconds}초가 인트로 예산 {GameSession.IntroSeconds}초를 넘음 " +
                           "— 연출이 끝나기 전에 타이머가 시작됨", this);

        Apply(Phase);
    }

    /// 방을 나가거나 씬이 내려갈 때 입력 잠금 해제
    private void OnDisable()
    {
        if (_applied != IntroPhase.Done) SetIntroActive(false);
    }

    private void Update()
    {
        if (_local) _localElapsed += Mathf.Min(Time.unscaledDeltaTime, MaxLocalStep);
        Apply(Phase);
        CheckFinalTickTok();
    }

    private void CheckFinalTickTok() /// 게임종료 5초 전 TickTok 재생
    {
        if (finalTickTokPlayed)
            return;

        GameSession session = GameSession.Instance;

        if (session == null)
            return;

        if (session.Intro != IntroPhase.Done)
            return;

        float remainingSeconds =
            GameSession.TotalSeconds - session.ElapsedSeconds;

        if (remainingSeconds <= 5f && remainingSeconds > 0f)
        {
            finalTickTokPlayed = true;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.TickTok);
            }
        }
    }

    /// 시계로 판정한 현재 단계
    private IntroPhase Phase
    {
        get
        {
            if (_local) return Elapsed < PlaybackSeconds ? IntroPhase.Running : IntroPhase.Done;

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

    /// 일시정지 여부. 오디오는 시계로 굴릴 수 없어 별도 전달
    private bool Paused
    {
        get
        {
            if (_local) return false;

            var session = GameSession.Instance;
            return session != null && session.IsPaused;
        }
    }

    /// 연출 실제 길이. 예산 검사와 로컬 미리보기 종료 판정용
    private float PlaybackSeconds =>
        Mathf.Max(blinkSeconds, narrationStartSeconds + (narration != null ? narration.Length : 0f));

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
                if (entered)
                {
                    SetIntroActive(true);
                    if (!heartBeatPlayed && SoundManager.Instance != null)
                    {
                        heartBeatPlayed = true;
                        SoundManager.Instance.PlaySFX(SFXType.HeartBeat);
                    }
                }

                // 블링크가 끝나면 즉시 반납. 나레이션 내내 풀스크린 패스를 물고 있지 않도록
                if (Elapsed < blinkSeconds) blink.ApplyNormalizedTime(Elapsed / blinkSeconds);
                else blink.Finish();

                if (narration != null) narration.ApplyTime(Elapsed - narrationStartSeconds, Paused);
                break;

            case IntroPhase.Done:
                if (!entered) return;
                blink.Finish();

                if (narration != null)
                    narration.Finish();

                SetIntroActive(false);

                // TickTok 재생 후 WhiteNoise 시작
                StartCoroutine(StartGameAudio());
                break;
        }
    }

    private IEnumerator StartGameAudio()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.TickTok);
        }

        // TickTok이 약 5초 동안 재생
        yield return new WaitForSecondsRealtime(5f);

        // TickTok이 끝난 뒤 WhiteNoise 시작
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(BGMType.WhiteNoise);
        }
    }

    /// 인트로 화면 상태. 씬이 잘못 저장돼 있어도 여기서 바로잡힘
    private void SetIntroActive(bool active)
    {
        if (active)
        {
            blink.Begin();
            if (narration != null) narration.Begin();
        }

        foreach (var go in gameplayUI)
            if (go != null) go.SetActive(!active);

        foreach (var go in introOnly)
            if (go != null) go.SetActive(active);

        if (active)
            playerControlLock.Lock(this, inputToLock);
        else playerControlLock.Unlock();
    }
}
