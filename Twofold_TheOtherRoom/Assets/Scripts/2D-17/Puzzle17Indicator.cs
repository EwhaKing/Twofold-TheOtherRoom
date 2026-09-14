using UnityEngine;
using UnityEngine.UI;

public class Puzzle17Indicator : MonoBehaviour
{
    [Header("2D-17_display")]
    [SerializeField] private Graphic targetGraphic;

    [SerializeField] private float blinkSpeed = 3f;

    private Puzzle17 puzzle;

    private void OnEnable()
    {
        CoopPuzzle.OnRegistered += BindPuzzle;
        puzzle = CoopPuzzle.Find<Puzzle17>(Puzzle17.Key);
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

    private void Update()
    {
        if (targetGraphic == null)
            return;

        bool shouldBlink =
            puzzle != null &&
            puzzle.Activated &&
            !puzzle.Started;

        Color color = targetGraphic.color;

        if (shouldBlink)
        {
            color.a =
                0.5f +
                Mathf.Sin(Time.time * blinkSpeed) * 0.5f;
        }
        else
        {
            color.a = 1f;
        }

        targetGraphic.color = color;
    }
}