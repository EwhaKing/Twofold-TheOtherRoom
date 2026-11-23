using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle18View2D : MonoBehaviour
{
    [SerializeField] private RectTransform bars;
    [SerializeField] private TMP_Text boardText;
    [SerializeField] private GameObject mirrorPiece;

    [SerializeField] private Color idleColor = new Color32(125, 125, 125, 255);
    [SerializeField] private Color currentColor = new Color32(190, 190, 190, 255);
    [SerializeField] private Color litColor = new Color32(153, 198, 77, 255);
    [SerializeField] private Color wrongColor = new Color32(200, 80, 80, 255);

    private Button[] buttons;
    private Image[] fills;
    private Puzzle18 puzzle;
    private int wrongBar = -1;
    private float wrongUntil;

    private void Awake()
    {
        if (bars == null || boardText == null || bars.childCount == 0)
        {
            Debug.LogError("2D-18: Bars와 Board Text를 연결해 주세요.", this);
            enabled = false;
            return;
        }

        buttons = new Button[bars.childCount];
        fills = new Image[bars.childCount];

        for (int i = 0; i < bars.childCount; i++)
        {
            Transform bar = bars.GetChild(i);
            Transform fill = bar.Find("Fill");
            buttons[i] = bar.GetComponent<Button>();
            fills[i] = fill != null ? fill.GetComponent<Image>() : null;

            if (bar.name != $"Bar_{i:00}" || buttons[i] == null || fills[i] == null)
            {
                Debug.LogError($"2D-18: Bar_{i:00}의 순서, Button, Fill을 확인해 주세요.", this);
                enabled = false;
                return;
            }
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].interactable = false;
            buttons[i].onClick.AddListener(() => OnBarClicked(index));
        }

        boardText.text = "";
        if (mirrorPiece != null)
            mirrorPiece.SetActive(false);
    }

    private void Update()
    {
        if (puzzle == null)
        {
            puzzle = CoopPuzzle.Find<Puzzle18>(Puzzle18.Key);
            if (puzzle == null)
                return;

            if (buttons.Length != puzzle.BarCount)
            {
                Debug.LogError(
                    $"2D-18: 화면에 막대 {buttons.Length}개, 공유 퍼즐에 {puzzle.BarCount}개가 있어요.",
                    this);
                enabled = false;
                return;
            }
        }

        bool solved = puzzle.Solved;
        bool myTurn = !solved && puzzle.ActorSide == PuzzleDimension.TwoD;

        boardText.text = solved && mirrorPiece != null
            ? ""
            : puzzle.BoardTextFor(PuzzleDimension.TwoD);

        for (int i = 0; i < buttons.Length; i++)
        {
            bool current = puzzle.IsCurrentStageBar(i);
            buttons[i].interactable = myTurn && current;

            if (puzzle.IsBarLit(i))
                fills[i].color = litColor;
            else if (myTurn && current && i == wrongBar && Time.unscaledTime < wrongUntil)
                fills[i].color = wrongColor;
            else
                fills[i].color = current ? currentColor : idleColor;
        }

        if (mirrorPiece != null && mirrorPiece.activeSelf != solved)
            mirrorPiece.SetActive(solved);

        PuzzleManager manager = PuzzleManager.Instance;
        if (solved && manager != null && !manager.IsSolved(Puzzle18.Id2D))
            manager.ReportSolved(Puzzle18.Id2D, PuzzleDimension.TwoD);
    }

    private void OnBarClicked(int index)
    {
        if (puzzle == null || puzzle.Solved ||
            puzzle.ActorSide != PuzzleDimension.TwoD ||
            !puzzle.IsCurrentStageBar(index))
            return;

        if (!puzzle.IsAnswer(index))
        {
            wrongBar = index;
            wrongUntil = Time.unscaledTime + 0.2f;
            return;
        }

        wrongBar = -1;
        puzzle.PressBar(index);
    }
}