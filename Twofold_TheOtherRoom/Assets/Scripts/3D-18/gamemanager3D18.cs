
using UnityEngine;
using TMPro;

public class gamemanager3D18 : MonoBehaviour
{
    [Header("Sticks")]
    [SerializeField] private stick3d18[] sticks;

    [Header("Board")]
    [SerializeField] private TMP_Text boardText;

    private Puzzle18 puzzle;
    private bool reportedSolved = false;

    private void Awake()
    {
        if (sticks == null || sticks.Length != 20)
        {
            Debug.LogError("[3D-18] 막대 20개를 연결해 주세요!");
            return;
        }

        for (int i = 0; i < sticks.Length; i++)
        {
            if (sticks[i] == null)
            {
                Debug.LogWarning($"[3D-18] Element {i}가 비어 있습니다!");
                continue;
            }

            sticks[i].Setup(this, i);
        }
    }
    

    private void Update()
    {
        // 2D가 Spawn한 공유 퍼즐 찾기
        if (puzzle == null)
        {
            puzzle = CoopPuzzle.Find<Puzzle18>(Puzzle18.Key);

            if (puzzle == null)
                return;
        }

        UpdateBoard();
        UpdateSticks();
        CheckSolved();
    }

    // 막대에서 E키를 눌렀을 때 호출
    public void TryPressStick(int index)
    {
        if (puzzle == null || puzzle.Solved)
            return;

        // 유효한 막대 번호인지 확인
        if (index < 0 || index >= puzzle.BarCount)
            return;

        // 3D 차례인지 확인
        if (puzzle.ActorSide != PuzzleDimension.ThreeD)
            return;

        // 현재 단계 막대인지 확인
        if (!puzzle.IsCurrentStageBar(index))
            return;

        // 정답 판정은 공유 Puzzle18이 담당
        if (puzzle.IsAnswer(index))
        {
            puzzle.PressBar(index);

            Debug.Log($"[3D-18] 정답! Stick {index}");
        }
        else
        {
            Debug.Log($"[3D-18] 오답! Stick {index}");

            // 실제 StickIndex로 막대 찾기
            if (sticks == null) return;

            foreach (var stick in sticks)
            {
                if (stick == null) continue;

                if (stick.StickIndex == index)
                {
                    stick.BlinkWrong();
                    break;
                }
            }
        }
    }

    // 전광판 갱신
    private void UpdateBoard()
    {
        if (boardText == null) return;

        boardText.text =
            puzzle.BoardTextFor(PuzzleDimension.ThreeD);
    }

    // 공유 퍼즐의 불빛 상태 반영
    private void UpdateSticks()
    {
        if (sticks == null) return;

        foreach (var stick in sticks)
        {
            if (stick == null) continue;

            stick.SetLit(
                puzzle.IsBarLit(stick.StickIndex)
            );
        }
    }

    // 퍼즐 완료 처리
    private void CheckSolved()
    {
        if (!puzzle.Solved)
        {
            // 디버그 초기화 후 다시 완료 가능하도록
            reportedSolved = false;
            return;
        }

        if (reportedSolved)
            return;

        if (PuzzleManager.Instance == null)
            return;

        PuzzleManager.Instance.ReportSolved(
            Puzzle18.Id3D,
            PuzzleDimension.ThreeD
        );

        reportedSolved = true;

        Debug.Log("[3D-18] 퍼즐 완료!");
    }
}
