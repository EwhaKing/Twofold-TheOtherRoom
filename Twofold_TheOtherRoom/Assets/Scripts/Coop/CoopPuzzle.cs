using System;
using System.Collections.Generic;
using Fusion;

/// <summary>연동 퍼즐 공통 베이스. 스폰돼 있는 동안만 사는 방 공유 상태.</summary>
public abstract class CoopPuzzle : NetworkBehaviour
{
    public abstract string PuzzleId { get; }

    static readonly Dictionary<string, CoopPuzzle> _live = new();

    /// 어느 쪽에서 스폰했든 양쪽에서 뜸
    public static event Action<CoopPuzzle> OnRegistered;
    public static event Action<CoopPuzzle> OnUnregistered;

    public static T Find<T>(string id) where T : CoopPuzzle
        => _live.TryGetValue(id, out var p) ? p as T : null;

    public override void Spawned()
    {
        _live[PuzzleId] = this;
        OnRegistered?.Invoke(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (_live.TryGetValue(PuzzleId, out var p) && p == this) _live.Remove(PuzzleId);
        OnUnregistered?.Invoke(this);
    }

    /// 그 tick 이후 흐른 시간. 양쪽이 같은 값을 봄
    protected float SecondsSince(int tick)
        => tick == 0 ? 0f : (Runner.Tick - tick) * Runner.DeltaTime;
}
