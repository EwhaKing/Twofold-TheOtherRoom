using System;

/// <summary>
/// 스테이지 전환 연출. 2D/3D 가 각자 구현하고 StageChangeDriver가 구동.
/// 암전으로 끝낼 것 — 다음 씬은 눈 감긴 채로 시작.
/// </summary>
public interface IStageCutscene
{
    /// 재생 중에 다시 불러도 무시할 것
    void Play();

    bool IsPlaying { get; }

    /// 암전까지 끝난 시점
    event Action Finished;
}
