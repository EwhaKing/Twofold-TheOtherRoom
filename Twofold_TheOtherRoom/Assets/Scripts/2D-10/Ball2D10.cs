using UnityEngine;
using UnityEngine.UI;

public class Ball2D10 : MonoBehaviour
{
    private Image ballImage;

    public enum BallColor
    {
        Red,
        Yellow,
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
            case BallColor.Red:
                ballImage.color = new Color32(255, 0, 0, 255);
                break;

            case BallColor.Yellow:
                ballImage.color = new Color32(255, 255, 0, 255);
                break;

            case BallColor.Green:
                ballImage.color = new Color32(0, 255, 0, 255);
                break;
        }
    }

    public BallColor GetColor()
    {
        return ballColor;
    }
}