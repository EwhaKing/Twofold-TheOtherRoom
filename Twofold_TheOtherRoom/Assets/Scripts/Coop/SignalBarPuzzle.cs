using Fusion;
using UnityEngine;

/// <summary>단계 정답</summary>
[System.Serializable]
public struct SignalBarStage
{
    [Tooltip("전광판에 뜨는 숫자이자 단계 안에서의 막대 번호. 1~4")]
    [Range(1, 4)] public int answer;

    [Tooltip("숫자를 띄우는 쪽. 막대는 반대쪽이 누름")]
    public PuzzleDimension hintSide;
}

/// <summary>
/// 막대 20개 + 전광판 연동 퍼즐 (2D-18 / 3D-18) 공유 상태.
/// 숫자가 뜬 전광판의 반대쪽이 그 번호의 막대를 누름.
///
/// 스폰: 2D, 씬 진입 시 1회. 실패해도 재스폰 없음.
/// </summary>
public class SignalBarPuzzle : CoopPuzzle
{
    public const string Key = "coop-18";   // CoopPuzzle.Find로 찾을 때
    public const string Id2D = "2D-18";    // 2D가 PuzzleManager에 보고할 때
    public const string Id3D = "3D-18";    // 3D가 PuzzleManager에 보고할 때

    public override string PuzzleId => Key;

    /// <summary>한 단계에 묶이는 막대 수</summary>
    public const int BarsPerStage = 4;


    #region 프리팹 설정값

    [Tooltip("단계별 정답")]
    [SerializeField]
    private SignalBarStage[] stages =
    {
        new SignalBarStage { answer = 1, hintSide = PuzzleDimension.ThreeD },
        new SignalBarStage { answer = 3, hintSide = PuzzleDimension.TwoD   },
        new SignalBarStage { answer = 2, hintSide = PuzzleDimension.ThreeD },
        new SignalBarStage { answer = 4, hintSide = PuzzleDimension.TwoD   },
        new SignalBarStage { answer = 1, hintSide = PuzzleDimension.ThreeD },
    };

    [Tooltip("다 풀렸을 때 전광판 문구")]
    [SerializeField] private string solvedText = "성공";

    #endregion


    #region 공유 상태

    /// <summary>진행 중인 단계. 0부터. stages.Length면 완료</summary>
    [Networked, OnChangedRender(nameof(RaiseChanged))]
    public int Step { get; private set; }

    /// <summary>단계 진행</summary>
    public event System.Action Changed;

    private void RaiseChanged()
    {
        Changed?.Invoke();
    }

    #endregion


    #region 2D/3D가 매 프레임 읽는 값

    /// <summary>전 단계 통과</summary>
    public bool Solved => Step >= stages.Length;

    /// <summary>전체 단계 수</summary>
    public int StageCount => stages.Length;

    /// <summary>전체 막대 수</summary>
    public int BarCount => stages.Length * BarsPerStage;

    /// <summary>이번 단계 정답 막대의 전역 인덱스</summary>
    public int AnswerBar => Solved ? -1 : Step * BarsPerStage + (stages[Step].answer - 1);

    /// <summary>이번 단계 정답 막대인지</summary>
    public bool IsAnswer(int bar) => !Solved && bar == AnswerBar;

    /// <summary>이번 단계 숫자를 띄우는 쪽</summary>
    public PuzzleDimension HintSide =>
        stages.Length == 0
            ? PuzzleDimension.TwoD
            : stages[Mathf.Clamp(Step, 0, stages.Length - 1)].hintSide;

    /// <summary>이번 단계 막대를 누르는 쪽. 힌트 쪽의 반대</summary>
    public PuzzleDimension ActorSide =>
        HintSide == PuzzleDimension.TwoD ? PuzzleDimension.ThreeD : PuzzleDimension.TwoD;


    /// <summary>초록불 판정. 지나간 단계의 정답 막대</summary>
    public bool IsBarLit(int bar)
    {
        if (bar < 0 || bar >= BarCount) return false;

        int stage = bar / BarsPerStage;
        if (stage >= Step) return false;

        return bar % BarsPerStage == stages[stage].answer - 1;
    }


    /// <summary>클릭 허용 범위. 이번 단계 막대 4개</summary>
    public bool IsCurrentStageBar(int bar)
    {
        if (Solved) return false;
        if (bar < 0 || bar >= BarCount) return false;

        return bar / BarsPerStage == Step;
    }


    /// <summary>내 전광판 문구. 힌트 쪽이면 숫자, 아니면 상대 차원 이름</summary>
    public string BoardTextFor(PuzzleDimension me)
    {
        if (Solved) return solvedText;
        if (me == HintSide) return stages[Step].answer.ToString();

        return NameOf(HintSide);
    }

    private static string NameOf(PuzzleDimension dimension)
        => dimension == PuzzleDimension.TwoD ? "2D" : "3D";

    #endregion


    #region 2D/3D가 부르는 API

    /// <summary>정답 막대 입력. bar는 전역 인덱스 0~19. 판정과 오답 연출은 뷰에서 IsAnswer로 처리</summary>
    public void PressBar(int bar)
    {
        RpcPressBar(bar);
    }

    #endregion


    #region Fusion RPC

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcPressBar(int bar)
    {
        // 연타 중복 방지
        if (IsAnswer(bar)) Step++;
    }


    /// <summary>디버그 전용 단계 대입. 스폰하지 않은 쪽도 쓰도록 RPC</summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcDebugSetStep(int step)
    {
        Step = Mathf.Clamp(step, 0, stages.Length);
    }

    #endregion


    #region 한쪽 테스트 디버그 메뉴

    [ContextMenu("디버그: 현재 단계 정답")]
    private void DebugPressAnswer()
    {
        if (Solved) return;

        PressBar(AnswerBar);
    }


    [ContextMenu("디버그: 전부 통과")]
    private void DebugSolve()
    {
        RpcDebugSetStep(stages.Length);
    }


    [ContextMenu("디버그: 처음으로")]
    private void DebugReset()
    {
        RpcDebugSetStep(0);
    }


    /// <summary>정답표</summary>
    [ContextMenu("디버그: 정답표 로그")]
    private void DebugLogAnswers()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[{nameof(SignalBarPuzzle)}] 막대 {BarCount}개 / {stages.Length}단계 / 현재 {Step}단계");

        for (int i = 0; i < stages.Length; i++)
        {
            SignalBarStage s = stages[i];
            PuzzleDimension actor =
                s.hintSide == PuzzleDimension.TwoD
                    ? PuzzleDimension.ThreeD
                    : PuzzleDimension.TwoD;

            sb.AppendLine(
                $"  {i + 1}단계  숫자 {s.answer}  힌트 {NameOf(s.hintSide)}  " +
                $"누르는 쪽 {NameOf(actor)}  전역 막대 {i * BarsPerStage + (s.answer - 1)}"
            );
        }

        Debug.Log(sb.ToString());
    }

    #endregion
}
