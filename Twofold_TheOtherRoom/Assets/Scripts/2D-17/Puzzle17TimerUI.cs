using UnityEngine;
using UnityEngine.UI;

public class Puzzle17TimerUI : MonoBehaviour
{
    [SerializeField] private Image timerFill;

    private Puzzle17 puzzle;

    private void OnEnable()
    {
        CoopPuzzle.OnRegistered += BindPuzzle;
        puzzle = CoopPuzzle.Find<Puzzle17>(Puzzle17.Key);
    }

    private void Start()
    {
        TryStartPuzzle();
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
            TryStartPuzzle();
        }
    }

    private void TryStartPuzzle()
    {
        if (puzzle == null)
        {
            puzzle = CoopPuzzle.Find<Puzzle17>(Puzzle17.Key);
        }

        if (puzzle == null)
        {
            Debug.LogWarning(
                "Puzzle17TimerUI: Puzzle17을 찾을 수 없습니다."
            );
            return;
        }

        if (!puzzle.Activated)
        {
            Debug.Log(
                "Puzzle17TimerUI: 아직 3D 밸브가 눌리지 않았습니다."
            );
            return;
        }

        if (puzzle.Started)
        {
            return;
        }

        puzzle.StartPuzzle();

        Debug.Log(
            "Puzzle17TimerUI: 2D-17 타이머 시작"
        );
    }

    private void Update()
    {
        if (timerFill == null)
            return;

        if (puzzle == null)
        {
            puzzle = CoopPuzzle.Find<Puzzle17>(Puzzle17.Key);

            if (puzzle == null)
            {
                timerFill.fillAmount = 1f;
                return;
            }
        }

        if (puzzle.Activated && !puzzle.Started)
        {
            TryStartPuzzle();
        }

        if (!puzzle.Started)
        {
            timerFill.fillAmount = 1f;
            return;
        }

        timerFill.fillAmount = puzzle.WaterFill;
    }
}