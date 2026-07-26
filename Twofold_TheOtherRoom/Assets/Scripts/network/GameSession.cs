using Fusion;
using UnityEngine;

/// <summary>
/// 방 전체가 공유하는 네트워크 상태 컨테이너
/// 방장이 방에 들어오면 생성, StateAuthority 가진 방장만 값 쓰기 가능.
/// </summary>
public class GameSession : NetworkBehaviour
{
    // 어디든 참조 가능
    public static GameSession Instance { get; private set; }

    // 0 = 모드1, 1 = 모드2. 방장만 쓰기 가능
    [Networked] public int Mode { get; set; }

    // 방장이 시작을 누르면 true. 전원이 값 변화를 감지해 각자 씬을 로드.
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

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(StartRequested):
                    if (StartRequested)
                        RoomManager.Instance?.BeginGameplay(Mode);
                    break;
            }
        }
    }

    // 아래 두 메서드는 방장 UI(모드 토글 / 시작 버튼)에서만 호출
    // StateAuthority 체크로 이중 방어

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
}
