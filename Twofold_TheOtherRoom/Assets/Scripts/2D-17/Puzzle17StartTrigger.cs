using UnityEngine;
using UnityEngine.EventSystems;

public class Puzzle17StartTrigger : MonoBehaviour, IPointerClickHandler
{
    private Puzzle17 puzzle;

    private void OnEnable()
    {
        CoopPuzzle.OnRegistered += BindPuzzle;
        puzzle = CoopPuzzle.Find<Puzzle17>(Puzzle17.Id);
    }

    private void OnDisable()
    {
        CoopPuzzle.OnRegistered -= BindPuzzle;
    }

    private void BindPuzzle(CoopPuzzle coopPuzzle)
    {
        if (coopPuzzle is Puzzle17 puzzle17)
        {
            puzzle = puzzle17;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (puzzle == null)
        {
            puzzle = CoopPuzzle.Find<Puzzle17>(Puzzle17.Id);
        }

        // 3D 밸브가 아직 눌리지 않았으면 시작하지 않음
        if (puzzle == null || !puzzle.Activated)
        {
            Debug.Log(
                "Puzzle17StartTrigger: 아직 3D 밸브가 눌리지 않았습니다."
            );

            return;
        }

        // 이미 시작된 퍼즐이면 다시 시작하지 않음
        if (puzzle.Started)
            return;

        // 이 순간부터 60초 시작
        puzzle.StartPuzzle();

        Debug.Log(
            "Puzzle17StartTrigger: 2D-17 제한시간 시작"
        );
    }
}