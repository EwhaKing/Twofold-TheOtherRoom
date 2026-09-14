using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Pipe17PuzzleManager : MonoBehaviour
{
    [Header("START → END 정답 파이프(순서대로)")]
    [SerializeField] private Pipe17Piece[] answerPath;

    [Header("시작 / 끝 파이프")]
    [SerializeField] private Image pipeStartImage;
    [SerializeField] private Image pipeEndImage;

    [Header("연결됐을 때 색")]
    [SerializeField]
    private Color connectedColor =
        new Color(0.2f, 0.65f, 1f, 1f);

    [Header("최소 틀린 파이프 수")]
    [SerializeField] private int minimumWrongPipes = 6;

    private Pipe17Piece[] allPipes;

    private Color pipeStartOriginalColor;
    private Color pipeEndOriginalColor;

    private bool solved;
    public bool IsSolved => solved;

    private void Start()
    {
        allPipes =
            GetComponentsInChildren<Pipe17Piece>(true);

        if (pipeStartImage != null)
        {
            pipeStartOriginalColor = pipeStartImage.color;
        }

        if (pipeEndImage != null)
        {
            pipeEndOriginalColor = pipeEndImage.color;
        }

        RandomizePipes();

        UpdateFlowColor();
    }

    private void RandomizePipes()
    {
        foreach (Pipe17Piece pipe in allPipes)
        {
            pipe.SetRandomRotation();
        }

        int wrongCount = 0;

        List<Pipe17Piece> correctPipes =
            new List<Pipe17Piece>();

        foreach (Pipe17Piece pipe in answerPath)
        {
            if (pipe == null)
                continue;

            if (pipe.IsCorrect())
            {
                correctPipes.Add(pipe);
            }
            else
            {
                wrongCount++;
            }
        }

        int needToMakeWrong =
            minimumWrongPipes - wrongCount;

        if (needToMakeWrong <= 0)
            return;

        Shuffle(correctPipes);

        for (
            int i = 0;
            i < needToMakeWrong &&
            i < correctPipes.Count;
            i++
        )
        {
            correctPipes[i].Rotate90();
        }
    }

    private void Shuffle(
        List<Pipe17Piece> list
    )
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex =
                Random.Range(i, list.Count);

            Pipe17Piece temp =
                list[i];

            list[i] =
                list[randomIndex];

            list[randomIndex] =
                temp;
        }
    }

    public void RefreshPuzzle()
    {
        UpdateFlowColor();
        CheckPuzzle();
    }

    private void UpdateFlowColor()
    {
        foreach (Pipe17Piece pipe in allPipes)
        {
            pipe.SetConnected(false);
        }

        if (pipeStartImage != null)
        {
            pipeStartImage.color = connectedColor;
        }

        if (pipeEndImage != null)
        {
            pipeEndImage.color = pipeEndOriginalColor;
        }

        bool allConnected = true;

        foreach (Pipe17Piece pipe in answerPath)
        {
            if (pipe == null)
                continue;

            if (!pipe.IsCorrect())
            {
                allConnected = false;
                break;
            }

            pipe.SetConnected(true);
        }

        if (allConnected && pipeEndImage != null)
        {
            pipeEndImage.color = connectedColor;
        }
    }

    private void CheckPuzzle()
    {
        if (solved)
            return;

        foreach (Pipe17Piece pipe in answerPath)
        {
            if (pipe == null)
                continue;

            if (!pipe.IsCorrect())
            {
                return;
            }
        }

        solved = true;

        Debug.Log(
            "finished"
        );

        Puzzle17 puzzle =
            CoopPuzzle.Find<Puzzle17>(
                Puzzle17.Id
            );

        if (puzzle != null)
        {
            puzzle.ReportConnected();
        }
    }
}