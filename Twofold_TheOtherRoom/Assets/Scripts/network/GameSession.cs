using Fusion;
using UnityEngine;

/// <summary>
/// 방 전체가 공유하는 네트워크 상태. 방장이 Spawn하고 방장만 씀.
/// 두 사람이 같은 값을 봐야 하는 것만 [Networked]로 둘 것.
/// 타이머 / 거울 조각 추가 방법은 ARCHITECTURE.md 참고.
/// </summary>
public class GameSession : NetworkBehaviour
{
    // Spawn된 뒤에만 유효. 항상 null 검사할 것
    public static GameSession Instance { get; private set; }

    /// 0 = 모드1, 1 = 모드2
    [Networked] public int Mode { get; set; }

    /// 방장이 시작을 누르면 true. 전원이 감지해 각자 씬 로드
    [Networked] public bool StartRequested { get; set; }

    ChangeDetector _changes;

    public override void Spawned()
    {
        Instance = this;
        _changes = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// 값이 바뀌는 순간 한 번만 해야 할 일만 여기 둠.
    /// Mode처럼 매 프레임 읽어도 되는 값은 View가 직접 가져옴.
    /// </summary>
    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(StartRequested):
                    if (StartRequested)
                        GameFlow.Instance?.BeginGameplay(Mode);
                    break;
            }
        }
    }

    // ---------- 방장만 호출 ----------
    // 게스트가 불러도 아무 일 없음

    public void SetMode(int mode)
    {
        if (Object.HasStateAuthority)
            Mode = mode;
    }

    public void RequestStart()
    {
        if (Object.HasStateAuthority)
            StartRequested = true;
    }

    // 타이머 / 거울 조각이 여기 들어옴. 필드 이름과 틱 계산법은 ARCHITECTURE.md.
    // 주의: [Networked]는 방장만 쓸 수 있음. 게스트가 바꿔야 하는 값은 RPC로 요청해야 함.
}
