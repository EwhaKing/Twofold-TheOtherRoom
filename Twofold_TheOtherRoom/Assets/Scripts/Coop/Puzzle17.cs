using Fusion;
using UnityEngine;


public class Puzzle17 : CoopPuzzle
{
    public const string Key = "coop-17";   // CoopPuzzle.Find 로 찾을 때 쓰는 이름
    public const string Id2D = "2D-17";     // 2D 가 PuzzleManager 에 보고할 때
    public const string Id3D = "3D-17";     // 3D 가 PuzzleManager 에 보고할 때

    public override string PuzzleId => Key;

    /// <summary>
    /// 배관 퍼즐 제한 시간
    /// </summary>
    public const float WaterSeconds = 60f;


    // =========================================================
    // 공유 상태
    // =========================================================

    /// <summary>
    /// 3D 플레이어가 밸브를 눌렀는지
    ///
    /// true가 되면 2D에서는
    /// - 해당 방이면 배관 퍼즐 배치본 깜빡임
    /// - 다른 방이면 이동 화살표 깜빡임
    /// </summary>
    [Networked, OnChangedRender(nameof(RaiseChanged))]
    public bool Activated { get; private set; }


    /// <summary>
    /// 2D 플레이어가 배관 퍼즐을 실제로 클릭한 시점
    /// 0이면 아직 제한시간이 시작되지 않은 상태
    /// </summary>
    [Networked]
    private int StartTick { get; set; }

    [Networked]
    private int SolvedTick { get; set; }

    /// <summary>
    /// 배관 퍼즐 성공 여부
    /// </summary>
    [Networked, OnChangedRender(nameof(RaiseChanged))]
    public bool Solved { get; private set; }


    /// <summary>
    /// 2D 플레이어가 퍼즐을 클릭해서
    /// 제한시간이 시작되었는지
    /// </summary>
    public bool Started => StartTick != 0;


    /// <summary>
    /// 남은 시간 비율
    ///
    /// 1 = 시간 가득 남음
    /// 0 = 제한시간 종료
    /// </summary>
    public float WaterFill
    {
        get
        {
            if (!Started)
                return 1f;

            int currentTick =
                Solved && SolvedTick != 0
                    ? SolvedTick
                    : Runner.Tick;

            float elapsed =
                (currentTick - StartTick) * Runner.DeltaTime;

            return Mathf.Clamp01(
                1f - elapsed / WaterSeconds
            );
        }
    }

    /// <summary>
    /// 제한시간이 끝났는데
    /// 아직 배관 연결에 성공하지 못한 상태
    /// </summary>
    public bool Failed =>
        Started &&
        !Solved &&
        WaterFill <= 0f;


    /// <summary>
    /// 공유 상태가 변경되었을 때 사용하는 이벤트
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
    /// 3D:
    /// 우물 밸브를 눌렀을 때 호출
    ///
    /// 여기서는 타이머를 시작하지 않고
    /// 2D에게 퍼즐 활성화 사실만 전달한다.
    /// </summary>
    public void OpenValve()
    {
        RpcOpenValve();
    }


    /// <summary>
    /// 2D:
    /// 플레이어가 배관 퍼즐 배치본을 클릭했을 때 호출
    ///
    /// 이 순간부터 제한시간이 시작된다.
    /// </summary>
    public void StartPuzzle()
    {
        RpcStartPuzzle();
    }


    /// <summary>
    /// 2D:
    /// 배관을 시작점부터 배수구까지
    /// 완전히 연결했을 때 호출
    /// </summary>
    public void ReportConnected()
    {
        RpcReportConnected();
    }


    // =========================================================
    // Fusion RPC
    // =========================================================

    /// <summary>
    /// 3D 밸브 클릭
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcOpenValve()
    {
        // 이미 활성화된 퍼즐이면 다시 처리하지 않음
        if (Activated)
            return;

        Activated = true;
    }


    /// <summary>
    /// 2D 플레이어가 실제 퍼즐을 열었을 때
    /// 제한시간 시작
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcStartPuzzle()
    {
        // 3D에서 아직 밸브를 누르지 않았다면 시작 불가
        if (!Activated)
            return;

        // 이미 시작된 경우 다시 시간 초기화하지 않음
        if (StartTick != 0)
            return;

        StartTick = Runner.Tick;
    }


    /// <summary>
    /// 배관 연결 성공
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcReportConnected()
    {
        if (!Started)
            return;

        if (Failed)
            return;

        if (Solved)
            return;

        SolvedTick = Runner.Tick;
        Solved = true;
    }

    // =========================================================
    // 혼자 테스트할 때 사용하는 디버그 메뉴
    // =========================================================

    /// <summary>
    /// 3D에서 밸브를 눌렀다고 가정
    /// </summary>
    [ContextMenu("디버그: 밸브 열기")]
    private void DebugOpenValve()
    {
        OpenValve();
    }


    /// <summary>
    /// 2D에서 배관 퍼즐을 클릭했다고 가정
    /// </summary>
    [ContextMenu("디버그: 퍼즐 시작")]
    private void DebugStartPuzzle()
    {
        StartPuzzle();
    }


    /// <summary>
    /// 배관 연결에 성공했다고 가정
    /// </summary>
    [ContextMenu("디버그: 성공 처리")]
    private void DebugSolve()
    {
        ReportConnected();
    }
}