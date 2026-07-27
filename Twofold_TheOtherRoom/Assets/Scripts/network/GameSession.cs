using Fusion;
using UnityEngine;

/// <summary>방이 지금 어느 단계인지. 양쪽이 이 값으로 화면을 맞춘다.</summary>
public enum RoomPhase
{
    Lobby = 0,        // 대기방
    ModeSelect = 1,   // 방장은 모드 선택, 게스트는 대기 화면
    Playing = 2,      // 게임플레이 씬 로드됨
}

/// <summary>
/// 방 전체가 공유하는 네트워크 상태. 방장이 Spawn하고 방장만 씀.
/// 두 사람이 같은 값을 봐야 하는 것만 [Networked]로 둘 것.
/// 타이머 / 거울 조각 추가 방법은 ARCHITECTURE.md 참고.
/// </summary>
public class GameSession : NetworkBehaviour
{
    // Spawn된 뒤에만 유효. 항상 null 검사할 것
    public static GameSession Instance { get; private set; }

    /// 0 = 모드1, 1 = 모드2. 방장이 모드 선택 화면에서 확정
    [Networked] public int Mode { get; set; }

    /// 방 단계. 바뀌는 순간 양쪽 GameFlow가 화면을 바꿈
    [Networked] public RoomPhase Phase { get; set; }

    /// 방장 이름. 방장이 Spawned에서 자기 걸 씀
    [Networked] public NetworkString<_16> HostName { get; set; }

    /// 게스트 이름. 게스트가 RPC로 요청하면 방장이 씀. 비어 있으면 아직 안 들어온 것
    [Networked] public NetworkString<_16> GuestName { get; set; }

    ChangeDetector _changes;

    public override void Spawned()
    {
        Instance = this;
        _changes = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);

        // [Networked]는 StateAuthority(방장)만 쓸 수 있어서, 게스트는 RPC로 부탁한다
        if (Object.HasStateAuthority)
        {
            HostName = PlayerProfile.Nickname;
            Phase    = RoomPhase.Lobby;
        }
        else
        {
            RpcSubmitName(PlayerProfile.Nickname);
        }
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

    /// 모드 선택 화면의 "뒤로가기" — 둘 다 로비로 되돌림
    public void CancelModeSelect()
    {
        if (Object.HasStateAuthority)
            Phase = RoomPhase.Lobby;
    }

    /// 알림창에서 "예" — 모드를 확정하고 바로 게임 시작
    public void ConfirmMode(int mode)
    {
        if (!Object.HasStateAuthority) return;
        Mode  = mode;
        Phase = RoomPhase.Playing;
    }

    /// 게스트가 나갔을 때. 이름을 지우고 방장을 로비로 되돌림
    public void HandleGuestLeft()
    {
        if (!Object.HasStateAuthority) return;
        GuestName = string.Empty;
        Phase     = RoomPhase.Lobby;
    }

    // ---------- 게스트 → 방장 ----------

    /// 게스트는 [Networked] 값을 직접 못 쓰므로 방장에게 이름을 대신 써 달라고 보낸다
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSubmitName(NetworkString<_16> name, RpcInfo info = default)
    {
        if (info.Source == Object.StateAuthority)
            HostName = name;
        else
            GuestName = name;
    }

    // 타이머 / 거울 조각이 여기 들어옴. 필드 이름과 틱 계산법은 ARCHITECTURE.md.
    // 주의: [Networked]는 방장만 쓸 수 있음. 게스트가 바꿔야 하는 값은 위 RPC처럼 요청해야 함.
}
