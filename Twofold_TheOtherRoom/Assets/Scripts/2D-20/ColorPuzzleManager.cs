using UnityEngine;
using UnityEngine.UI;

public class ColorPuzzleManager : MonoBehaviour
{
    [Header("붓")]
    [SerializeField] private Image brushImage;

    [Header("그림")]
    [SerializeField] private Image sunImage;
    [SerializeField] private Image cloudImage;
    [SerializeField] private Image grassImage;
    [SerializeField] private Image houseImage;

    [Header("정답 화면")]
    [SerializeField] private GameObject emptyCanvas;

    private readonly Color32 redColor = new Color32(248, 56, 56, 255);
    private readonly Color32 orangeColor = new Color32(243, 146, 16, 255);
    private readonly Color32 yellowColor = new Color32(253, 235, 37, 255);
    private readonly Color32 greenColor = new Color32(40, 174, 23, 255);
    private readonly Color32 blueColor = new Color32(11, 51, 175, 255);
    private readonly Color32 purpleColor = new Color32(67, 4, 135, 255);

    private readonly Color32 sunAnswer = new Color32(11, 51, 175, 255);
    private readonly Color32 cloudAnswer = new Color32(243, 146, 16, 255);
    private readonly Color32 grassAnswer = new Color32(67, 4, 135, 255);
    private readonly Color32 houseAnswer = new Color32(253, 235, 37, 255);

    private Color32 currentBrushColor;
    private bool hasSelectedColor = false;
    private bool solved = false;

    public bool IsSolved => solved;

    private void Start()
    {
        emptyCanvas.SetActive(false);
    }

    public void SelectRed()
    {
        SelectColor(redColor);
    }

    public void SelectOrange()
    {
        SelectColor(orangeColor);
    }

    public void SelectYellow()
    {
        SelectColor(yellowColor);
    }

    public void SelectGreen()
    {
        SelectColor(greenColor);
    }

    public void SelectBlue()
    {
        SelectColor(blueColor);
    }

    public void SelectPurple()
    {
        SelectColor(purpleColor);
    }

    private void SelectColor(Color32 color)
    {
        currentBrushColor = color;
        hasSelectedColor = true;

        brushImage.color = color;
    }

    public void PaintSun()
    {
        PaintObject(sunImage);
    }

    public void PaintCloud()
    {
        PaintObject(cloudImage);
    }

    public void PaintGrass()
    {
        PaintObject(grassImage);
    }

    public void PaintHouse()
    {
        PaintObject(houseImage);
    }

    private void PaintObject(Image target)
    {
        if (!hasSelectedColor || solved)
            return;

        target.color = currentBrushColor;

        CheckPuzzle();
    }

    private void CheckPuzzle()
    {
        bool sunCorrect = (Color32)sunImage.color == sunAnswer;
        bool cloudCorrect = (Color32)cloudImage.color == cloudAnswer;
        bool grassCorrect = (Color32)grassImage.color == grassAnswer;
        bool houseCorrect = (Color32)houseImage.color == houseAnswer;

        if (sunCorrect && cloudCorrect && grassCorrect && houseCorrect)
        {
            solved = true;

            emptyCanvas.SetActive(true);

            Debug.Log("finished");
        }
    }
}