using Fusion;
using UnityEngine;

public class Puzzle11 : CoopPuzzle
{
    public const string Key = "coop-11";
    public const string Id2D = "2D-11";
    public const string Id3D = "3D-11";

    public override string PuzzleId => Key;

    [Networked] public bool Cleared3D { get; private set; }

    public void ReportCleared3D()
    {
      RPCCleared3D();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPCCleared3D()
    {
      Cleared3D = true;
    }

    [ContextMenu("디버그: 3D 거울 화면")]
    private void DebugCleared3D()
    {
      RPCCleared3D();
    }
}