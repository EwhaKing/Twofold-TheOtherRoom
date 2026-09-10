using Fusion;
using UnityEngine;

/// <summary>
/// 2D-17 배관 퍼즐 공유 상태
/// 3D에서 밸브를 누르면 타이머가 시작되고,
/// 2D에서 배관 연결에 성공하면 퍼즐이 해결된다.
/// </summary>
public class Puzzle17 : CoopPuzzle
{
    public const string Id = "2D-17";

    public override string PuzzleId => Id;

    /// <summary>
    /// 제한 시간: 60초
    /// </summary>
    public const float WaterSeconds = 60f;

    // 밸브를 누른 시점
    [Networked]
    int StartTick { get; set; }

    // 배관 퍼즐 성공 여부
    [Networked, OnChangedRender(nameof(RaiseChanged))]
    public bool Solved { get; private set; }

    /// <summary>
    /// 현재 남아 있는 물의 양
    /// 1 = 물이 가득 참
    /// 0 = 물이 모두 빠짐
    ///
    /// 네트워크로 물의 양 자체를 보내지 않고
    /// StartTick을 기준으로 각 클라이언트가 계산한다.
    /// </summary>
    public float WaterFill
    {
        get
        {
            return Mathf.Clamp01(
                1f - SecondsSince(StartTick) / WaterSeconds
            );
        }
    }

    /// <summary>
    /// 제한 시간이 끝났는데 아직 성공하지 못한 상태
    /// </summary>
    public bool Failed =>
        !Solved &&
        StartTick != 0 &&
        WaterFill <= 0f;

    /// <summary>
    /// 상태가 변경되었을 때 2D/3D 쪽에서 사용할 이벤트
    /// </summary>
    public event System.Action Changed;

    private void RaiseChanged()
    {
        Changed?.Invoke();
    }


    // =========================================================
    // 3D / 2D에서 사용하는 공개 API
    // =========================================================

    /// <summary>
    /// 3D: 우물의 밸브를 눌렀을 때 호출
    /// </summary>
    public void OpenValve()
    {
        RpcOpenValve();
    }

    /// <summary>
    /// 2D: 배관을 시작점부터 배수구까지 연결했을 때 호출
    /// </summary>
    public void ReportConnected()
    {
        RpcReportConnected();
    }


    // =========================================================
    // Fusion RPC
    // =========================================================

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcOpenValve()
    {
        // 처음 밸브를 눌렀을 때만 시작
        if (StartTick == 0)
        {
            StartTick = Runner.Tick;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcReportConnected()
    {
        // 제한 시간 안에 연결 성공한 경우
        if (!Failed)
        {
            Solved = true;
        }
    }


    // =========================================================
    // 혼자 테스트할 때 사용하는 디버그 메뉴
    // =========================================================

    [ContextMenu("디버그: 밸브 열기")]
    private void DebugOpenValve()
    {
        OpenValve();
    }

    [ContextMenu("디버그: 성공 처리")]
    private void DebugSolve()
    {
        ReportConnected();
    }
}