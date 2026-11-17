using UnityEngine;
using UnityEngine.UI;

public class Ball2D10 : MonoBehaviour
{
    private Image ballImage;

    public enum BallColor
    {
        Purple,
        Orange,
        Green
    }

    public BallColor ballColor;

    private void Awake()
    {
        ballImage = GetComponent<Image>();
    }

    void Start()
    {
        SetState(ballColor);
    }
    public void SetState(BallColor color)
    {
        ballColor = color;

        switch (color)
        {
            case BallColor.Purple:
                ballImage.color = new Color32(140, 0, 255, 255); // #8C00FF (purple)
                break;

            case BallColor.Orange:
                ballImage.color = new Color32(251, 132, 40, 255); // #FB8428 (orange)
                break;

            case BallColor.Green:
                ballImage.color = new Color32(0, 112, 35, 255); // #007023 (green)
                break;
        }
    }

    public BallColor GetColor()
    {
        return ballColor;
    }
}