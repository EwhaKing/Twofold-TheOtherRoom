using Fusion;
using UnityEngine;

/// <summary>방이 지금 어느 단계인지. 양쪽이 이 값으로 화면을 맞춘다.</summary>
public enum RoomPhase
{
    Lobby = 0,        // 대기방
    ModeSelect = 1,   // 방장은 모드 선택, 게스트는 대기 화면
    Playing = 2,      // 게임플레이 씬 로드됨
}

/// <summary>인트로 진행 단계. 시계로만 판정.</summary>
public enum IntroPhase
{
    Waiting = 0,   // 상대 로드 대기. 눈 감긴 채 정지
    Running = 1,   // 연출 재생
    Done    = 2,   // 연출 끝. 게임 진행
}

/// <summary>
/// 방 전체가 공유하는 네트워크 상태. 방장이 Spawn하고 방장만 씀.
/// 두 사람이 같은 값을 봐야 하는 것만 [Networked]로 둘 것.
/// </summary>
public class GameSession : NetworkBehaviour
{
    // Spawn된 뒤에만 유효. 항상 null 검사할 것
    public static GameSession Instance { get; private set; }

    /// 0 = 모드1, 1 = 모드2. 방장이 모드 선택 화면에서 확정
    [Networked] public int Mode { get; set; }

    /// 방 단계. 바뀌는 순간 양쪽 GameFlow가 화면을 바꿈
    [Networked] public RoomPhase Phase { get; set; }

    // 타이머 - 기본
    [Networked] public bool P1Loaded { get; set; }
    [Networked] public bool P2Loaded { get; set; }
    [Networked] public int StartedTick { get; set; }
    public const float TotalSeconds = 15f * 60f;

    /// 인트로 길이. 실제 연출 소요와 무관한 고정 예산
    public const float IntroSeconds = 7f;

    /// 로딩 완료 후 흐른 시간. 인트로 포함
    public float SinceStartSeconds // Timer가 부를 때마다 로컬마다 지난 시간 계산해서 보내줌
    {
        get
        {
            if (StartedTick == 0) return 0f;
            int now = IsPaused ? PausedTick : Runner.Tick;
            return (now - StartedTick - TotalPausedTicks) * Runner.DeltaTime;
        }
    }

    /// 인트로 이후 흐른 시간. 제한시간 차감 기준
    public float ElapsedSeconds => Mathf.Max(0f, SinceStartSeconds - IntroSeconds);

    /// 인트로 단계. StartedTick 이 0이면 상대 대기 중
    public IntroPhase Intro
    {
        get
        {
            if (StartedTick == 0) return IntroPhase.Waiting;
            return SinceStartSeconds < IntroSeconds ? IntroPhase.Running : IntroPhase.Done;
        }
    }

    // 타이머 - 일시정지
    [Networked] public bool IsPaused { get; set; }
    [Networked] public int PausedTick { get; set; }
    [Networked] public int TotalPausedTicks { get; set; }
    [Networked] public PlayerRef PausedBy { get; set; }

    ChangeDetector _changes;

    public override void Spawned()
    {
        Instance = this;
        _changes = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);

        if (Object.HasStateAuthority)
            Phase = RoomPhase.Lobby;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// 값이 바뀌는 순간 한 번만 해야 할 일만 여기 둠.
    /// Mode나 이름처럼 매 프레임 읽어도 되는 값은 View가 직접 가져옴.
    /// </summary>
    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(Phase):
                    GameFlow.Instance?.ApplyPhase(Phase);
                    break;
            }
        }
    }

    // ---------- 방장만 호출 ----------
    // 게스트가 불러도 아무 일 없음

    /// 로비의 "게임 시작" — 방장은 모드 선택으로, 게스트는 대기 화면으로
    public void RequestModeSelect()
    {
        if (Object.HasStateAuthority)
            Phase = RoomPhase.ModeSelect;
    }

    /// 알림창에서 "예" — 모드를 확정하고 바로 게임 시작
    public void ConfirmMode(int mode)
    {
        if (!Object.HasStateAuthority) return;
        ResetTimer();
        Mode  = mode;
        Phase = RoomPhase.Playing;
    }

    /// 게스트가 나갔을 때. 모드 선택 중이었다면 방장을 로비로 되돌림
    public void HandleGuestLeft()
    {
        if (Object.HasStateAuthority)
            Phase = RoomPhase.Lobby;
    }

    // 씬 로드 완료 시 호출하는 RPC
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcReportLoaded(bool isHost)
    {
        if(isHost) P1Loaded = true;
        else P2Loaded = true;

        if(P1Loaded && P2Loaded && StartedTick == 0) StartedTick = Runner.Tick;
    }

    // 타이머 초기화
    private void ResetTimer()
    {
        P1Loaded = false;
        P2Loaded = false;
        StartedTick = 0;
        IsPaused = false;
        PausedTick = 0;
        TotalPausedTicks = 0;
        PausedBy = PlayerRef.None;
    }

    // 일시정지 시 호출하는 RPC
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestPause(RpcInfo info = default)
    {
        if(IsPaused != false) return;
        PausedTick = Runner.Tick;
        IsPaused = true;
        PausedBy = info.Source;
    }

    // 재개 시 호출하는 RPC
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestResume(RpcInfo info = default)
    {
        if(info.Source != PausedBy || IsPaused != true) return; // 멈춘 플레이어만 재개 가능
        IsPaused = false;
        TotalPausedTicks += Runner.Tick - PausedTick;
    }
}
