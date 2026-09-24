using Fusion;
using UnityEngine;

/// <summary>
/// 지도(2D-13) / 지구본(3D-13) 좌표 맞추기 연동 퍼즐 공유 상태.
///
/// 정답 판정은 각 뷰가 함. 여기는 '맞췄다/틀렸다'만 받음.
/// </summary>
public class Puzzle13 : CoopPuzzle
{
    public const string Key = "coop-13";   // CoopPuzzle.Find로 찾을 때
    public const string Id2D = "2D-13";    // 2D가 PuzzleManager에 보고할 때
    public const string Id3D = "3D-13";    // 3D가 PuzzleManager에 보고할 때

    public override string PuzzleId => Key;


    #region 공유 상태

    /// 2D 정답 상태
    [Networked] private bool Matched2D { get; set; }

    /// 3D 정답 상태
    [Networked] private bool Matched3D { get; set; }

    #endregion


    #region 2D/3D가 매 프레임 읽는 값

    /// <summary>양쪽이 같은 좌표. 한번 true시 고정.</summary>
    public bool Solved => Matched2D && Matched3D;

    #endregion


    #region 2D/3D가 부르는 API

    /// <summary>자기 축이 정답인지 보고. 값이 바뀔 때만 부르면 됨</summary>
    public void ReportMatch(PuzzleDimension side, bool matched)
    {
        RpcReportMatch(side == PuzzleDimension.TwoD, matched);
    }

    #endregion


    #region Fusion RPC

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcReportMatch(bool isTwoD, bool matched)
    {
        if (Solved) return;

        if (isTwoD) Matched2D = matched;
        else Matched3D = matched;
    }


    /// <summary>디버그 전용. 스폰하지 않은 쪽도 쓰도록 RPC</summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcDebugSetMatched(bool twoD, bool threeD)
    {
        Matched2D = twoD;
        Matched3D = threeD;
    }

    #endregion


    #region 한쪽 테스트 디버그 메뉴

    [ContextMenu("디버그: 2D만 맞춤")]
    private void DebugMatch2D()
    {
        RpcDebugSetMatched(true, false);
    }


    [ContextMenu("디버그: 3D만 맞춤")]
    private void DebugMatch3D()
    {
        RpcDebugSetMatched(false, true);
    }


    [ContextMenu("디버그: 양쪽 맞춤")]
    private void DebugSolve()
    {
        RpcDebugSetMatched(true, true);
    }


    [ContextMenu("디버그: 처음으로")]
    private void DebugReset()
    {
        RpcDebugSetMatched(false, false);
    }

    #endregion
}
